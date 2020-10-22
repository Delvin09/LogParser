using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using Dapper;
using System.Diagnostics;
using System.Linq;

namespace Common
{
    internal class LogRepository : ILogRepository
    {
        private readonly string _connectionString;

        public LogRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("LogDatabase");
        }

        public IEnumerable<HostInfo> GetHostsWithoutLocation()
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                return connection.Query<HostInfo>("SELECT DISTINCT Host FROM [Log] WHERE Geolocation IS NULL");
            }
        }

        public void Insert(IEnumerable<LogRecord> records)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Execute("insert [Log] values (@RequestDateTime, @Host, @Route, @QueryParameters, @ResultCode, @ResponseSize)", records);
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
