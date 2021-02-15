using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using WordPress.Crawler.Shared.Services;

namespace WordPress.Crawler.Service.Fetcher
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> logger;
        private readonly IConfiguration configuration;
        private readonly IServiceScopeFactory serviceScopeFactory;
        //private readonly CrawlerService crawlerService;
        private string[] sitesToCrawl;

        public Worker(ILogger<Worker> _logger, IConfiguration _configuration, IServiceScopeFactory _serviceScopeFactory)
        {
            logger = _logger;
            configuration = _configuration;
            serviceScopeFactory = _serviceScopeFactory;
            //crawlerService = _crawlerService;
        }

        public override Task StartAsync(CancellationToken cancellationToken)
        {
            //crawlerService = new CrawlerService();
            sitesToCrawl = configuration["SitesToCrawl"].Split(",", StringSplitOptions.RemoveEmptyEntries);
            
            return base.StartAsync(cancellationToken);  
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var scope = serviceScopeFactory.CreateScope();
                    var crawlerService = scope.ServiceProvider.GetRequiredService<CrawlerService>();
                    var fetchTasks = sitesToCrawl.Select(s => crawlerService.FetchPostAsync(s));
                    await Task.WhenAll(fetchTasks);
                    logger.LogInformation("All done.");
                }
                catch (Exception ex)
                {
                    logger.LogCritical(ex, "An exception occurred while fetching post.");
                    continue;
                }
                await Task.Delay(3600000, stoppingToken);
            }
        }
    }
}
