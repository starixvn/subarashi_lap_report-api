namespace Fanex.Athena.Report.BySite.Controllers;

using System;
using System.Linq;
using System.Threading.Tasks;
using DTOs;
using MetaModels;
using Microsoft.AspNetCore.Mvc;
using Resources;

public partial class MatchMonitoringCenterController
{
    private const int UUSBaseCurrencyId = -999;
    private readonly string[] AllowUUSSites = { "TGXI", "Athena000", "TGXISW" };

    [HttpGet]
    public async Task<IActionResult> GetAlertRule(int monitoringId)
    {
        var result = await matchMonitoringCenterService.GetAlertRule(monitoringId);
        var baseCurrencyList = baseCurrencyService.GetBaseCurrencyList().ToList();

        if (Array.Exists(AllowUUSSites, site => string.Equals(contextService.SiteName, site, StringComparison.OrdinalIgnoreCase)))
        {
            var baseCurrencyDict = baseCurrencyList.ToDictionary(x => x.Key, x => x.Value);
            baseCurrencyDict.Add(UUSBaseCurrencyId, "UUS");
            baseCurrencyList = baseCurrencyDict.ToList();
        }

        return Ok(new
        {
            DataList = result,
            BaseCurrencyList = baseCurrencyList,
            contextService.DefaultBaseCurrency
        });
    }

    [HttpPost]
    public async Task<IActionResult> InsertAlertRule([FromBody] AlertRuleMetaModel model)
    {
        model.UserId = contextService.UserId;
        model.IP = contextService.ClientIP;
        model.Language = contextService.Language;

        if (!ValidateAlertRuleModel(model))
        {
            return BadRequest(ViewResources.InvalidRequestMsg);
        }

        var result = await matchMonitoringCenterService.InsertAlertRule(model);
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> UpdateAlertRule([FromBody] AlertRuleMetaModel model)
    {
        model.UserId = contextService.UserId;
        model.IP = contextService.ClientIP;

        if (!ValidateAlertRuleModel(model))
        {
            return BadRequest(ViewResources.InvalidRequestMsg);
        }

        var result = await matchMonitoringCenterService.UpdateAlertRule(model);
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> DeleteAlertRule([FromBody] DeleteAlertRuleMetaModel model)
    {
        model.UserId = contextService.UserId;
        model.IP = contextService.ClientIP;

        var result = await matchMonitoringCenterService.DeleteAlertRule(model);
        return Ok(result);
    }

    private bool ValidateAlertRuleModel(AlertRuleMetaModel model)
    {
        string[] validConditionTypes = { "or", "and" };
        short[] validIntervals = { -1, 180, 300, 600 };

        if (string.IsNullOrWhiteSpace(model.SettingName))
        {
            return false;
        }

        if (string.IsNullOrEmpty(model.ConditionTypeName) ||
            !Array.Exists(validConditionTypes, ct => string.Equals(ct, model.ConditionTypeName, StringComparison.OrdinalIgnoreCase)))
        {
            return false;
        }

        if (!IsValidBaseCurrency(model.BaseCurrency))
        {
            return false;
        }

        if (!validIntervals.Contains(model.NotificationInterval))
        {
            return false;
        }

        if (model.AlertRule != null && !ValidateAlertRuleProperties(model.AlertRule))
        {
            return false;
        }

        return true;
    }

    private bool IsValidBaseCurrency(string baseCurrency)
    {
        var baseCurrencyList = baseCurrencyService.GetBaseCurrencyList();
        bool isUUSAllowed = Array.Exists(AllowUUSSites, site => string.Equals(contextService.SiteName, site, StringComparison.OrdinalIgnoreCase));

        if (string.IsNullOrEmpty(baseCurrency))
        {
            return false;
        }

        if (string.Equals("UUS", baseCurrency, StringComparison.OrdinalIgnoreCase))
        {
            return isUUSAllowed;
        }

        return baseCurrencyList.Any(bc =>
            string.Equals(bc.Value, baseCurrency, StringComparison.OrdinalIgnoreCase));
    }

    private static bool ValidateAlertRuleProperties(AlertRule alertRule)
    {
        const int MaxBetRatio = 100;
        const decimal MaxSqlMoney = 922_337_203_685_477;

        if (alertRule.NegativeProfit < 0 || alertRule.NegativeProfit > MaxSqlMoney)
        {
            return false;
        }

        if (alertRule.BetRatio < 0 || alertRule.BetRatio > MaxBetRatio)
        {
            return false;
        }

        return true;
    }
}