namespace Fanex.Athena.Report.BySite.Controllers;

using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using BySite.MetaModels;
using BySite.Services.Interfaces;
using Common.Services.Interfaces;
using Core.Attributes;
using Core.Constants;
using Core.Services.Interfaces;
using Fanex.Athena.Report.BySite.Excels.Interfaces;
using Fanex.Athena.Report.Core.Extensions;
using Fanex.Athena.Report.Core.Services;
using Fanex.Athena.Report.Resources;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.FileProviders;
using Models;

[ApiController]
[Route("api/[controller]/[action]")]
[BearerAuthorize(Roles.PortalSuperAdmin, Roles.MatchMonitoringCenter)]
public partial class MatchMonitoringCenterController(
    IMatchMonitoringCenterService matchMonitoringCenterService,
    IBaseCurrencyService baseCurrencyService,
    IMatchMonitoringCenterLogService matchMonitoringCenterLogService,
    IContextService contextService,
    IMatchMonitoringCenterExcel excel,
    IBalanceUpService balanceUpService,
    IFileProvider file) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult> Index([FromQuery] string leagueIds = null, [FromQuery] string matchIds = null)
    {
        var leagueList = await matchMonitoringCenterService.GetLeagueList(contextService.LicenseeId);
        leagueIds = string.IsNullOrEmpty(leagueIds) ? string.Join(",", leagueList.Select(l => l.Id)) : leagueIds;

        var matchList = await matchMonitoringCenterService.GetMatchList(contextService.LicenseeId, leagueIds);
        matchIds = string.IsNullOrEmpty(matchIds) ? string.Join(",", matchList.Select(m => m.Id)) : matchIds;
        var balanceUp = await balanceUpService.GetBalanceUpAsync();

        return Ok(new MatchMonitoringCenterViewModel
        {
            LeagueList = leagueList,
            MatchList = matchList,
            BaseCurrency = contextService.DefaultBaseCurrency,
            BaseCurrencies = baseCurrencyService.GetBaseCurrencyList().ToArray(),
            Currencies = contextService.CurrencyList.ToArray(),
            LeagueIds = leagueIds,
            MatchIds = matchIds,
            HasSettingPermission = contextService.IsInRoles(Roles.PortalSuperAdmin, Roles.MatchMonitoringSetting),
            MoveBetsDay = balanceUp.MoveBetsDay.AddDays(1),
        });
    }

    [HttpGet]
    public async Task<ActionResult> GetMatchList([FromQuery] string leagueIds)
    {
        return Ok(await matchMonitoringCenterService.GetMatchList(contextService.LicenseeId, leagueIds));
    }

    [HttpPost]
    [AutoBindingMetaModel(bindAgent: true)]
    [RequestValidation(validateCurrency: true)]
    public async Task<ActionResult> GetData([FromBody] MatchMonitoringCenterGetMetaModel metaModel)
    {
        UpdateMetaModel(metaModel);
        return Ok(await matchMonitoringCenterService.GetData(metaModel));
    }

    [HttpPost]
    [AutoBindingMetaModel(bindAgent: true)]
    [RequestValidation(validateCurrency: true)]
    public async Task<ActionResult> ExportExcel([FromBody] MatchMonitoringCenterGetMetaModel metaModel)
    {
        UpdateMetaModel(metaModel);
        var template = file.GetFileInfo("/ExcelTemplate/MatchMonitoringCenter.xls").PhysicalPath;
        var data = await matchMonitoringCenterService.GetData(metaModel);
        if (!data.Running.IsPresent() && !data.Settled.IsPresent())
        {
            return NoContent();
        }

        var fileContent = excel.Export(template, data, metaModel);

        var stream = new FileStream(fileContent, FileMode.Open, FileAccess.Read, FileShare.None, 4096, FileOptions.DeleteOnClose);
        return File(stream, "application/octet-stream");
    }

    [HttpPost]
    public async Task<ActionResult> AddMonitoring([FromBody] MatchMonitoringCenterAddMetaModel metaModel)
    {
        metaModel.UserId = contextService.UserId;
        metaModel.LicenseeId = contextService.LicenseeId;
        metaModel.IP = contextService.ClientIP;
        metaModel.Language = contextService.Language;
        var result = await matchMonitoringCenterService.AddMonitoring(metaModel);
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult> DeleteMonitoring([FromBody] MatchMonitoringCenterUpdMetaModel metaModel)
    {
        metaModel.UserId = contextService.UserId;
        metaModel.IP = contextService.ClientIP;
        var result = await matchMonitoringCenterService.DeleteMonitoring(metaModel);
        return Ok(result);
    }

    private void UpdateMetaModel(MatchMonitoringCenterGetMetaModel metaModel)
    {
        metaModel.LicenseeId = contextService.LicenseeId;
    }

    [HttpGet]
    public async Task<IActionResult> GetTriggeredAlertHistory(int monitoringId)
    {
        var result = await matchMonitoringCenterService.GetTriggeredAlertHistory(monitoringId);
        return Ok(result);
    }

    [HttpGet]
    public async Task<IActionResult> GetAlertRule(int monitoringId)
    {
        var result = await matchMonitoringCenterService.GetAlertRule(monitoringId);
        var baseCurrencyList = baseCurrencyService.GetBaseCurrencyList().ToArray();

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
        short[] validIntervals = { 60, 120, 180, 300, 600 };

        if (string.IsNullOrWhiteSpace(model.SettingName))
        {
            return false;
        }

        if (string.IsNullOrEmpty(model.ConditionTypeName) ||
            !validConditionTypes.Any(ct => string.Equals(ct, model.ConditionTypeName, StringComparison.OrdinalIgnoreCase)))
        {
            return false;
        }

        var baseCurrencyList = baseCurrencyService.GetBaseCurrencyList();
        if (string.IsNullOrEmpty(model.BaseCurrency) ||
            !baseCurrencyList.Any(bc => string.Equals(bc.Value, model.BaseCurrency, StringComparison.OrdinalIgnoreCase)))
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

    private static bool ValidateAlertRuleProperties(DTOs.AlertRule alertRule)
    {
        const int MaxBetRatio = 100;
        const decimal MaxSqlMoney = 922337203685477;

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