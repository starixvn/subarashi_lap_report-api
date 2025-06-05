namespace Fanex.Athena.Report.BySite.Controllers;

using Fanex.Athena.Report.BySite.DTOs;
using Fanex.Athena.Report.Common.Model.DTO;
using Microsoft.AspNetCore.Mvc;
using Models;
using System.Collections.Generic;
using System.Threading.Tasks;

public partial class MatchMonitoringCenterController
{
    [HttpGet]
    public async Task<ActionResult<MatchMonitoringCenterLogViewModel>> LogIndex()
    {
        var actionOptions = await matchMonitoringCenterLogService.GetActions();
        var logTypeOptions = await matchMonitoringCenterLogService.GetLogTypes();

        return Ok(new MatchMonitoringCenterLogViewModel
        {
            ActionOptions = actionOptions,
            LogTypeOptions = logTypeOptions
        });
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<SelectItem>>> GetActionList([FromQuery] string logTypes)
    {
        var actionOptions = await matchMonitoringCenterLogService.GetActions(logTypes);
        return Ok(actionOptions);
    }

    [HttpPost]
    public async Task<ActionResult<IEnumerable<MatchMonitoringCenterLogItem>>> GetLogData([FromBody] MatchMonitoringCenterGetLogMetaModel metalModel)
    {
        metalModel.LicenseeId = contextService.LicenseeId;
        metalModel.LicenseeName = contextService.SiteName;

        var logData = await matchMonitoringCenterLogService.GetLogData(metalModel);

        return Ok(logData);
    }
}