using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using Fanex.Athena.Report.BySite.Excels;
using Fanex.Athena.Report.BySite.Excels.Interfaces;
using Fanex.Athena.Report.BySite.Repositories;
using Fanex.Athena.Report.BySite.Repositories.Interfaces;
using Fanex.Athena.Report.Common;
using Fanex.Athena.Report.Core.DTO;
using Fanex.Athena.Report.Core.Extensions;
using Fanex.Athena.Report.Core.Services;
using Fanex.Logging;
using Fanex.Logging.RabbitMQ;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;

namespace Fanex.Athena.Report
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc()
                .SetCompatibilityVersion(CompatibilityVersion.Version_2_2)
                .AddApplicationPart(Assembly.Load(new AssemblyName("Fanex.Athena.Report.BySite")));
            services.Configure<ApplicationOptions>(Configuration.GetSection("ApplicationOptions"));

            services.AddSingleton<IFileProvider>(new PhysicalFileProvider(Directory.GetCurrentDirectory()));

            BySite.ReportBySiteDIConfig.Setup(services);
            SharedServiceDIConfig.Setup(services);
            LogManager
                  .SetDefaultLogCategory(Configuration.GetSection("LogCategory").Value)
                  .Use(new List<ILogEngine>
                   {
                         new RabbitMQLogging(Configuration.GetSection("RabbitMQ").Value)
                   });
            FanexDataSetting.ConnectDB(Configuration);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            var supportedCultures = new List<CultureInfo>
            {
                new CultureInfo("en-US"),
                new CultureInfo("zh-TW"),
                new CultureInfo("zh-CN"),
                new CultureInfo("zh-Hans")
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
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseIPFilter();
            app.UseCustomApiAuth();
            app.UseErrorHandler();
            app.UseMvc();
        }
    }
}
