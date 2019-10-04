namespace Fanex.Athena.Report.BySite
{
    using Fanex.Athena.Report.BySite.Excels;
    using Fanex.Athena.Report.BySite.Excels.Interfaces;
    using Fanex.Athena.Report.BySite.Repositories;
    using Fanex.Athena.Report.BySite.Repositories.Interfaces;
    using Microsoft.Extensions.DependencyInjection;

    public static class ReportBySiteDIConfig
    {
        public static void Setup(IServiceCollection services)
        {
            services.AddTransient<IPlatformWinLossRepository, PlatformWinLossRepository>();
            services.AddTransient<IPlatformWinLossSummaryExcel, PlatformWinLossSummaryExcel>();
            services.AddTransient<IPlatformWinLossMemberExcel, PlatformWinLossMemberExcel>();
        }
    }
}
