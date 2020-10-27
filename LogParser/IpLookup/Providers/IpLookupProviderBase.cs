using Common;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace LogParser
{
    public abstract class IpLookupProviderBase : IIpLookupProvider
    {
        public const string NotFound = "Unknow";

        protected readonly ILogger _logger;
        private readonly int _lookupBusSize;

        public abstract int Priority { get; }

        public IpLookupProviderBase(ILogger logger, IConfiguration configuration)
        {
            _logger = logger;
            _lookupBusSize = configuration.GetValue("ipLookupBusSize", 300);
        }

        public async Task<IEnumerable<HostInfo>> Lookup(IEnumerable<HostInfo> hosts)
        {
            var stopwatch = Stopwatch.StartNew();
            _logger.LogTrace($"Start provider lookup at {DateTime.Now}.");
            try
            {
                int page = 0;
                IEnumerable<HostInfo> result = Enumerable.Empty<HostInfo>();
                
                var hostsInPage = hosts.Skip(page * _lookupBusSize).Take(_lookupBusSize);
                while (hostsInPage.Any())
                {
                    result = result.Concat(await Task.WhenAll(hostsInPage
                            .Select(h => LookupInner(h).ContinueWith(t => t.IsFaulted || t.Exception != null ? h : null)).ToArray()));

                    page++;
                    hostsInPage = hosts.Skip(page * _lookupBusSize).Take(_lookupBusSize);
                }
                return result.Where(h => h != null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lookup provider threw an exception. Perhaps provider is offline or blocked by request limitation.");
                return hosts.Where(h => string.IsNullOrEmpty(h.Geolocation));
            }
            finally
            {
                stopwatch.Stop();
                _logger.LogTrace($"End provider lookup at {DateTime.Now}. Elapsed: {stopwatch.Elapsed}");
            }
        }

        protected abstract Task LookupInner(HostInfo current);

        protected async Task<string> GetIp(string host)
        {
            try
            {
                if (IPAddress.TryParse(host, out IPAddress address))
                    return host;

                var ips = await Dns.GetHostAddressesAsync(host);
                return ips.FirstOrDefault(ip => !IPAddress.IsLoopback(ip) && ip != IPAddress.Any && ip != IPAddress.Broadcast)?.ToString();
            }
            catch
            {
                return null;
            }
        }
    }
}
