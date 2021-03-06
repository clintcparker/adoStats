using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace adoStats_core
{
public static class ServiceContainer
    {
        public static IHost host {get;}
        static ServiceContainer()
        {
            var builder = new HostBuilder()
            .ConfigureServices((hostContext, services) =>
            {
                services.AddHttpClient();
                services.AddTransient<IAzureDevWebService, AzureDevWebService>();
                services.AddTransient<StatsService, StatsService>();
                services.AddTransient<StatsServiceFactory, StatsServiceFactory>();
                services.AddTransient<AzureDevWebUtilities, AzureDevWebUtilities>();
            }).UseConsoleLifetime();

            host = builder.Build();
        }
        
    }
}