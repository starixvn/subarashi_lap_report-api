using Fanex.Athena.Report.Common.Excel;
using Fanex.Athena.Report.Common.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Fanex.Athena.Report.Common
{
    public static class SharedServiceDIConfig
    {
        public static void Setup(IServiceCollection services)
        {
            services.AddScoped<IExcelBuilder, ExcelBuilder>();
            services.AddSingleton<IBalanceUpService, BalanceUpService>();

        }
    }
}
