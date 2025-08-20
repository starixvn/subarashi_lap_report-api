namespace Fanex.Athena.Report.BySite.Services.Interfaces;

using System.Collections.Generic;
using System.Threading.Tasks;
using Common.Model.DTO;
using DTOs;
using Models;

public interface IMatchMonitoringCenterLogService
{
    Task<IEnumerable<SelectItem>> GetActions(string logTypes = "");

    Task<IEnumerable<SelectItem>> GetLogTypes();

    Task<IEnumerable<MatchMonitoringCenterLogItem>> GetLogData(MatchMonitoringCenterGetLogMetaModel model);
}