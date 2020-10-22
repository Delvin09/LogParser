using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace Common.Extensions
{
    public static class CommonServicesExtensions
    {
        public static IServiceCollection AddCommonServices(this IServiceCollection serviceCollection)
            => serviceCollection.AddTransient<ILogRepository, LogRepository>();
    }
}
