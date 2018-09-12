using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OneDirect.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Razor;
using OneDirect.Helper;
using Serilog;
using System.IO;
using Serilog.Events;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using OneDirect.ViewModels;
using System.Collections.Specialized;
using OneDirect.Repository.Interface;
using OneDirect.Repository;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Http;

namespace OneDirect
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            Environment.SetEnvironmentVariable("ASPNETCORE_preventHostingStartup", "True");
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();

            if (env.IsDevelopment())
            {
                // This will push telemetry data through Application Insights pipeline faster, allowing you to view results immediately.
                builder.AddApplicationInsightsSettings(developerMode: true);
            }


            Log.Logger = new LoggerConfiguration()
               .MinimumLevel.Debug()
               .WriteTo.RollingFile(Path.Combine(env.ContentRootPath, "Logs\\log-{Date}.txt"), LogEventLevel.Debug,
                   outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level}] {Message}{NewLine}{Exception}").
               CreateLogger();

            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {

            TimeSlot timeslot = new TimeSlot();
            Configuration.GetSection("TimeSlot").Bind(timeslot);
            ConfigVars.NewInstance.slots = timeslot;

            services.AddDbContext<OneDirectContext>(options =>
                options.UseNpgsql(ConfigVars.NewInstance.ConnectionString)
             );

            //set form options
            services.Configure<FormOptions>(options =>
            {
                options.ValueCountLimit = 500; //default 1024
                options.ValueLengthLimit = int.MaxValue; //not recommended value
                options.MultipartBodyLengthLimit = long.MaxValue; //not recommended value
            });

            // Add framework services.
            services.AddApplicationInsightsTelemetry(Configuration);

            //Hangfire configuration
            //services.AddHangfire(configuration => { });
            services.AddMvc();
            //services.AddMemoryCache();
            services.AddDistributedMemoryCache();


            services.AddSession(options =>
            {
                // Set a short timeout for easy testing.
                options.IdleTimeout = TimeSpan.FromMinutes(Convert.ToInt32(ConfigVars.NewInstance.servertimeout));
                options.Cookie.HttpOnly = true;
                
            });


            services.AddMvc().AddJsonOptions(options =>
            {
                options.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
            });

            services.AddNodeServices();

            services.AddScoped<IUserInterface, UserRepository>();
            services.AddScoped<IRequestFactory, RequestFactory>();
            services.AddScoped<LoginAuthorizeAttribute>();
            services.AddScoped<Smtp>();
                       
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory, IApplicationLifetime appLifetime)
        {



            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();
            loggerFactory.WithFilter(new FilterLoggerSettings
            {
                { "Microsoft.AspNetCore.DataProtection.KeyManagement.XmlKeyManager", LogLevel.None },
                { "Microsoft.AspNetCore.DataProtection.KeyManagement.DefaultKeyResolver", LogLevel.None },
                { "Microsoft.AspNetCore.DataProtection.KeyManagement.KeyRingBasedDataProtector", LogLevel.None },
                { "Microsoft.AspNetCore.DataProtection.KeyManagement.KeyRingBasedDataProtector.", LogLevel.None },
                { "Hangfire.Server.ServerWatchdog", LogLevel.None },
                { "Hangfire.Server.Worker", LogLevel.None },
                { "Hangfire.Server.DelayedJobScheduler", LogLevel.None },
                { "Hangfire.Server.RecurringJobScheduler", LogLevel.None },
                { "Hangfire.BackgroundJobServer", LogLevel.None },
                { "Hangfire.Server.BackgroundProcessingServer", LogLevel.None },
                { "Hangfire.Server.ServerHeartbeat", LogLevel.None },
                { "Hangfire.PostgreSql.ExpirationManager", LogLevel.None }
            }).AddSerilog();


            Utilities.AppLoggerFactory = loggerFactory;
            Utilities.env = env;
            

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseBrowserLink();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            
            app.UseSession();

           

            app.UseStaticFiles();

            


           

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                   template: "{controller=Login}/{action=Index}/{id?}");

                routes.MapRoute(
                     name: "DefaultApi",
                     template: "api/{controller}/{id}");
                routes.MapRoute(
                   name: "DefaultApiWithAction",
                   template: "api/{controller}/{action}/{id}");

            });
           
            //Log
            appLifetime.ApplicationStopped.Register(Log.CloseAndFlush);

        }
       
    }
}
