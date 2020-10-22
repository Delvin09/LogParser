using Common;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace LogParser
{
    public abstract class IpLookupProviderBase : IIpLookupProvider
    {
        protected readonly ILogger _logger;

        public abstract int Priority { get; }

        public IpLookupProviderBase(ILogger logger)
        {
            _logger = logger;
        }

        public IEnumerable<HostInfo> Lookup(IEnumerable<HostInfo> hosts)
        {
            _logger.LogDebug("Start provider lookup.");

            var recordsEnumerator = hosts.GetEnumerator();
            while (recordsEnumerator.MoveNext())
            {
                try
                {
                    LookupInner(recordsEnumerator.Current);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Lookup provider threw an exception. Perhaps provider is offline or blocked by request limitation.");
                    return EnumerateTail(recordsEnumerator);
                }
            }

            _logger.LogDebug("End provider lookup.");
            return Enumerable.Empty<HostInfo>();
        }

        protected abstract void LookupInner(HostInfo current);

        protected IEnumerable<T> EnumerateTail<T>(IEnumerator<T> tail)
        {
            while (tail.MoveNext()) yield return tail.Current;
        }

        protected string GetIp(string host)
        {
            try
            {
                var ips = Dns.GetHostAddresses(host);
                return ips.FirstOrDefault()?.ToString();
            }
            catch
            {
                return null;
            }
        }
    }
}
