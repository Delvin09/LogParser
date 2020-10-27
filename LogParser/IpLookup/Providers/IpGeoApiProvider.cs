using Common;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RestSharp;
using System;
using System.Threading.Tasks;

namespace LogParser
{
    internal class IpGeoApiProvider : IpLookupProviderBase
    {
        private const string _baseUrl = "https://api.ipgeolocationapi.com/geolocate/";
        private readonly RestClient _client;

        public override int Priority => 1;

        public IpGeoApiProvider(ILogger<IpGeoApiProvider> logger, IConfiguration configuration)
            : base(logger, configuration)
        {
            _client = new RestClient
            {
                BaseUrl = new UriBuilder(_baseUrl) { Scheme = Uri.UriSchemeHttps, Port = -1 }.Uri
            };
        }

        protected override async Task LookupInner(HostInfo record)
        {
            var ip = await GetIp(record.Host);
            if (ip == null)
            {
                record.Geolocation = NotFound;
                _logger.LogTrace($"Skipped host `{record.Host}` - IP address not found.");
                return;
            }

            var request = new RestRequest();
            request.AddUrlSegment("IpAddress", ip);
            request.Resource = "{IpAddress}";
            var response = await _client.ExecuteTaskAsync<dynamic>(request, Method.GET);
            if ((int)response.StatusCode >= 400 && (int)response.StatusCode < 500)
            {
                _logger.LogError($"Response returned code {response.StatusCode} for host {record.Host} with IP {ip}.");
                return;
            }
            if (response.StatusCode <= 0)
            {
                throw new ApplicationException($"An unexpected error occurred on the server side. IP = `{ip}`", response.ErrorException);
            }
            if ((int)response.StatusCode >= 500 || response.ErrorException != null)
            {
                _logger.LogWarning(response.ErrorException, $"An unexpected error occurred on the server side. IP = `{ip}`");
                record.Geolocation = NotFound;
                return;
            }

            try
            {
                string country = response.Data["name"];
                record.Geolocation = string.IsNullOrEmpty(country) ? NotFound : country;
            }
            catch (Exception ex)
            {
                record.Geolocation = NotFound;
                _logger.LogWarning(ex, $"An unexpected error occurred when responce processed. IP = `{ip}`");
            }
        }
    }
}
