namespace Fanex.Athena.Report.BySite.Controllers;

using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Common.Services.Interfaces;
using Core.Attributes;
using Core.Constants;
using Core.Extensions;
using Core.Services.Interfaces;
using Excels.Interfaces;
using MetaModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.FileProviders;
using Models;
using Services.Interfaces;

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
    private readonly Dictionary<string, string> settingTabDictionary = new()
    {
        { Roles.MatchMonitoringSettingAlertRules, "AlertRules" },
        { Roles.MatchMonitoringSettingLogs, "Logs" }
    };

    [HttpGet]
    public async Task<ActionResult> Index(
        [FromQuery] string leagueIds = null,
        [FromQuery] string matchIds = null,
        [FromQuery] int currencyId = 0,
        [FromQuery] string baseCurrency = null)
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
            DefaultBaseCurrency = contextService.DefaultBaseCurrency,
            BaseCurrency = string.IsNullOrEmpty(baseCurrency) ? contextService.DefaultBaseCurrency : baseCurrency.ToUpper(CultureInfo.InvariantCulture),
            BaseCurrencies = baseCurrencyService.GetBaseCurrencyList().ToArray(),
            Currencies = contextService.CurrencyList.ToArray(),
            CurrencyId = currencyId,
            LeagueIds = leagueIds,
            MatchIds = matchIds,
            HasSettingPermission = contextService.IsInRoles(Roles.PortalSuperAdmin, Roles.MatchMonitoringSetting),
            MoveBetsDay = balanceUp.MoveBetsDay.AddDays(1)
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
        var template = file.GetFileInfo("/ExcelTemplate/MatchMonitoringCenter.xlsx").PhysicalPath;
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

    [HttpGet]
    public async Task<IActionResult> GetTriggeredAlertHistory(int monitoringId)
    {
        var result = await matchMonitoringCenterService.GetTriggeredAlertHistory(monitoringId);
        return Ok(result);
    }

    [HttpGet]
    public IActionResult SettingIndex()
    {
        var tabKeys = new List<string>();

        if (contextService.IsInRoles(Roles.PortalSuperAdmin, Roles.MatchMonitoringSettingAlertRules))
        {
            tabKeys.Add(settingTabDictionary[Roles.MatchMonitoringSettingAlertRules]);
        }

        if (contextService.IsInRoles(Roles.PortalSuperAdmin, Roles.MatchMonitoringSettingLogs))
        {
            tabKeys.Add(settingTabDictionary[Roles.MatchMonitoringSettingLogs]);
        }

        return Ok(new
        {
            tabKeys
        });
    }

    private void UpdateMetaModel(MatchMonitoringCenterGetMetaModel metaModel)
    {
        metaModel.LicenseeId = contextService.LicenseeId;
    }
}