namespace Fanex.Athena.Report.BySite.Repositories;

using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Common.Model.DTO;
using Common.Services.Interfaces;
using Core.Models.DTOs;
using Core.Utility;
using Data;
using Data.Extensions.AdoNet;
using DTOs;
using Interfaces;
using MetaModels;
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
        db.SetParameterValue("Langkey", metaModel.Language);
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

    public async Task<IEnumerable<MatchMonitoringSetting>> GetAlertRule(int monitoringID)
    {
        const string uspName = "Lrf_LicenseeMatchMonitoringSetting_Sel";
        using var db = ObjectDbFactory.CreateInstance(uspName);

        db.SetParameterValue("MonitoringID", monitoringID);

        var results = new List<MatchMonitoringSetting>();

        using (var reader = await db.ExecuteReaderAsync().ConfigureAwait(false))
        {
            while (reader.Read())
            {
                results.Add(new MatchMonitoringSetting
                {
                    SettingID = reader.GetInt32(reader.GetOrdinal("SettingID")),
                    SettingName = reader.GetString(reader.GetOrdinal("SettingName")),
                    ConditionTypeName = reader.GetString(reader.GetOrdinal("ConditionTypeName")).ToLowerInvariant(),
                    BaseCurrency = reader.GetString(reader.GetOrdinal("BaseCurrency")),
                    NotificationInterval = reader.GetInt16(reader.GetOrdinal("NotificationInterval")),
                    Rules = reader.GetString(reader.GetOrdinal("Rules"))
                });
            }
        }

        return results;
    }

    public async Task<ResultInfo> InsertAlertRule(AlertRuleMetaModel model)
    {
        const string uspName = "Lrf_LicenseeMatchMonitoringSetting_Ins";
        using var db = ObjectDbFactory.CreateInstance(uspName);

        db.SetParameterValue("MonitoringID", model.MonitoringID);
        db.SetParameterValue("SettingId", model.SettingId);
        db.SetParameterValue("SettingName", model.SettingName);
        db.SetParameterValue("ConditionType", model.ConditionType);
        db.SetParameterValue("BaseCurrency", model.BaseCurrency);
        db.SetParameterValue("NotificationInterval", model.NotificationInterval);
        db.SetParameterValue("RulesJson", model.RulesJson);
        db.SetParameterValue("UserId", model.UserId);
        db.SetParameterValue("Langkey", model.Language);
        db.SetParameterValue("IP", model.IP);

        db.AddResultParam();

        await db.ExecuteNonQueryAsync().ConfigureAwait(false);

        return db.GetResult();
    }

    public async Task<ResultInfo> UpdateAlertRule(AlertRuleMetaModel model)
    {
        const string uspName = "Lrf_LicenseeMatchMonitoringSetting_Upd";
        using var db = ObjectDbFactory.CreateInstance(uspName);

        db.SetParameterValue("SettingId", model.SettingId);
        db.SetParameterValue("SettingName", model.SettingName);
        db.SetParameterValue("ConditionType", model.ConditionType);
        db.SetParameterValue("BaseCurrency", model.BaseCurrency);
        db.SetParameterValue("NotificationInterval", model.NotificationInterval);
        db.SetParameterValue("RulesJson", model.RulesJson);
        db.SetParameterValue("UserId", model.UserId);
        db.SetParameterValue("IP", model.IP);

        db.AddResultParam();

        await db.ExecuteNonQueryAsync().ConfigureAwait(false);

        return db.GetResult();
    }

    public async Task<ResultInfo> DeleteAlertRule(int settingId, int userId, string ip)
    {
        const string uspName = "Lrf_LicenseeMatchMonitoringSetting_Del";
        using var db = ObjectDbFactory.CreateInstance(uspName);

        db.SetParameterValue("SettingId", settingId);
        db.SetParameterValue("UserId", userId);
        db.SetParameterValue("IP", ip);

        db.AddResultParam();

        await db.ExecuteNonQueryAsync().ConfigureAwait(false);

        return db.GetResult();
    }

    public async Task<IEnumerable<TriggeredAlertHistory>> GetTriggeredAlertHistory(int monitoringID)
    {
        const string uspName = "Lrf_MatchMonitoring_SelTriggeredAlert";
        using var db = ObjectDbFactory.CreateInstance(uspName);

        db.SetParameterValue("MonitoringID", monitoringID);

        var result = new List<TriggeredAlertHistory>();
        using var reader = await db.ExecuteReaderAsync().ConfigureAwait(false);
        while (reader.Read())
        {
            string alertRules = reader.ToString("AlertRule");
            string detectedInfos = reader.ToString("DetectedInfo");

            result.Add(new TriggeredAlertHistory
            {
                Time = reader.ToDateTime("LogDate"),
                AlertRules = string.IsNullOrEmpty(alertRules) ? [] : JsonConvert.DeserializeObject<List<string>>(alertRules),
                DetectedInfos = string.IsNullOrEmpty(detectedInfos) ? [] : JsonConvert.DeserializeObject<List<string>>(detectedInfos)
            });
        }
        return result;
    }
}