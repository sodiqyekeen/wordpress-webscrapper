using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog.Web;
using WordPress.Crawler.Shared.Data;
using WordPress.Crawler.Shared.Services;

namespace WordPress.Crawler.Service.Fetcher
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var logger = NLogBuilder.ConfigureNLog("NLog.config").GetCurrentClassLogger();

            try
            {
                logger.Debug("Inside main method, about to create host builder...");
                CreateHostBuilder(args).Build().Run();
            }
            catch (Exception ex)
            {
                logger.Fatal(ex, "An exception occurred prevent CreateHostBuilder()");
                throw;
            }
            finally
            {
                NLog.LogManager.Shutdown();
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseWindowsService()
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddDbContext<CrawlerDbContext>(option =>
                        option.UseSqlServer(hostContext.Configuration.GetConnectionString("CrawlerDbConnectionString"),
                                x => x.MigrationsAssembly("WordPress.Crawler.Shared")), ServiceLifetime.Transient);
                    services.AddScoped<CrawlerService>();
                    services.AddHostedService<Worker>();
                    services.AddHttpClient();
                })
            .ConfigureLogging(logger =>
            {
                logger.ClearProviders();
                logger.SetMinimumLevel(LogLevel.Trace);
            }).UseNLog();
    }
}
