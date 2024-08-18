using System;
using Hangfire.Common;
using Hangfire.Sample.Library;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Be.Auto.Hangfire.Dashboard.RecurringJobManager.Core.Extensions;

namespace Hangfire.JobExtensions
{
    public class Startup(IConfiguration configuration)
    {
        public IConfiguration Configuration { get; } = configuration;

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });


            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);



            services.AddHangfire(config => config
                                                 .UseSqlServerStorage(Configuration.GetConnectionString("HangfireConnection"))
                                                 .UseDashboardRecurringJobManager(typeof(Startup).Assembly, typeof(AppointmentSmsNotificationJob).Assembly));
            services.AddHangfireServer();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
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

            app.UseHangfireDashboard();

            var manager = new RecurringJobManager();
            manager.AddOrUpdate("ReadTransactionJob", Job.FromExpression(() => Console.WriteLine("")), "*/5 * * * *");
            manager.AddOrUpdate("ReadTransactionJob2", Job.FromExpression(() => Console.WriteLine("")), "*/8 * * * *");

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseCookiePolicy();


        }
    }
}
