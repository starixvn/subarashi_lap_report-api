namespace Fanex.Athena.Report.Common
{
    using Fanex.Athena.Report.Common.Excel;
    using Fanex.Athena.Report.Common.Services;
    using Microsoft.Extensions.DependencyInjection;

    public static class SharedServiceDIConfig
    {
        public static void Setup(IServiceCollection services)
        {
            services.AddTransient<IExcelBuilder, ExcelBuilderSXSSF>();
            services.AddSingleton<IBalanceUpService, BalanceUpService>();
            services.AddSingleton<IFileService, FileService>();
        }
    }
}
