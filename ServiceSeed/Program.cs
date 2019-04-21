using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.ApplicationInsights;

namespace ServiceSeed
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            ILogger logger = null;
            try
            {
                var host = CreateWebHostBuilder(args).Build();
                logger = host.Services.GetRequiredService<ILogger<Program>>();
                logger?.LogInformation("Service starting...");
                await host.RunAsync();
                logger?.LogInformation("Service ended.");

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                logger?.LogCritical(ex, "Service terminated unexpectedly.");
            }
            finally
            {
                await Task.Delay(10000);
            }
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((builderContext, config) =>
                {
                    config.SetBasePath(builderContext.HostingEnvironment.ContentRootPath)
                        .AddJsonFile($"appsettings.json", optional: false, reloadOnChange: false)
                        .AddEnvironmentVariables();
                })
                .ConfigureLogging((hostContext, configLogging) =>
                {
                    configLogging.AddConfiguration(hostContext.Configuration.GetSection("Logging"));
                    configLogging.AddConsole((options) =>
                    {
                        options.IncludeScopes = Convert.ToBoolean(hostContext.Configuration["Logging:IncludeScopes"]);
                    });
                    configLogging.AddApplicationInsights(hostContext.Configuration["Logging:ApplicationInsights:Instrumentationkey"]);

                    // Optional: Apply filters to configure LogLevel Trace or above is sent to
                    // ApplicationInsights for all categories.
                    configLogging.AddFilter<ApplicationInsightsLoggerProvider>("", LogLevel.Trace);

                    // Additional filtering For category starting in "Microsoft",
                    // only Warning or above will be sent to Application Insights.
                    configLogging.AddFilter<ApplicationInsightsLoggerProvider>("Microsoft", LogLevel.Warning);

                })
                .UseStartup<Startup>();
    }
}
