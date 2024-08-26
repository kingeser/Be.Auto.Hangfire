using System;
using Be.Auto.Hangfire.Dashboard.RecurringJobManager.Core.Extensions;
using Be.Auto.Hangfire.Dashboard.RecurringJobManager.Models;
using Hangfire;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Sample.Library;

namespace Sample
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

            services.AddHangfire(config => config
                                                 .UseSqlServerStorage(Configuration.GetConnectionString("HangfireConnection"))
                                                 .UseDashboardRecurringJobManager(option =>
                                                 {
                                                     option.AddAppDomain(AppDomain.CurrentDomain);
                                                     option.ConcurrentJobExecution = ConcurrentJobExecution.Disable;
                                                     option.WebRequestJob.TimeOut = TimeSpan.FromSeconds(15);
                                                 })
            );
            services.AddHangfireServer();


            services.AddScoped<IProductService, ProductService>(t => new ProductService("https://domain.com"));
            services.AddScoped<IAppointmentSmsNotificationService, AppointmentSmsNotificationService>(t => new AppointmentSmsNotificationService("https://domain.com"));
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

            app.UseHangfireDashboard("/hangfire", new DashboardOptions()
            {
                DarkModeEnabled = false
            });

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseCookiePolicy();


        }
    }
}
