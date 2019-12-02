namespace Fanex.Athena.Report
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using Fanex.Athena.Report.Common;
    using Fanex.Athena.Report.Common.Excels;
    using Fanex.Athena.Report.Core.DTO;
    using Fanex.Athena.Report.Core.Extensions;
    using Fanex.Athena.Report.Core.Middleware;
    using Fanex.RemotingRef;
    using MessagePack.AspNetCoreMvcFormatter;
    using MessagePack.Resolvers;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.HttpOverrides;
    using Microsoft.AspNetCore.Localization;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.FileProviders;

    public class Startup
    {
        public Startup(IConfiguration configuration, IHostingEnvironment hostingEnvironment)
        {
            Configuration = configuration;
            HostingEnvironment = hostingEnvironment;
        }

        public IConfiguration Configuration { get; }

        public IHostingEnvironment HostingEnvironment { get; }

        //// This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<ApplicationOptions>(Configuration.GetSection("ApplicationOptions"));
            services.Configure<ExcelOptions>(Configuration.GetSection("ExcelOptions"));

            services.AddHealthChecks().AddCheck<WhoAmIHealthCheck>("WhoAmIHealthCheck");
            services.AddMemoryCache();

            FanexDataSetting.Setup(Configuration);

            //// Fanex.RemotingRef
            RefDataTypeContainer
            .ConnectTo(new Uri(Configuration["Fanex.RemotingRef.Service.Url"]))
            .FromApplication("Fanex.LAP.ReportAPI");
            services.AddSingleton<IRemotingRefService, RemotingRefService>();

            services.AddSingleton<IFileProvider>(x =>
                new PhysicalFileProvider(HostingEnvironment.ContentRootPath)
                );
            SharedServiceDIConfig.Setup(services);
            BySite.ReportBySiteDIConfig.Setup(services);
            ByPlayer.ReportByPlayerDIConfig.Setup(services);

            CompositeResolver.RegisterAndSetAsDefault(
                     //// resolver custom types first
                     NativeDateTimeResolver.Instance,
                     //// finally use standard resolver
                     ContractlessStandardResolver.Instance);
            services.Configure<ForwardedHeadersOptions>(options =>
            {
                options.ForwardedHeaders = ForwardedHeaders.XForwardedFor |
                    ForwardedHeaders.XForwardedProto;
                // Only loopback proxies are allowed by default.
                // Clear that restriction because forwarders are enabled by explicit
                // configuration.
                options.KnownNetworks.Clear();
                options.KnownProxies.Clear();
            });
            services.AddMvc(option =>
            {
                option.OutputFormatters.Add(new MessagePackOutputFormatter());
                option.InputFormatters.Add(new MessagePackInputFormatter());
            }).SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
          
        }

        //// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.UseForwardedHeaders();

            var dateformat = new DateTimeFormatInfo
            {
                ShortDatePattern = "M/d/yyyy",
                ShortTimePattern = "h:mm tt",
                LongDatePattern = "MM/dd/yyyy",
                LongTimePattern = "h:mm:ss tt"
            };
            var supportedCultures = new[]
           {
                new CultureInfo("en-US"){ DateTimeFormat= dateformat },
            };
            var supportedUICultures = new List<CultureInfo>
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
                SupportedUICultures = supportedUICultures,
                RequestCultureProviders = new List<IRequestCultureProvider>
                {
                    new ApiRequestCultureProvider()
                },
            });

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseErrorHandler();
            }
            app.UseCustomHealthCheck();
            app.UseIPFilter();
            app.UseCustomApiAuth();
            app.UseMvc();
        }
    }
}