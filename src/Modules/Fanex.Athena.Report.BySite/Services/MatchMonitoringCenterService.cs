namespace Fanex.Athena.Report.BySite.Services;

using System.Collections.Generic;
using System.Threading.Tasks;
using Fanex.Athena.Report.BySite.DTOs;
using Fanex.Athena.Report.BySite.MetaModels;
using Fanex.Athena.Report.BySite.Repositories.Interfaces;
using Fanex.Athena.Report.BySite.Services.Interfaces;
using Fanex.Athena.Report.Common.Model.DTO;
using Fanex.Athena.Report.Core.Models.DTOs;

public class MatchMonitoringCenterService(
    IMatchMonitoringCenterRepository matchMonitoringCenterRepository) 
    : IMatchMonitoringCenterService
{
    public async Task<ResultInfo> AddMonitoring(MatchMonitoringCenterAddMetaModel metaModel)
    {
        return await matchMonitoringCenterRepository.AddMonitoring(metaModel);
    }

    public async Task<MatchMonitoringCenter> GetData(MatchMonitoringCenterGetMetaModel metaModel)
    {
        return await matchMonitoringCenterRepository.GetData(metaModel);
    }

    public async Task<IList<League>> GetLeagueList(int licenseeId)
    {
        return await matchMonitoringCenterRepository.GetLeagueList(licenseeId);
    }

    public async Task<IList<Match>> GetMatchList(int licenseeId, string leagueIds)
    {
        return await matchMonitoringCenterRepository.GetMatchList(licenseeId, leagueIds);
    }

    public async Task<ResultInfo> DeleteMonitoring(MatchMonitoringCenterUpdMetaModel metaModel)
    {
        return await matchMonitoringCenterRepository.DeleteMonitoring(metaModel);
    }
} 