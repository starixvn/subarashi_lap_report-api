namespace Fanex.Athena.Report.BySite
{
    using Fanex.Athena.Report.BySite.Excels;
    using Fanex.Athena.Report.BySite.Excels.Interfaces;
    using Fanex.Athena.Report.BySite.Repositories;
    using Fanex.Athena.Report.BySite.Repositories.Interfaces;
    using Fanex.Athena.Report.BySite.Services;
    using Fanex.Athena.Report.BySite.Services.Interfaces;
    using Fanex.Athena.Report.Common.Services;
    using Fanex.Athena.Report.Common.Services.Interfaces;
    using Fanex.Core.Utils.Repository;
    using Fanex.Core.Utils.Repository.Interfaces;
    using Fanex.Core.Utils.Service;
    using Fanex.Core.Utils.Service.Interfaces;
    using Microsoft.Extensions.DependencyInjection;

    public static class ReportBySiteDIConfig
    {
        public static IServiceCollection AddBySiteModule(this IServiceCollection services)
        {
            services.AddTransient<IPlatformWinLossRepository, PlatformWinLossRepository>();
            services.AddTransient<IPlatformWinLossSummaryExcel, PlatformWinLossSummaryExcel>();
            services.AddTransient<IPlatformWinLossMemberExcel, PlatformWinLossMemberExcel>();
            services.AddTransient<IDailyBetsRepository, DailyBetsRepository>();
            services.AddTransient<IDailyBetsService, DailyBetsService>();
            services.AddTransient<IDailyBetsExcel, DailyBetsExcel>();
            services.AddTransient<IGodOfFortuneWinLossRepository, GodOfFortuneWinLossRepository>();
            services.AddTransient<IGodOfFortuneWinLossService, GodOfFortuneWinLossService>();
            services.AddTransient<IGodOfFortuneWinLossExcel, GodOfFortuneWinLossExcel>();
            services.AddTransient<ILeagueWinLossRepository, LeagueWinLossRepository>();
            services.AddTransient<ILeagueWinLossService, LeagueWinLossService>();
            services.AddTransient<ILeagueWinLossExcel, LeagueWinLossExcel>();
            services.AddTransient<ICacheRefAdapter, CacheRefAdapter>();
            services.AddTransient<ISettledMatchCountService, SettledMatchCountService>();
            services.AddTransient<ISettledMatchCountRepository, SettledMatchCountRepository>();
            services.AddTransient<IExtraPayoutRepository, ExtraPayoutRepository>();
            services.AddTransient<IExtraPayoutService, ExtraPayoutService>();
            services.AddTransient<IExtraPayoutExcel, ExtraPayoutExcel>();
            services.AddTransient<IExtraPayoutDetailExcel, ExtraPayoutDetailExcel>();
            services.AddTransient<INewCashOutWinLossRepository, NewCashOutWinLossRepository>();
            services.AddTransient<INewCashOutWinLossService, NewCashOutWinLossService>();
            services.AddTransient<INewCashOutWinLossExcel, NewCashOutWinLossExcel>();
            services.AddTransient<IAbnormalBetRepository, AbnormalBetRepository>();
            services.AddTransient<IAbnormalBetService, AbnormalBetService>();
            services.AddTransient<IIpRepository, IpRepository>();
            services.AddTransient<IIpService, IpService>();
            services.AddTransient<ISubIssueService, SubIssueService>();
            services.AddTransient<IAbnormalBetSummaryExcel, AbnormalBetSummaryExcel>();
            services.AddTransient<IAbnormalBetIpSummaryExcel, AbnormalBetIpSummaryExcel>();
            services.AddTransient<IAbnormalBetDetailExcel, AbnormalBetDetailExcel>();
            services.AddTransient<IWinlossAnalysisRepository, WinlossAnalysisRepository>();
            services.AddTransient<IWinlossAnalysisService, WinlossAnalysisService>();
            services.AddTransient<IWinlossAnalysisExcel, WinlossAnalysisExcel>();
            services.AddTransient<IWeeklyLiveMatchExcel, WeeklyLiveMatchExcel>();
            services.AddTransient<IWeeklyLiveMatchService, WeeklyLiveMatchService>();
            services.AddTransient<IWeeklyLiveMatchRepository, WeeklyLiveMatchRepository>();
            services.AddTransient<IDailyBetDetailsService, DailyBetDetailsService>();
            services.AddTransient<IDailyBetDetailsRepository, DailyBetDetailsRepository>();
            services.AddTransient<IDailyBetDetailsExcel, DailyBetDetailsExcel>();
            services.AddTransient<IPlayerSabaCoinsRepository, PlayerSabaCoinsRepository>();
            services.AddTransient<IPlayerSabaCoinsService, PlayerSabaCoinsService>();
            services.AddTransient<IPlayerSabaCoinsExcel, PlayerSabaCoinsExcel>();
            services.AddTransient<IMatchWinLossService, MatchWinLossService>();
            services.AddTransient<IMatchWinLossRepository, MatchWinLossRepository>();
            services.AddTransient<IMatchWinLossExcel, MatchWinLossExcel>();
            services.AddTransient<ILiveReportRepository, LiveReportRepository>();
            services.AddTransient<ILiveReportService, LiveReportService>();
            services.AddTransient<ILiveReportExcel, LiveReportExcel>();
            services.AddTransient<ILiveReportMatchListExcel, LiveReportMatchListExcel>();
            services.AddTransient<ITurnoverRepository, TurnoverRepository>();
            services.AddTransient<ITurnoverService, TurnoverService>();
            services.AddTransient<ITurnoverExcel, TurnoverExcel>();

            return services;
        }
    }
}