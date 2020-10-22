using Common;
using Microsoft.Extensions.Logging;
using RestSharp;
using System;

namespace LogParser
{
    internal class IpGeoApiProvider : IpLookupProviderBase
    {
        private const string _baseUrl = "https://api.ipgeolocationapi.com/geolocate/";
        private readonly RestClient _client;

        public override int Priority => 1;

        public IpGeoApiProvider(ILogger<IpGeoApiProvider> logger)
            : base(logger)
        {
            _client = new RestClient
            {
                BaseUrl = new UriBuilder(_baseUrl) { Scheme = Uri.UriSchemeHttps, Port = -1 }.Uri
            };
        }

        protected override void LookupInner(HostInfo record)
        {
            var request = new RestRequest();
            var ip = GetIp(record.Host);
            if (ip == null)
            {
                _logger.LogTrace($"Skipped host `{record.Host}` - IP address not found.");
                return;
            }

            request.AddUrlSegment("IpAddress", ip);
            request.Resource = "{IpAddress}";
            var response = _client.Execute<dynamic>(request, Method.GET);
            if ((int)response.StatusCode >= 400 && (int)response.StatusCode < 500)
            {
                _logger.LogTrace($"Skipped host `{record.Host}` - response returned code {response.StatusCode}.");
                return;
            }
            if ((int)response.StatusCode >= 500 || response.ErrorException != null)
            {
                throw new ApplicationException(string.IsNullOrEmpty(response.ErrorMessage) ? "An unexpected error occurred on the server side." : response.ErrorMessage, response.ErrorException);
            }
            record.Geolocation = response.Data["name"];
        }
    }
}
