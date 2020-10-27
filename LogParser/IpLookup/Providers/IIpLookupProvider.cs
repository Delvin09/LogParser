using Common;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LogParser
{
    public interface IIpLookupProvider
    {
        int Priority { get; }

        Task<IEnumerable<HostInfo>> Lookup(IEnumerable<HostInfo> hosts);
    }
}
