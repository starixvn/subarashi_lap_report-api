using Fanex.Athena.Report.ByPlayer.Excels;
using Fanex.Athena.Report.ByPlayer.Excels.Interfaces;
using Fanex.Athena.Report.ByPlayer.Service;
using Fanex.Athena.Report.ByPlayer.Service.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace Fanex.Athena.Report.ByPlayer
{
    public static class ReportByPlayerDIConfig
    {
        public static void Setup(IServiceCollection services)
        {
            services.AddTransient<ICustomerWinLossService, CustomerWinLossService>();
            services.AddTransient<ICustomerWinLossExcel, CustomerWinLossExcel>();
            services.AddTransient<ICustomerBalanceService, CustomerBalanceService>();
            services.AddTransient<ICustomerBalanceExcel, CustomerBalanceExcel>();
        }
    }
}