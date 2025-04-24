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
using Fanex.Athena.Report.DbSettingConfig;
using Fanex.Data;
using Fanex.Data.SqlServer;
using Fanex.Data.Vault;
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
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;

var builder = WebApplication.CreateBuilder(args);
ConfigureLogging(builder);
ConfigureConfiguration(builder);
builder.WebHost.UseSentry();
ConfigureServices(builder.Services, builder.Configuration, builder.Environment);

var app = builder.Build();
ConfigureMiddleware(app);

app.Run();

void ConfigureLogging(WebApplicationBuilder builder)
{
    builder.Logging.ClearProviders();
    builder.Logging.AddConsole();
}

void ConfigureConfiguration(WebApplicationBuilder builder)
{
    var env = builder.Environment;
    builder.Configuration.SetBasePath(env.ContentRootPath);
    builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: false);
    builder.Configuration.AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true, reloadOnChange: false);
    builder.Configuration.AddEnvironmentVariables();
}

void ConfigureServices(IServiceCollection services, IConfiguration configuration, IHostEnvironment environment)
{
    var settings = configuration.GetSection(nameof(AppSettings)).Get<AppSettings>();

    services.AddHealthChecks().AddCheck<WhoAmIHealthCheck>("WhoAmIHealthCheck");

    services.Configure<ExcelOptions>(configuration.GetSection("ExcelOptions"));
    services.Configure<AppSettings>(configuration.GetSection(nameof(AppSettings)));
    services.AddSingleton(sp => sp.GetRequiredService<IOptions<AppSettings>>().Value);

    services.AddMemoryCache();
    services.AddDistributedMemoryCache();
    services.AddHttpContextAccessor();
    services.AddHttpClient();
    services.AddBetListApiClient(settings);

    services.AddControllers(options =>
    {
        var policy = new AuthorizationPolicyBuilder()
                      .RequireAuthenticatedUser()
                      .Build();
        options.Filters.Add(new AuthorizeFilter(policy));
        options.ModelBinderProviders.AddBodyOrDefaultBinding();
    }).AddNewtonsoftJson();

    ConfigureAuthentication(services, configuration);
    ConfigureLocalization(services);
    ConfigureFanexData(services, configuration);

    RefDataTypeContainer
       .ConnectTo(new Uri(configuration["Fanex.RemotingRef.Service.Url"]))
       .FromApplication("Fanex.LAP.ReportAPI");
    services.AddSingleton<IRemotingRefService, RemotingRefService>();

    services.AddSingleton<IFileProvider>(x =>
        new PhysicalFileProvider(environment.ContentRootPath));

    services.AddCore().AddCommon().AddByPlayerModule().AddBySiteModule();

    services.Configure<ForwardedHeadersOptions>(options =>
    {
        options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
        options.KnownNetworks.Clear();
        options.KnownProxies.Clear();
    });
}

void ConfigureAuthentication(IServiceCollection services, IConfiguration configuration)
{
    var cacheDuration = int.TryParse(configuration["IdentityServer:CacheDuration"], out int cache) ? cache : 0;
    services.AddAuthentication("Bearer")
        .AddOAuth2Introspection("Bearer", options =>
        {
            options.Authority = configuration["IdentityServer:Host"];
            options.ClientId = configuration["IdentityServer:CredentialIntrospection:ClientId"];
            options.ClientSecret = configuration["IdentityServer:CredentialIntrospection:ClientSecret"];
            options.SaveToken = true;
            options.SkipTokensWithDots = true;
            options.EnableCaching = cacheDuration > 0;
            options.CacheKeyPrefix = "OAuth2Introspection:";
            options.CacheDuration = TimeSpan.FromMinutes(cacheDuration);
            options.DiscoveryPolicy.RequireHttps = false;
            options.TokenRetriever = TokenRetrieval.FromAuthorizationHeader();
        });
}

void ConfigureLocalization(IServiceCollection services)
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
        options.SupportedCultures = [enCulture];
        options.SupportedUICultures =
        [
            enCulture,
            new CultureInfo("zh-CN"),
            new CultureInfo("zh-Hans"),
            new CultureInfo("zh-TW"),
            new CultureInfo("vi-VN"),
            new CultureInfo("th-TH"),
            new CultureInfo("id-ID"),
        ];

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

        options.RequestCultureProviders =
        [
            new CustomRequestCultureProvider(context =>
            {
                var langCode = context.User.FindFirst("Language")?.Value ?? "en";
                var culture = languageCodeMappers.GetValueOrDefault(langCode,"en-US");
                return Task.FromResult(new ProviderCultureResult(culture: EnCultureName, uiCulture: culture));
            })
        ];
    });
}

void ConfigureFanexData(IServiceCollection services, IConfiguration configuration)
{
    DbSettingProviderManager
        .StartNewSession()
        .WithSqlServer(configuration)
        .Register(ShardDbSettingProvider.ProviderName,
            new ShardDbSettingProvider(new ShardInfoAccessor(new ContextService(services.BuildServiceProvider().GetRequiredService<IHttpContextAccessor>())))
            .CreateObjectDbFunc)
        .UseDefaultDbSettingProvider("~/SpConfig/FanexSpName.xml", true)
        .Run();
    services.TryAddSingleton<VaultDbAliasService, VaultDbAliasServiceImp>();
}

void ConfigureMiddleware(WebApplication app)
{
    app.UsePing();
    app.UseForwardedHeaders();
    if (app.Environment.IsProduction() || string.Equals(app.Configuration["UseErrorHandler"], "1"))
    {
        app.UseErrorHandler();
    }
    else
    {
        app.UseDeveloperExceptionPage();
    }
    app.UseStaticFiles();
    app.UseWhoAmICheck();
    app.UseRouting();
    app.UseAuthentication();
    app.UseAuthorization();
    app.UseRequestLocalization();

    app.MapGet("/", async context => await context.Response.WriteAsync("LAP Report API is running"));
    app.MapControllers();
}