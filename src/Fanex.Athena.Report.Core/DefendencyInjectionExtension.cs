namespace Fanex.Athena.Report.Core;

using System;
using ApiClients;
using ApiClients.Abstraction;
using Common.Settings;
using Fanex.Core.Utils;
using Microsoft.Extensions.DependencyInjection;
using Repositories;
using Repositories.Abstraction;
using Services;
using Services.Interfaces;
using Utility;
using Utility.Interfaces;

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

    public static IServiceCollection AddCommonApiClient(this IServiceCollection services, AppSettings settings)
    {
        services.AddHttpClient<ICommonApiClient, CommonApiClient>(client =>
        {
            client.BaseAddress = new Uri(settings.CommonApiHostUrl);
        });

        return services;
    }
}