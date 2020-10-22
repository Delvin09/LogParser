using Common.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.IO;

namespace LogParser
{
    internal class Application
    {
        private readonly ILogParser _logParser;
        private readonly ILogger<Application> _logger;
        private readonly IIpLookupService _ipLookupService;

        public Application(ILogParser logParser, IIpLookupService ipLookupService, ILogger<Application> logger)
        {
            _logParser = logParser;
            _logger = logger;
            _ipLookupService = ipLookupService;
        }

        public static Application Create(string[] args)
            => ConfigureServices(args).GetService<Application>();

        private static IServiceProvider ConfigureServices(string[] args)
            => new ServiceCollection()
                .AddSingleton<IConfiguration>(CreateConfiguration(args))
                .AddLogging(opt => {
                    var conf = opt.Services.BuildServiceProvider().GetService<IConfiguration>();
                    opt.AddConfiguration(conf.GetSection("Logging"));
                    opt.AddConsole();
                    opt.AddDebug();
                 })
                .AddCommonServices()
                .AddSingleton<Application>()
                .AddTransient<ILogParser, LogParser>()
                .AddSingleton<IIpLookupService, IpLookupService>()
                .AddTransient<IIpLookupProvider, IpGeoApiProvider>()
                .AddTransient<IIpLookupProvider, IpStackProvider>()
                .BuildServiceProvider();

        private static IConfigurationRoot CreateConfiguration(string[] args)
            => new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", false, true)
                .AddCommandLine(args)
                .Build();

        public void Run()
        {
            try
            {
                _logParser.Parse();
                _ipLookupService.Lookup();
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "An error occurred while the program was running");
            }
        }
    }
}
