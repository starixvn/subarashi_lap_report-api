namespace Fanex.Athena.Report
{
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Reflection;

    using Fanex.Athena.Report.Common;
    using Fanex.Athena.Report.Core.DTO;
    using Fanex.Athena.Report.Core.Extensions;
    using Fanex.Logging;
    using Fanex.Logging.RabbitMQ;

    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Localization;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.FileProviders;

    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<ApplicationOptions>(Configuration.GetSection("ApplicationOptions"));

            services.AddHealthChecks();

            services.AddMemoryCache();

            services.AddSingleton<IFileProvider>(new PhysicalFileProvider(Directory.GetCurrentDirectory()));

            BySite.ReportBySiteDIConfig.Setup(services);
            ByPlayer.ReportByPlayerDIConfig.Setup(services);

            SharedServiceDIConfig.Setup(services);

            LogManager
                  .SetDefaultLogCategory(Configuration.GetSection("LogCategory").Value)
                  .Use(new List<ILogEngine>
                   {
                         new RabbitMQLogging(Configuration.GetSection("RabbitMQ").Value)
                   });
            FanexDataSetting.ConnectDB(Configuration);

            services.AddMvc()
                    .SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            var dateformat = new DateTimeFormatInfo
            {
                ShortDatePattern = "M/d/yyyy",
                ShortTimePattern = "h:mm tt",
                LongDatePattern = "MM/dd/yyyy",
                LongTimePattern = "h:mm:ss tt"
            };

            var supportedCultures = new List<CultureInfo>
            {
                new CultureInfo("en-US") { DateTimeFormat = dateformat },
                new CultureInfo("zh-TW") { DateTimeFormat = dateformat },
                new CultureInfo("zh-CN") { DateTimeFormat = dateformat },
                new CultureInfo("zh-Hans") { DateTimeFormat = dateformat }
            };
            app.UseRequestLocalization(new RequestLocalizationOptions
            {
                DefaultRequestCulture = new RequestCulture("en-US"),
                SupportedCultures = supportedCultures,
                SupportedUICultures = supportedCultures
            });

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseErrorHandler();
                app.UseHsts();
            }

            app.UseIPFilter();
            app.UseCustomApiAuth();
            app.UseMvc();
        }
    }
}