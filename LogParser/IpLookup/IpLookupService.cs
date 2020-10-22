using Common;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace LogParser
{
    internal class IpLookupService : IIpLookupService
    {
        private readonly IIpLookupProvider[] _providers;
        private readonly ILogRepository _logRepository;
        private readonly ILogger<IpLookupService> _logger;

        public IpLookupService(IEnumerable<IIpLookupProvider> providers, ILogRepository logRepository, ILogger<IpLookupService> logger)
        {
            _providers = providers.OrderBy(p => p.Priority).ToArray();
            _logRepository = logRepository;
            _logger = logger;
        }

        public void Lookup()
        {
            var stopwatch = Stopwatch.StartNew();
            _logger.LogInformation($"Start lookup geolocation.");

            var origin = _logRepository.GetHostsWithoutLocation();
            IEnumerable<HostInfo> tail = origin;
            foreach (var provider in _providers)
            {
                if (!tail.Any())
                    break;

                tail = provider.Lookup(tail);
            }
            _logger.LogDebug("Store geolocation data in db.");
            _logRepository.UpdateGeolocations(origin.Where(h => !string.IsNullOrEmpty(h.Geolocation)));

            stopwatch.Stop();
            _logger.LogInformation($"End lookup geolocation. Elapsed: {stopwatch.Elapsed}. Remaining tail: {tail.Count()}");
        }
    }
}
