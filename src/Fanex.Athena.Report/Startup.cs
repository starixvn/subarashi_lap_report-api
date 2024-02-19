namespace Fanex.Athena.Report
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Threading.Tasks;
    using Fanex.Athena.Report.ByPlayer;
    using Fanex.Athena.Report.BySite;
    using Fanex.Athena.Report.Common;
    using Fanex.Athena.Report.Common.Excel;
    using Fanex.Athena.Report.Common.Settings;
    using Fanex.Athena.Report.Core;
    using Fanex.Athena.Report.Core.Extensions;
    using Fanex.Athena.Report.Core.Middleware;
    using Fanex.Athena.Report.Core.ModelBinding;
    using Fanex.Athena.Report.Core.Services;
    using Fanex.Data;
    using Fanex.Data.SqlServer;
    using Fanex.RemotingRef;
    using IdentityModel.AspNetCore.OAuth2Introspection;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.HttpOverrides;
    using Microsoft.AspNetCore.Localization;
    using Microsoft.AspNetCore.Mvc.Authorization;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.FileProviders;
    using Microsoft.Extensions.Hosting;

    public class Startup
    {
        public Startup(IConfiguration configuration, IWebHostEnvironment hostingEnvironment)
        {
            Configuration = configuration;
            HostingEnvironment = hostingEnvironment;
        }

        public IConfiguration Configuration { get; }

        public IWebHostEnvironment HostingEnvironment { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddHttpClient();
            services.AddControllers(options =>
            {
                var policy = new AuthorizationPolicyBuilder()
                              .RequireAuthenticatedUser()
                              .Build();
                options.Filters.Add(new AuthorizeFilter(policy));
                options.ModelBinderProviders.AddBodyOrDefaultBinding();
            }).AddNewtonsoftJson();

            var instrospecCacheDuration = int.TryParse(Configuration["IdentityServer:CacheDuration"], out int cache) ? cache : 0;
            services.AddAuthentication("Bearer")
               .AddOAuth2Introspection("Bearer", options =>
               {
                   options.Authority = Configuration["IdentityServer:Host"];
                   options.ClientId = Configuration["IdentityServer:CredentialIntrospection:ClientId"];
                   options.ClientSecret = Configuration["IdentityServer:CredentialIntrospection:ClientSecret"];
                   options.SaveToken = true;
                   options.SkipTokensWithDots = true;
                   options.EnableCaching = instrospecCacheDuration > 0;
                   options.CacheKeyPrefix = "OAuth2Introspection:";
                   options.CacheDuration = TimeSpan.FromMinutes(instrospecCacheDuration);
                   options.DiscoveryPolicy.RequireHttps = false;
                   options.TokenRetriever = TokenRetrieval.FromAuthorizationHeader();
               });

            services.Configure<ExcelOptions>(Configuration.GetSection("ExcelOptions"));
            services.AddSingleton(Configuration.GetSection("AppSettings").Get<AppSettings>());

            services.AddHttpContextAccessor();
            services.AddHealthChecks().AddCheck<WhoAmIHealthCheck>("WhoAmIHealthCheck");
            services.AddMemoryCache();
            services.AddDistributedMemoryCache();

            ConfigRequestLocalization(services);
            ConfigFanexData(services);

            RefDataTypeContainer
            .ConnectTo(new Uri(Configuration["Fanex.RemotingRef.Service.Url"]))
            .FromApplication("Fanex.LAP.ReportAPI");
            services.AddSingleton<IRemotingRefService, RemotingRefService>();

            services.AddSingleton<IFileProvider>(x =>
                new PhysicalFileProvider(HostingEnvironment.ContentRootPath)
                );

            services.AddCore()
                    .AddCommon()
                    .AddByPlayerModule()
                    .AddBySiteModule();
            services.Configure<ForwardedHeadersOptions>(options =>
            {
                options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
                options.KnownNetworks.Clear();
                options.KnownProxies.Clear();
            });
        }

        private void ConfigFanexData(IServiceCollection services)
        {
            var listConnectionName = Configuration.GetSection("ConnectionStrings").GetChildren().Select(_ => _.Key);
            var connectionStrings = new Dictionary<string, ConnectionConfiguration>(listConnectionName
                .Select(connectionName => new KeyValuePair<string, ConnectionConfiguration>(connectionName,
                    new ConnectionConfiguration(connectionName, Configuration.GetConnectionString(connectionName)))
                ));

            DbSettingProviderManager
                .StartNewSession()
                .WithSqlServer()
                .Use(connectionStrings)
                .Use(new ShardDbSettingProvider(new ShardInfoAccessor(
                    new ContextService(services.BuildServiceProvider().GetRequiredService<IHttpContextAccessor>())),
                    resourcePath: "~/SpConfig/FanexSpName.xml"))
                .Run();
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UsePing();
            app.UseForwardedHeaders();

            if (HostingEnvironment.IsProduction() || string.Equals(Configuration["UseErrorHandler"], "1"))
            {
                app.UseErrorHandler();
            }
            else 
            {
                app.UseDeveloperExceptionPage(); //NOSONAR not used in secure context
            }

            app.UseStaticFiles();
            app.UseWhoAmICheck();
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseRequestLocalization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGet("/", async context => await context.Response.WriteAsync("LAP Report API is running"));
                endpoints.MapControllers();
            });
        }

        private static void ConfigRequestLocalization(IServiceCollection services)
        {
            services.Configure<RequestLocalizationOptions>(options =>
            {
                const string EnCultureName = "en-US";
                var enCulture = new CultureInfo(EnCultureName)
                {
                    DateTimeFormat = new DateTimeFormatInfo
                    {
                        ShortDatePattern = "M/d/yyyy",
                        ShortTimePattern = "h:mm tt",
                        LongDatePattern = "MM/dd/yyyy",
                        LongTimePattern = "h:mm:ss tt"
                    }
                };

                options.DefaultRequestCulture = new RequestCulture(EnCultureName);
                options.SupportedCultures = new List<CultureInfo>
                {
                    enCulture
                };
                options.SupportedUICultures = new List<CultureInfo>
                {
                    enCulture,
                    new CultureInfo("zh-CN"),
                    new CultureInfo("zh-Hans"),
                    new CultureInfo("zh-TW"),
                    new CultureInfo("vi-VN"),
                    new CultureInfo("th-TH"),
                    new CultureInfo("id-ID"),
                };

                var languageCodeMappers = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase)
                {
                    { "en", "en-US" },
                    { "cs", "zh-CN" },
                    { "zhcn", "zh-Hans" },
                    { "ch", "zh-TW" },
                    { "vn", "vi-VN" },
                    { "id", "id-ID" },
                    { "th", "th-TH" },
                };

                options.RequestCultureProviders = new List<IRequestCultureProvider>
                {
                    new CustomRequestCultureProvider(context =>
                    {
                        var langCode = context.User.FindFirst("Language")?.Value ?? "en";
                        var culture = languageCodeMappers.GetValueOrDefault(langCode,"en-US");
                        return Task.FromResult(new ProviderCultureResult(culture: EnCultureName, uiCulture: culture));
                    })
                };
            });
        }
    }
}