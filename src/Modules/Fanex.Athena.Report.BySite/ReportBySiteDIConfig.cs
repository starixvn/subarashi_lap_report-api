namespace Fanex.Athena.Report.BySite;

using Common.Services;
using Common.Services.Interfaces;
using Core.Repositories;
using Core.Repositories.Abstraction;
using Core.Services;
using Core.Services.Interfaces;
using Excels;
using Excels.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Repositories;
using Repositories.Interfaces;
using Services;
using Services.Interfaces;

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
        services.AddTransient<IWinLossByTimeZoneRepository, WinLossByTimeZoneRepository>();
        services.AddTransient<IWinLossByTimeZoneService, WinLossByTimeZoneService>();
        services.AddTransient<IWinLossByTimeZoneExcel, WinLossByTimeZoneExcel>();
        services.AddTransient<ISubsidiaryPerformanceRepository, SubsidiaryPerformanceRepository>();
        services.AddTransient<ISubsidiaryPerformanceService, SubsidiaryPerformanceService>();
        services.AddTransient<ISubsidiaryPerformanceSummaryExcel, SubsidiaryPerformanceSummaryExcel>();
        services.AddTransient<ISubsidiaryPerformanceDetailExcel, SubsidiaryPerformanceDetailExcel>();

        return services;
    }
}