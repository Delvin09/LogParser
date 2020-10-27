using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Common
{
    public interface ILogRepository
    {
        Task AddLogs(IEnumerable<LogRecord> records);
        Task<IEnumerable<HostInfo>> GetHostsWithoutLocation();
        Task UpdateGeolocations(IEnumerable<HostInfo> origin);
        Task<IEnumerable<string>> GetFrequentlyHosts(int topCount, DateTime? start, DateTime? end);
        Task<IEnumerable<string>> GetFrequentlyRoutes(int topCount, DateTime? start, DateTime? end);
        Task<IEnumerable<LogRecord>> GetLogs(int offset, int limit, DateTime? start, DateTime? end);
    }
}
