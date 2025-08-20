namespace Fanex.Athena.Report.BySite.Services;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Common.Model.DTO;
using Core.Models.DTOs;
using DTOs;
using Interfaces;
using MetaModels;
using Repositories.Interfaces;

public class MatchMonitoringCenterService(
    IMatchMonitoringCenterRepository matchMonitoringCenterRepository)
    : IMatchMonitoringCenterService
{
    public async Task<ResultInfo> AddMonitoring(MatchMonitoringCenterAddMetaModel metaModel)
    {
        return await matchMonitoringCenterRepository.AddMonitoring(metaModel);
    }

    public async Task<MatchMonitoringCenter> GetData(MatchMonitoringCenterGetMetaModel metaModel)
    {
        return await matchMonitoringCenterRepository.GetData(metaModel);
    }

    public async Task<IList<League>> GetLeagueList(int licenseeId)
    {
        return await matchMonitoringCenterRepository.GetLeagueList(licenseeId);
    }

    public async Task<IList<Match>> GetMatchList(int licenseeId, string leagueIds)
    {
        return await matchMonitoringCenterRepository.GetMatchList(licenseeId, leagueIds);
    }

    public async Task<ResultInfo> DeleteMonitoring(MatchMonitoringCenterUpdMetaModel metaModel)
    {
        return await matchMonitoringCenterRepository.DeleteMonitoring(metaModel);
    }

    public async Task<IEnumerable<MatchMonitoringSetting>> GetAlertRule(int monitoringID)
    {
        var results = await matchMonitoringCenterRepository.GetAlertRule(monitoringID);

        if (results != null && results.Any())
        {
            foreach (var result in results)
            {
                result.AlertRule = MapAlertRules(result.Rules);
            }
        }

        return results;
    }

    public async Task<ResultInfo> InsertAlertRule(AlertRuleMetaModel model)
    {
        var rulesJson = ConvertAlertRulesToJsonForUpdate(model.AlertRule);
        var repositoryModel = CreateRepositoryModel(model, rulesJson);
        return await matchMonitoringCenterRepository.InsertAlertRule(repositoryModel);
    }

    public async Task<ResultInfo> UpdateAlertRule(AlertRuleMetaModel model)
    {
        var rulesJson = ConvertAlertRulesToJsonForUpdate(model.AlertRule);
        var repositoryModel = CreateRepositoryModel(model, rulesJson);
        return await matchMonitoringCenterRepository.UpdateAlertRule(repositoryModel);
    }

    public async Task<ResultInfo> DeleteAlertRule(DeleteAlertRuleMetaModel model)
    {
        return await matchMonitoringCenterRepository.DeleteAlertRule(model.SettingId, model.UserId, model.IP);
    }

    public async Task<IEnumerable<TriggeredAlertHistory>> GetTriggeredAlertHistory(int monitoringID)
    {
        return await matchMonitoringCenterRepository.GetTriggeredAlertHistory(monitoringID);
    }

    private AlertRuleMetaModel CreateRepositoryModel(AlertRuleMetaModel model, string rulesJson)
    {
        model.ConditionType = ConvertConditionTypeNameToType(model.ConditionTypeName);

        return new AlertRuleMetaModel
        {
            MonitoringID = model.MonitoringID,
            SettingId = model.SettingId,
            SettingName = model.SettingName,
            ConditionType = model.ConditionType,
            BaseCurrency = model.BaseCurrency,
            NotificationInterval = model.NotificationInterval,
            UserId = model.UserId,
            IP = model.IP,
            AlertRule = model.AlertRule,
            RulesJson = rulesJson
        };
    }

    private AlertRule MapAlertRules(string rulesJson)
    {
        var alertRule = new AlertRule();

        if (string.IsNullOrEmpty(rulesJson))
        {
            return alertRule;
        }

        var alertRuleType = typeof(AlertRule);
        var properties = alertRuleType.GetProperties();

        if (JsonSerializer.Deserialize<List<JsonRuleItem>>(rulesJson) is List<JsonRuleItem> jsonRules)
        {
            foreach (var rule in jsonRules)
            {
                var property = Array.Find(properties, p => string.Equals(p.Name, rule.RuleName, StringComparison.OrdinalIgnoreCase));

                if (property != null)
                {
                    property.SetValue(alertRule, rule.Threshold);
                }
            }
        }

        return alertRule;
    }

    private string ConvertAlertRulesToJsonForUpdate(AlertRule alertRule)
    {
        if (alertRule == null)
        {
            return string.Empty;
        }

        var jsonRules = new List<JsonRuleUpdateItem>();
        var properties = typeof(AlertRule).GetProperties();

        foreach (var property in properties)
        {
            var value = property.GetValue(alertRule);
            if (value != null)
            {
                var ruleId = property.Name.ToUpperInvariant() switch
                {
                    "NEGATIVEPROFIT" => 1,
                    "BETRATIO" => 2,
                    _ => 0
                };

                jsonRules.Add(new JsonRuleUpdateItem
                {
                    RuleId = ruleId,
                    Threshold = Convert.ToDecimal(value)
                });
            }
        }

        return JsonSerializer.Serialize(jsonRules);
    }

    private byte ConvertConditionTypeNameToType(string conditionTypeName)
    {
        if (string.IsNullOrEmpty(conditionTypeName))
        {
            return 0;
        }

        return conditionTypeName.Trim().ToUpperInvariant() switch
        {
            "OR" => 0,
            "AND" => 1,
            _ => 0
        };
    }
}