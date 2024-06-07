using System;
using System.Net;
using System.Net.Http;
using Fanex.Athena.Report.Common.Settings;
using Fanex.Athena.Report.Core.ApiClients;
using Fanex.Athena.Report.Core.ApiClients.Abstraction;
using Fanex.Athena.Report.Core.Repositories;
using Fanex.Athena.Report.Core.Repositories.Abstraction;
using Fanex.Athena.Report.Core.Services;
using Fanex.Athena.Report.Core.Services.Interfaces;
using Fanex.Athena.Report.Core.Utility;
using Fanex.Athena.Report.Core.Utility.Interfaces;
using Fanex.Core.Utils;
using Microsoft.Extensions.DependencyInjection;

namespace Fanex.Athena.Report.Core
{
    public static class DefendencyInjectionExtension
    {
        public static IServiceCollection AddCore(this IServiceCollection services)
        {
            services.AddSingleton<IMetaModelBindingService, BaseMetaModelBindingService>();
            services.AddSingleton<ILicenseeService, LicenseeService>();
            services.AddSingleton<IFileSystem, FileSystem>();
            services.AddScoped<IContextService, ContextService>();
            services.AddSingleton<ICurrencyValidation, CurrencyValidation>();
            services.AddTransient<ISubsidiaryService, SubsidiaryService>();
            services.AddTransient<IBaseCurrencyService, BaseCurrencyService>();
            services.AddTransient<IProductService, ProductService>();
            services.AddTransient<IProductRepository, ProductRepository>();
            services.AddScoped<IDbFactory, DbFactory>();
            services.AddTransient<IBetTypeService, BetTypeService>();
            services.AddTransient<ISportService, SportService>();
            services.AddTransient<IBetTypeRepository, BetTypeRepository>();
            services.AddTransient<ISportRepository, SportRepository>();
            services.AddTransient<ICustomerClassService, CustomerClassService>();
            services.AddTransient<IDataPermissionService, DataPermissionService>();
            return services;
        }

        public static IServiceCollection AddBetListApiClient(this IServiceCollection services, AppSettings settings)
        {
            const string BetListTokenKey = "BETLIST_TOKEN_KEY";
            services.AddHttpClient<IBetListApiClient, BetListApiClient>(client =>
            {
                client.BaseAddress = new Uri(settings.BetListApiHostUrl);
                client.DefaultRequestHeaders.Add(BetListTokenKey, settings.BetListApiTokenValue);
            }).ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
            {
                AllowAutoRedirect = false,
                AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip
            });

            return services;
        }
    }
}
