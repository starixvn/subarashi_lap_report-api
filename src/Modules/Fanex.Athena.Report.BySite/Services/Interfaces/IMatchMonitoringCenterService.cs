namespace Fanex.Athena.Report.BySite.Services.Interfaces;

using System.Collections.Generic;
using System.Threading.Tasks;
using Common.Model.DTO;
using Core.Models.DTOs;
using DTOs;
using MetaModels;

public interface IMatchMonitoringCenterService
{
    Task<ResultInfo> AddMonitoring(MatchMonitoringCenterAddMetaModel metaModel);

    Task<MatchMonitoringCenter> GetData(MatchMonitoringCenterGetMetaModel metaModel);

    Task<IList<League>> GetLeagueList(int licenseeId);

    Task<IList<Match>> GetMatchList(int licenseeId, string leagueIds);

    Task<ResultInfo> DeleteMonitoring(MatchMonitoringCenterUpdMetaModel metaModel);

    Task<IEnumerable<MatchMonitoringSetting>> GetAlertRule(int monitoringID);

    Task<ResultInfo> InsertAlertRule(AlertRuleMetaModel model);

    Task<ResultInfo> UpdateAlertRule(AlertRuleMetaModel model);

    Task<ResultInfo> DeleteAlertRule(DeleteAlertRuleMetaModel model);

    Task<IEnumerable<TriggeredAlertHistory>> GetTriggeredAlertHistory(int monitoringID);
}