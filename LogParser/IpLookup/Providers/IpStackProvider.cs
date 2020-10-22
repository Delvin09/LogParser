using Common;
using IpStack;
using Microsoft.Extensions.Logging;

namespace LogParser
{
    internal class IpStackProvider : IpLookupProviderBase
    {
        // for simplicity, I put it here and not in secret
        private const string ApiKey = "7253b1daabbbcdf1596d07dad910b7a2";
        private readonly IpStackClient _client;

        public override int Priority => 5;

        public IpStackProvider(ILogger<IpStackProvider> logger)
            : base(logger)
        {
            _client = new IpStackClient(ApiKey);
        }

        protected override void LookupInner(HostInfo record)
        {
            var ip = GetIp(record.Host);
            if (ip == null)
            {
                _logger.LogTrace($"Skipped host `{record.Host}` - IP address not found.");
                return;
            }

            var response = _client.GetIpAddressDetails(ip);
            record.Geolocation = response?.CountryName;
        }
    }
}
