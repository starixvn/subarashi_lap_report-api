using System.Collections.Generic;
using System.Threading.Tasks;
using Fanex.Athena.Report.BySite.DTOs;
using Fanex.Athena.Report.BySite.Models;

namespace Fanex.Athena.Report.BySite.Repositories.Interfaces;

public interface IMatchMonitoringCenterLogRepository
{
    Task<IEnumerable<LogAction>> GetLogActions();

    Task<IEnumerable<MatchMonitoringCenterLogRawItem>> GetLogData(MatchMonitoringCenterGetLogMetaModel model);
}