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
            services.AddScoped<ICustomerWinLossService, CustomerWinLossService>();
            services.AddScoped<ICustomerWinLossExcel, CustomerWinLossExcel>();
        }
    }
}