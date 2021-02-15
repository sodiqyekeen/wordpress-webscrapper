using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using WordPress.Crawler.Shared.Services;

namespace WordPress.Crawler.Service.Publisher
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> logger;
        private readonly IServiceScopeFactory serviceScopeFactory;

        public Worker(ILogger<Worker> _logger, IServiceScopeFactory _serviceScopeFactory)
        {
            logger = _logger;
            serviceScopeFactory = _serviceScopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var scope = serviceScopeFactory.CreateScope();
                    var crawlerService = scope.ServiceProvider.GetRequiredService<CrawlerService>();
                    await crawlerService.PublishPost();
                    logger.LogInformation("All done.");
                }
                catch (Exception ex)
                {
                    logger.LogCritical(ex, "An exception occurred while fetching post.");
                    continue;
                }
                await Task.Delay(120000, stoppingToken);
            }
        }
    }
}
