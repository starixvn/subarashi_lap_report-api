namespace Fanex.Athena.Report.BySite.Services;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using Common.Constants;
using Common.Model.DTO;
using DTOs;
using Interfaces;
using Microsoft.Extensions.Caching.Memory;
using Models;
using Repositories.Interfaces;
using Resources;

public class MatchMonitoringCenterLogService : IMatchMonitoringCenterLogService
{
    private const int CacheTimeoutMinutes = 30;
    private const string LogActionsCacheKey = "MATCH_MONITORING_LOG_ACTIONS";
    private const string ActionsWithFilterCacheKeyPrefix = "MATCH_MONITORING_LOG_ACTIONS_FILTER_";
    private const string LogTypesCacheKey = "MATCH_MONITORING_LOG_TYPES";

    private readonly IMatchMonitoringCenterLogRepository _matchMonitoringCenterLogRepository;
    private readonly IMemoryCache _cache;

    public MatchMonitoringCenterLogService(
        IMatchMonitoringCenterLogRepository matchMonitoringCenterLogRepository,
        IMemoryCache memoryCache)
    {
        _matchMonitoringCenterLogRepository = matchMonitoringCenterLogRepository;
        _cache = memoryCache;
    }

    public async Task<IEnumerable<SelectItem>> GetActions(string logTypes = "")
    {
        string cacheKey = string.IsNullOrEmpty(logTypes)
            ? $"{ActionsWithFilterCacheKeyPrefix}{CultureInfo.CurrentUICulture.Name}ALL"
            : $"{ActionsWithFilterCacheKeyPrefix}{CultureInfo.CurrentUICulture.Name}{logTypes}";

        if (_cache.TryGetValue(cacheKey, out IEnumerable<SelectItem> cachedActions))
        {
            return cachedActions;
        }

        var logActions = await GetLogActionsAsync();

        if (!string.IsNullOrEmpty(logTypes))
        {
            var logTypeIds = logTypes.Split(',')
                .Where(id => !string.IsNullOrWhiteSpace(id))
                .Select(id => int.TryParse(id.Trim(), out int typeId) ? typeId : 0)
                .Where(id => id > 0)
                .ToList();

            if (logTypeIds.Any())
            {
                logActions = logActions.Where(action => logTypeIds.Contains(action.ActionTypeID));
            }
        }

        var actions = logActions
            .GroupBy(action => action.ActionID)
            .OrderBy(action => action.Key)
            .Select(group => new SelectItem
            {
                Key = group.Key.ToString(),
                Value = group.First().ActionName
            })
            .ToList();

        _cache.Set(cacheKey, actions, TimeSpan.FromMinutes(CacheTimeoutMinutes));

        return actions;
    }

    public async Task<IEnumerable<SelectItem>> GetLogTypes()
    {
        if (_cache.TryGetValue($"{LogTypesCacheKey}{CultureInfo.CurrentUICulture.Name}", out IEnumerable<SelectItem> cachedLogTypes))
        {
            return cachedLogTypes;
        }

        var logActions = await GetLogActionsAsync();

        var logTypes = logActions
            .GroupBy(action => action.ActionTypeID)
            .OrderBy(logType => logType.Key)
            .Select(group => new SelectItem
            {
                Key = group.Key.ToString(),
                Value = group.First().ActionTypeName
            })
            .ToList();

        _cache.Set($"{LogTypesCacheKey}{CultureInfo.CurrentUICulture.Name}", logTypes, TimeSpan.FromMinutes(CacheTimeoutMinutes));

        return logTypes;
    }

    public async Task<IEnumerable<MatchMonitoringCenterLogItem>> GetLogData(MatchMonitoringCenterGetLogMetaModel model)
    {
        var logData = await _matchMonitoringCenterLogRepository.GetLogData(model);
        var logActions = await GetLogActionsAsync();

        return logData.Select(item => MapToLogItem(item, logActions));
    }

    private MatchMonitoringCenterLogItem MapToLogItem(MatchMonitoringCenterLogRawItem logData, IEnumerable<LogAction> logActions)
    {
        var action = logActions.FirstOrDefault(a => a.ActionID == logData.ActionID);

        return new MatchMonitoringCenterLogItem
        {
            TimeStamp = logData.LogDate.ToString("yyyy-MM-dd HH:mm:ss"),
            Type = action?.ActionTypeName ?? string.Empty,
            Action = action?.ActionName ?? string.Empty,
            Operator = logData.Operator,
            Detail = GetDetail(logData)
        };
    }

    private string GetDetail(MatchMonitoringCenterLogRawItem logData)
    {
        return logData.ActionID switch
        {
            MatchMonitoringCenterLogAction.CreateAlertRule => ProcessSingleXmlData(logData.Data),
            MatchMonitoringCenterLogAction.RemoveAlertRule => ProcessSingleXmlData(logData.Data),
            MatchMonitoringCenterLogAction.UpdateAlertRule => ProcessXmlData4004(logData.OldData, logData.Data),
            MatchMonitoringCenterLogAction.AddMonitoringMatch => ViewResources.AddMonitoringMatch,
            MatchMonitoringCenterLogAction.RemoveMonitoringMatch => ViewResources.RemoveMonitoringMatch,
            MatchMonitoringCenterLogAction.AlertTriggered => string.Format(ViewResources.AlertTriggeredMsg, FormatXmlDataForAlertTriggered(logData.Data)),
            _ => $"Data: {logData.Data}, Old Data: {logData.OldData}"
        };
    }

    private string ProcessSingleXmlData(string xmlData)
    {
        if (string.IsNullOrEmpty(xmlData))
        {
            return string.Empty;
        }

        try
        {
            return FormatXmlDataForSetting(xmlData);
        }
        catch
        {
            return xmlData;
        }
    }

    private string ProcessXmlData4004(string oldXmlData, string newXmlData)
    {
        string oldFormatted = ProcessSingleXmlData(oldXmlData);
        string newFormatted = ProcessSingleXmlData(newXmlData);

        if (!string.IsNullOrEmpty(oldFormatted))
        {
            return $"{oldFormatted} -> {newFormatted}";
        }

        return newFormatted;
    }

#pragma warning disable S1541

    private static string FormatXmlDataForSetting(string xmlData)
#pragma warning restore S1541
    {
        if (string.IsNullOrEmpty(xmlData))
        {
            return string.Empty;
        }

        try
        {
            var doc = XDocument.Parse(xmlData);
            var root = doc.Element("Root")?.Elements("r").FirstOrDefault();

            if (root == null)
            {
                return xmlData;
            }

            var settingName = GetStringAttributeValue(root, "SettingName");
            var baseCurrency = GetStringAttributeValue(root, "BaseCurrency");
            var notificationInterval = GetIntAttributeValue(root, "NotificationInterval");

            var rules = root.Elements("Rules").ToList();
            if (!rules.Any())
            {
                return $"Name:[{settingName}]";
            }

            var formattedRules = new List<string>();
            foreach (var rule in rules)
            {
                var threshold = GetDecimalAttributeValue(rule, "Threshold");
                var ruleId = GetStringAttributeValue(rule, "RuleID");

                if (!string.IsNullOrEmpty(threshold))
                {
                    formattedRules.Add(ruleId == "2" ? $"{threshold}%" : threshold);
                }
            }

            var conditionTypeValue = GetIntAttributeValue(root, "ConditionType");
            var conditionTypeName = ConvertConditionTypeToName(conditionTypeValue);
            var notificationIntervalValue = ConvertNotificationIntervalToValue(notificationInterval);

            return formattedRules.Count switch
            {
                > 1 => $"{ViewResources.Name}:[{settingName}] | {formattedRules[0]} | {baseCurrency} | {conditionTypeName} | {formattedRules[1]} | {notificationIntervalValue}",
                1 => $"{ViewResources.Name}:[{settingName}] | {formattedRules[0]} | {baseCurrency} | {conditionTypeName} | {notificationIntervalValue}",
                _ => $"{ViewResources.Name}:[{settingName}] | {baseCurrency} | {conditionTypeName} | {notificationIntervalValue}"
            };
        }
        catch
        {
            return xmlData;
        }
    }

    private static string ConvertConditionTypeToName(int conditionType)
    {
        return conditionType switch
        {
            0 => ViewResources.or,
            1 => ViewResources.and,
            _ => ViewResources.or
        };
    }

    private static string ConvertNotificationIntervalToValue(int notificationInterval)
    {
        if (notificationInterval < 0)
        {
            return ViewResources.OnlyOnce;
        }

        return $"{notificationInterval} {ViewResources.seconds}";
    }

    private static string FormatXmlDataForAlertTriggered(string xmlData)
    {
        if (string.IsNullOrEmpty(xmlData))
        {
            return string.Empty;
        }

        try
        {
            var doc = XDocument.Parse(xmlData);
            var root = doc.Element("Root")?.Elements("r").FirstOrDefault();

            if (root == null)
            {
                return xmlData;
            }

            var settingName = GetStringAttributeValue(root, "SettingName");
            var baseCurrency = GetStringAttributeValue(root, "BaseCurrency");

            var negativeProfit = GetDecimalAttributeValue(root, "NegativeProfit");
            var forecastLoss = GetDecimalAttributeValue(root, "ForecastLoss");
            var betRatio = GetDecimalAttributeValue(root, "BetRatio");
            var homeStakePercentage = GetDecimalAttributeValue(root, "HomeStakePercentage");
            var awayStakePercentage = GetDecimalAttributeValue(root, "AwayStakePercentage");

            var conditionTypeValue = GetIntAttributeValue(root, "ConditionType");
            var conditionTypeName = ConvertConditionTypeToName(conditionTypeValue);

            return $"[{settingName}] | {negativeProfit} ({forecastLoss}) | {baseCurrency} | {conditionTypeName} | {betRatio}% ({homeStakePercentage}%:{awayStakePercentage}%)";
        }
        catch
        {
            return xmlData;
        }
    }

    private static string GetDecimalAttributeValue(XElement element, string attributeName)
    {
        var attr = element.Attributes()
            .FirstOrDefault(a => string.Equals(a.Name.LocalName, attributeName, StringComparison.OrdinalIgnoreCase));

        if (attr != null && decimal.TryParse(attr.Value, out decimal value))
        {
            var roundedValue = decimal.Round(value, 2);
            return roundedValue.ToString("N2");
        }
        return "0.00";
    }

    private static string GetStringAttributeValue(XElement element, string attributeName, string defaultValue = "")
    {
        var attr = element.Attributes()
            .FirstOrDefault(a => string.Equals(a.Name.LocalName, attributeName, StringComparison.OrdinalIgnoreCase));

        return attr?.Value ?? defaultValue;
    }

    private static int GetIntAttributeValue(XElement element, string attributeName, int defaultValue = 0)
    {
        var attr = element.Attributes()
            .FirstOrDefault(a => string.Equals(a.Name.LocalName, attributeName, StringComparison.OrdinalIgnoreCase));

        if (attr != null && int.TryParse(attr.Value, out int value))
        {
            return value;
        }
        return defaultValue;
    }

    private async Task<IEnumerable<LogAction>> GetLogActionsAsync()
    {
        if (_cache.TryGetValue($"{LogActionsCacheKey}{CultureInfo.CurrentUICulture.Name}", out IEnumerable<LogAction> cachedLogActions))
        {
            return cachedLogActions;
        }

        var logActions = await _matchMonitoringCenterLogRepository.GetLogActions();

        _cache.Set($"{LogActionsCacheKey}{CultureInfo.CurrentUICulture.Name}", logActions, TimeSpan.FromMinutes(CacheTimeoutMinutes));

        return logActions;
    }
}