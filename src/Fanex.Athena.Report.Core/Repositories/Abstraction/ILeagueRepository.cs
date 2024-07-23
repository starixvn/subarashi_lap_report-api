using System.Collections.Generic;
using System.Threading.Tasks;
using Fanex.Athena.Report.Core.Models.DTOs;
using Fanex.Athena.Report.Core.Models.MetaModels;

namespace Fanex.Athena.Report.Core.Repositories.Abstraction
{
    public interface ILeagueRepository
    {
        Task<IEnumerable<League>> GetLeaguesBySport(LeagueMetaModel metaModel, IEnumerable<int> specialLeagues);

        Task<IEnumerable<League>> GetRunningLeagues(string agentList, string betTypes, IEnumerable<int> specialLeagues);
    }
}
