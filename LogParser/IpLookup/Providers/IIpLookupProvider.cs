using Common;
using System.Collections.Generic;

namespace LogParser
{
    public interface IIpLookupProvider
    {
        int Priority { get; }

        IEnumerable<HostInfo> Lookup(IEnumerable<HostInfo> hosts);
    }
}
