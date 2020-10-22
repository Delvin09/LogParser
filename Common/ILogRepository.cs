using System.Collections.Generic;

namespace Common
{
    public interface ILogRepository
    {
        //void Insert(LogRecord record);
        void Insert(IEnumerable<LogRecord> records);
        IEnumerable<HostInfo> GetHostsWithoutLocation();
        void UpdateGeolocations(IEnumerable<HostInfo> origin);
    }
}
