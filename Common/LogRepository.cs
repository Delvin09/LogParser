using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using Dapper;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Common
{
    internal class LogRepository : ILogRepository
    {
        private readonly string _connectionString;

        public LogRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("LogDatabase") ?? throw new ArgumentNullException(nameof(_connectionString));
        }

        public async Task<IEnumerable<string>> GetFrequentlyHosts(int topCount, DateTime? start, DateTime? end)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                return await connection.QueryAsync<string>($@"SELECT TOP (@topCount) l.Host, Count(*) freq FROM [Log] l
    {GetTimeWhereCondition(start, end)}
    GROUP BY l.Host
    ORDER BY freq desc", new { topCount, start, end });
            }
        }

        public async Task<IEnumerable<string>> GetFrequentlyRoutes(int topCount, DateTime? start, DateTime? end)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                return await connection.QueryAsync<string>($@"SELECT TOP (@topCount) l.Route, Count(*) freq FROM [Log] l
    {GetTimeWhereCondition(start, end, "l.Route IS NOT NULL")}
    GROUP BY l.Route
    ORDER BY freq desc", new { topCount, start, end });
            }
        }

        public IEnumerable<HostInfo> GetHostsWithoutLocation()
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                return connection.Query<HostInfo>("SELECT DISTINCT Host FROM [Log] WHERE Geolocation IS NULL");
            }
        }

        public async Task<IEnumerable<LogRecord>> GetLogs(int offset, int limit, DateTime? start, DateTime? end)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                return await connection.QueryAsync<LogRecord>(@$"SELECT * FROM [Log] l
{GetTimeWhereCondition(start, end)}
ORDER BY RequestDateTime
OFFSET @offset ROW
FETCH NEXT @limit ROWS ONLY", new { offset, limit, start, end });
            }
        }

        private string GetTimeWhereCondition(DateTime? start, DateTime? end, string additionalCodition = null)
        {
            if (start == end && start == null)
                return !string.IsNullOrEmpty(additionalCodition) ? $"WHERE {additionalCodition}" : string.Empty;

            additionalCodition = string.IsNullOrEmpty(additionalCodition) ? string.Empty : additionalCodition + " AND";

            if (start == null)
                return $"WHERE {additionalCodition} l.RequestDateTime <= @end";

            if (end == null)
                return $"WHERE {additionalCodition} l.RequestDateTime >= @start";

            return $"WHERE {additionalCodition} l.RequestDateTime BETWEEN @start AND @end";
        }

        public void Insert(IEnumerable<LogRecord> records)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Execute("INSERT [Log] VALUES (@RequestDateTime, @Host, @Route, @QueryParameters, @ResultCode, @ResponseSize, @Geolocation)"
                    , records);
            }
        }

        public void UpdateGeolocations(IEnumerable<HostInfo> hostInfos)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Execute("UPDATE [Log] SET Geolocation = @Geolocation WHERE Host = @Host", hostInfos);
            }
        }
    }
}
