namespace Fanex.Athena.Report.BySite
{
    using Fanex.Athena.Report.BySite.Excels;
    using Fanex.Athena.Report.BySite.Excels.Interfaces;
    using Fanex.Athena.Report.BySite.Repositories;
    using Fanex.Athena.Report.BySite.Repositories.Interfaces;
    using Fanex.Athena.Report.BySite.Services;
    using Fanex.Athena.Report.BySite.Services.Interfaces;
    using Microsoft.Extensions.DependencyInjection;

    public static class ReportBySiteDIConfig
    {
        public static void Setup(IServiceCollection services)
        {
            services.AddTransient<IPlatformWinLossRepository, PlatformWinLossRepository>();
            services.AddTransient<IPlatformWinLossSummaryExcel, PlatformWinLossSummaryExcel>();
            services.AddTransient<IPlatformWinLossMemberExcel, PlatformWinLossMemberExcel>();
            services.AddTransient<IDailyBetsRepository, DailyBetsRepository>();
            services.AddTransient<IDailyBetsService, DailyBetsService>();
            services.AddTransient<IDailyBetsExcel, DailyBetsExcel>();
            services.AddTransient<IPromotionPrizeRepository, PromotionPrizeRepository>();
            services.AddTransient<IPromotionPrizeExcel, PromotionPrizeExcel>();
        }
    }
}
