using System;
using Hangfire;
using Hangfire.SqlServer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using WordPress.Crawler.Shared.Data;
using WordPress.Crawler.Shared.Services;


namespace WordPress.Crawler.UI
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<CrawlerDbContext>(option =>
            {
                option.UseSqlServer(Configuration.GetConnectionString("CrawlerDbConnectionString"), 
                    x => x.MigrationsAssembly("WordPress.Crawler.Shared"));
            });

            services.AddHangfire(options => options
               .SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
               .UseSimpleAssemblyNameTypeSerializer()
               .UseRecommendedSerializerSettings()
               .UseSqlServerStorage(Configuration.GetConnectionString("HangfireConnection"), new SqlServerStorageOptions
               {
                   CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
                   SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
                   QueuePollInterval = TimeSpan.Zero,
                   UseRecommendedIsolationLevel = true,
                   UsePageLocksOnDequeue = true,
                   DisableGlobalLocks = true
               }));

            services.AddRazorPages();
            services.AddServerSideBlazor();
            services.AddScoped<CrawlerService>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        [Obsolete]
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            DashboardOptions dashboardOptions = new DashboardOptions
            {
                DashboardTitle = "Post Publisher Dashboard",
                DisplayStorageConnectionString = false
                //Authorization = new[] { new HangfireAuthorization() }
            };
            app.UseHangfireDashboard("/hangfire", dashboardOptions);
            BackgroundJobServerOptions hangfireServerOptions = new BackgroundJobServerOptions
            {
                ServerName = String.Format(
                    "{0}.{1}",
                    Environment.MachineName,
                    Guid.NewGuid().ToString())
            };
            app.UseHangfireServer(hangfireServerOptions);

            //RecurringJob.AddOrUpdate<CrawlerService>("Publish-Post", job => job.RunPostPublisher(), Cron.Hourly());
            //RecurringJob.AddOrUpdate<CrawlerService>("Fetch-ghgossip", job => job.RunPostFecther("https://www.ghgossip.com/"), Cron.Hourly());
            //RecurringJob.AddOrUpdate<CrawlerService>("Fetch-uproxx", job => job.RunPostFecther("https://uproxx.com/"), Cron.Hourly());
            //RecurringJob.AddOrUpdate<CrawlerService>("Fetch-DailyPost", job => job.RunPostFecther("https://dailypost.ng/"), Cron.Hourly());
            //RecurringJob.AddOrUpdate<CrawlerService>("Fetch-Vanguard", job => job.RunPostFecther("https://www.vanguardngr.com/"), Cron.Hourly());
            //RecurringJob.AddOrUpdate<CrawlerService>("Fetch-PunchNg", job => job.RunPostFecther("https://punchng.com/"), Cron.Hourly());

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapBlazorHub();
                endpoints.MapFallbackToPage("/_Host");
            });
        }
    }
}
