using Common;
using IpStack;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace LogParser
{
    internal class IpStackProvider : IpLookupProviderBase
    {
        // for simplicity, I put it here and not in secret
        private const string ApiKey = "7253b1daabbbcdf1596d07dad910b7";
        private readonly IpStackClient _client;

        public override int Priority => 5;

        public IpStackProvider(ILogger<IpStackProvider> logger, IConfiguration configuration)
            : base(logger, configuration)
        {
            _client = new IpStackClient(ApiKey);
        }

        protected async override Task LookupInner(HostInfo record)
        {
            var ip = await GetIp(record.Host);
            if (ip == null)
            {
                record.Geolocation = NotFound;
                _logger.LogTrace($"Skipped host `{record.Host}` - IP address not found.");
                return;
            }

            var response = _client.GetIpAddressDetails(ip);
            record.Geolocation = response?.CountryName ?? NotFound;
        }
    }
}
