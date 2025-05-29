namespace Fanex.Athena.Report.BySite.Controllers;

using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using BySite.MetaModels;
using Core.Attributes;
using Core.Constants;
using Core.Services.Interfaces;
using BySite.Services.Interfaces;
using Models;
using System.Linq;
using Microsoft.Extensions.FileProviders;
using Fanex.Athena.Report.Core.Extensions;
using Fanex.Athena.Report.BySite.Excels.Interfaces;
using System.IO;

[ApiController]
[Route("api/[controller]/[action]")]
[BearerAuthorize(Roles.PortalSuperAdmin, Roles.MatchMonitoringCenter)]
public class MatchMonitoringCenterController(
    IMatchMonitoringCenterService matchMonitoringCenterService,
    IBaseCurrencyService baseCurrencyService,
    IContextService contextService,
    IMatchMonitoringCenterExcel excel,
    IFileProvider file) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult> Index([FromQuery] string leagueIds = null, [FromQuery] string matchIds = null)
    {
        var leagueList = await matchMonitoringCenterService.GetLeagueList(contextService.LicenseeId);
        leagueIds = string.IsNullOrEmpty(leagueIds) ? string.Join(",", leagueList.Select(l => l.Id)) : leagueIds;

        var matchList = await matchMonitoringCenterService.GetMatchList(contextService.LicenseeId, leagueIds);
        matchIds = string.IsNullOrEmpty(matchIds) ? string.Join(",", matchList.Select(m => m.Id)) : matchIds;

        return Ok(new MatchMonitoringCenterViewModel
        {
            LeagueList = leagueList,
            MatchList = matchList,
            BaseCurrency = contextService.DefaultBaseCurrency,
            BaseCurrencies = baseCurrencyService.GetBaseCurrencyList().ToArray(),
            Currencies = contextService.CurrencyList.ToArray(),
            LeagueIds = leagueIds,
            MatchIds = matchIds
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

        var result = await matchMonitoringCenterService.InsertAlertRule(model);
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> UpdateAlertRule([FromBody] AlertRuleMetaModel model)
    {
        model.UserId = contextService.UserId;
        model.IP = contextService.ClientIP;

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

}