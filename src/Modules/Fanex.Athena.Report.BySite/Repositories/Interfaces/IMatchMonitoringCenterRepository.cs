namespace Fanex.Athena.Report.BySite.Repositories.Interfaces;

using System.Collections.Generic;
using System.Threading.Tasks;
using Common.Model.DTO;
using Core.Models.DTOs;
using DTOs;
using MetaModels;

public interface IMatchMonitoringCenterRepository
{
    Task<ResultInfo> AddMonitoring(MatchMonitoringCenterAddMetaModel metaModel);

    Task<IList<League>> GetLeagueList(int licenseeId);

    Task<IList<Match>> GetMatchList(int licenseeId, string leagueIds);

    Task<MatchMonitoringCenter> GetData(MatchMonitoringCenterGetMetaModel metaModel);

    Task<ResultInfo> DeleteMonitoring(MatchMonitoringCenterUpdMetaModel metaModel);

    Task<IEnumerable<MatchMonitoringSetting>> GetAlertRule(int monitoringID);

    Task<ResultInfo> InsertAlertRule(AlertRuleMetaModel model);

    Task<ResultInfo> UpdateAlertRule(AlertRuleMetaModel model);

    Task<ResultInfo> DeleteAlertRule(int settingId, int userId, string ip);

    Task<IEnumerable<TriggeredAlertHistory>> GetTriggeredAlertHistory(int monitoringID);
}