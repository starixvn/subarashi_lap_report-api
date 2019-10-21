namespace Fanex.Athena.Report.Common
{
    using Fanex.Athena.Report.Common.Excel;
    using Fanex.Athena.Report.Common.Services;
    using Fanex.Athena.Report.Common.Services.Interfaces;
    using Microsoft.Extensions.DependencyInjection;

    public static class SharedServiceDIConfig
    {
        public static void Setup(IServiceCollection services)
        {
            services.AddTransient<IExcelBuilder, ExcelBuilder>();
            services.AddSingleton<IBalanceUpService, BalanceUpService>();
            services.AddSingleton<IFileService, FileService>();
            services.AddSingleton<ICurrencyService, CurrencyService>();
        }
    }
}
