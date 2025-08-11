namespace Fanex.Athena.Report.Common
{
    using Excel;
    using Excel.Interfaces;
    using Microsoft.Extensions.DependencyInjection;
    using Services;
    using Services.Interfaces;

    public static class SharedServiceDIConfig
    {
        public static IServiceCollection AddCommon(this IServiceCollection services)
        {
            services.AddTransient<IExcelBuilder, ExcelBuilder>();
            services.AddSingleton<IBalanceUpService, BalanceUpService>();
            services.AddSingleton<IFileService, FileService>();
            services.AddSingleton<ICurrencyService, CurrencyService>();
            services.AddScoped<INameService, NameService>();
            services.AddScoped<ICompressService, CompressService>();

            return services;
        }
    }
}