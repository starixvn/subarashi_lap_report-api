namespace Fanex.Athena.Report.BySite.Repositories.Interfaces;

using System.Collections.Generic;
using System.Threading.Tasks;
using DTOs;
using Models;

public interface IMatchMonitoringCenterLogRepository
{
    Task<IEnumerable<LogAction>> GetLogActions();

    Task<IEnumerable<MatchMonitoringCenterLogRawItem>> GetLogData(MatchMonitoringCenterGetLogMetaModel model);
}