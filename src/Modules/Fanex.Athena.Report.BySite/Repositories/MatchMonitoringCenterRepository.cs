namespace Fanex.Athena.Report.BySite.Repositories;

using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Fanex.Athena.Report.BySite.DTOs;
using Fanex.Athena.Report.BySite.MetaModels;
using Fanex.Athena.Report.BySite.Repositories.Interfaces;
using Fanex.Athena.Report.Common.Model.DTO;
using Fanex.Athena.Report.Common.Services.Interfaces;
using Fanex.Athena.Report.Core.Exceptions;
using Fanex.Athena.Report.Core.Models.DTOs;
using Fanex.Athena.Report.Core.Utility;
using Fanex.Common;
using Fanex.Common.Extensions;
using Fanex.Data;
using Newtonsoft.Json;

public class MatchMonitoringCenterRepository(
    ICacheRefAdapter cacheRefAdapter)
    : IMatchMonitoringCenterRepository
{
    public async Task<ResultInfo> AddMonitoring(MatchMonitoringCenterAddMetaModel metaModel)
    {
        const string uspName = "Lrf_LicenseeMatchMonitoring_Ins";
        using var db = ObjectDbFactory.CreateInstance(uspName);

        db.SetParameterValue("LicenseeId", metaModel.LicenseeId);
        db.SetParameterValue("MatchID", metaModel.MatchId);
        db.SetParameterValue("BetType", metaModel.BetType);
        db.SetParameterValue("LiveIndicator", metaModel.LiveIndicator);
        db.SetParameterValue("LiveHomeScore", metaModel.LiveHomeScore);
        db.SetParameterValue("LiveAwayScore", metaModel.LiveAwayScore);
        db.SetParameterValue("UserId", metaModel.UserId);
        db.SetParameterValue("IP", metaModel.IP);
        db.AddResultParam();
        await db.ExecuteNonQueryAsync().ConfigureAwait(false);

        return db.GetResult();
    }

    public async Task<MatchMonitoringCenter> GetData(MatchMonitoringCenterGetMetaModel metaModel)
    {
        const string uspName = "Lrf_Rpt_MatchMonitoringCenter";
        using var db = ObjectDbFactory.CreateInstance(uspName);

        db.SetParameterValue("LicenseeId", metaModel.LicenseeId);
        db.SetParameterValue("MatchIdList", metaModel.MatchIdList);
        db.SetParameterValue("MatchStatus", metaModel.MatchStatus);
        db.SetParameterValue("TriggeredAlert", metaModel.TriggeredAlert);
        db.SetParameterValue("AgentList", metaModel.AgentListXml);
        db.SetParameterValue("BaseCurrency", metaModel.BaseCurrency);

        var result = new MatchMonitoringCenter();
        using var reader = await db.ExecuteReaderAsync().ConfigureAwait(false);
        result.Running = GetMatchMonitoringItems(reader);
        reader.NextResult();
        result.Settled = GetMatchMonitoringItems(reader);
        return result;
    }

    private List<MatchMonitoringItem> GetMatchMonitoringItems(IDataReader reader)
    {
        var result = new List<MatchMonitoringItem>();

        while (reader.Read())
        {
            var leagueId = reader.ToInt("LeagueId");
            var sportType = reader.ToInt("SportType");
            var matchId = reader.ToInt("MatchId");
            var homeId = reader.ToInt("HomeId");
            var awayId = reader.ToInt("AwayId");
            var isNeutral = reader.ToBool("IsNeutral");
            var homeName = cacheRefAdapter.GetTeamName(homeId, sportType);
            var awayName = cacheRefAdapter.GetTeamName(awayId, sportType);
            var matchName = cacheRefAdapter.GetMatchName(homeId, awayId, isNeutral);
            var alertRules = reader.ToString("AlertRules");

            var item = new MatchMonitoringItem
            {
                LeagueId = leagueId,
                LeagueName = cacheRefAdapter.GetLeagueName(leagueId, sportType),
                MonitoringId = reader.ToInt("MonitoringId"),
                MatchId = matchId,
                MatchName = matchName,
                HomeName = homeName,
                AwayName = awayName,
                HomeId = homeId,
                AwayId = awayId,
                IsNeutral = isNeutral,
                KickOffTime = reader.ToDateTime("KickOffTime"),
                LiveIndicator = reader.ToBool("LiveIndicator"),
                LiveHomeScore = reader.ToString("LiveHomeScore"),
                LiveAwayScore = reader.ToString("LiveAwayScore"),
                IsAlerted = reader.ToBool("IsAlerted"),
                AlertRules = string.IsNullOrEmpty(alertRules) ? [] : JsonConvert.DeserializeObject<List<string>>(alertRules),
                IsSettled = reader.ToBool("IsSettled"),
                TotalHome = reader.ToDecimal("TotalHome"),
                TotalAway = reader.ToDecimal("TotalAway"),
                HomeStake = reader.ToDecimal("HomeStake"),
                AwayStake = reader.ToDecimal("AwayStake"),
                ForecastHome = reader.ToDecimal("ForecastHome"),
                ForecastAway = reader.ToDecimal("ForecastAway")
            };

            result.Add(item);
        }

        return result;
    }

    public async Task<IList<League>> GetLeagueList(int licenseeId)
    {
        const string uspName = "Lrf_MatchMonitoring_SelLeague";
        using var db = ObjectDbFactory.CreateInstance(uspName);

        db.SetParameterValue("LicenseeId", licenseeId);
        var result = new List<League>();
        using var reader = await db.ExecuteReaderAsync().ConfigureAwait(false);
        while (reader.Read())
        {
            var leagueId = reader.ToInt("LeagueId");
            var sportType = reader.ToInt("SportType");
            var league = new League
            {
                Id = leagueId,
                Name = cacheRefAdapter.GetLeagueName(leagueId, sportType)
            };
            result.Add(league);
        }
        return result;
    }

    public async Task<IList<Match>> GetMatchList(int licenseeId, string leagueIds)
    {
        const string uspName = "Lrf_MatchMonitoring_SelMatchByLeague";
        using var db = ObjectDbFactory.CreateInstance(uspName);

        db.SetParameterValue("LicenseeId", licenseeId);
        db.SetParameterValue("LeagueIDList", leagueIds);
        var result = new List<Match>();
        using var reader = await db.ExecuteReaderAsync().ConfigureAwait(false);
        while (reader.Read())
        {
            var matchId = reader.ToInt("MatchId");
            var homeId = reader.ToInt("HomeId");
            var awayId = reader.ToInt("AwayId");
            var isNeutral = reader.ToBool("IsNeutral");
            var match = new Match
            {
                Id = matchId,
                Name = cacheRefAdapter.GetMatchName(homeId, awayId, isNeutral)
            };
            result.Add(match);
        }
        return result;
    }

    public async Task<ResultInfo> DeleteMonitoring(MatchMonitoringCenterUpdMetaModel metaModel)
    {
        const string uspName = "Lrf_LicenseeMatchMonitoring_Upd";
        using var db = ObjectDbFactory.CreateInstance(uspName);

        db.SetParameterValue("MonitoringId", metaModel.MonitoringId);
        db.SetParameterValue("UserId", metaModel.UserId);
        db.SetParameterValue("IP", metaModel.IP);
        db.AddResultParam();

        await db.ExecuteNonQueryAsync().ConfigureAwait(false);

        return db.GetResult();
    }
}
