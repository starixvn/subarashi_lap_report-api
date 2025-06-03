using System.Collections.Generic;
using System.Threading.Tasks;
using Fanex.Athena.Report.BySite.DTOs;
using Fanex.Athena.Report.BySite.Models;
using Fanex.Athena.Report.Common.Model.DTO;

namespace Fanex.Athena.Report.BySite.Services.Interfaces;

public interface IMatchMonitoringCenterLogService
{
    Task<IEnumerable<SelectItem>> GetActions(string logTypes = "");

    Task<IEnumerable<SelectItem>> GetLogTypes();

    Task<IEnumerable<MatchMonitoringCenterLogItem>> GetLogData(MatchMonitoringCenterGetLogMetaModel model);
}