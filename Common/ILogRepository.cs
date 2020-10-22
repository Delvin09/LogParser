using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Common
{
    public interface ILogRepository
    {
        void Insert(IEnumerable<LogRecord> records);
        IEnumerable<HostInfo> GetHostsWithoutLocation();
        void UpdateGeolocations(IEnumerable<HostInfo> origin);
        Task<IEnumerable<string>> GetFrequentlyHosts(int topCount, DateTime? start, DateTime? end);
        Task<IEnumerable<string>> GetFrequentlyRoutes(int topCount, DateTime? start, DateTime? end);
        Task<IEnumerable<LogRecord>> GetLogs(int offset, int limit, DateTime? start, DateTime? end);
    }
}
