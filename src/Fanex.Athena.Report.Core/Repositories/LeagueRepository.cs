using System;
using Fanex.Athena.Report.Common.Services.Interfaces;
using Fanex.Athena.Report.Common.Settings;
using Fanex.Athena.Report.Core.Models.DTOs;
using Fanex.Athena.Report.Core.Models.MetaModels;
using Fanex.Data;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fanex.Athena.Report.Common.Extensions;
using Fanex.Athena.Report.Core.Repositories.Abstraction;
using Fanex.Common.Extensions;

namespace Fanex.Athena.Report.Core.Repositories
{
    public class LeagueRepository : ILeagueRepository
    {
        private bool IsProduction => _appSettings.Environment.Equals("PRO", StringComparison.OrdinalIgnoreCase);

        private readonly AppSettings _appSettings;

        private readonly ICacheRefAdapter _cacheRefAdapter;

        private readonly IBalanceUpService _balanceUpService;

        public LeagueRepository(IBalanceUpService balanceUpService, AppSettings appSettings, ICacheRefAdapter cacheRefAdapter)
        {
            _balanceUpService = balanceUpService;
            _appSettings = appSettings;
            _cacheRefAdapter = cacheRefAdapter;
        }

        public async Task<IEnumerable<League>> GetLeaguesBySport(LeagueMetaModel metaModel, IEnumerable<int> specialLeagues)
        {
            using (var db = ObjectDbFactory.CreateInstance("Lrf_SelLeagueBySport"))
            {
                db.SetParameterValue("sportId", metaModel.SportId);
                db.SetParameterValue("Fromdate", metaModel.FromDate);
                db.SetParameterValue("Todate", metaModel.ToDate);
                db.SetParameterValue("AgentList", metaModel.AgentListXml);
                db.SetParameterValue("MoveBetsDay", (await _balanceUpService.GetBalanceUpAsync()).MoveBetsDay);
                db.SetParameterValue("IsIncludeTestLeague", !IsProduction);
                return await ReadLeagues(db, specialLeagues).ConfigureAwait(false);
            }
        }

        public async Task<IEnumerable<League>> GetRunningLeagues(string agentList, string betTypes, IEnumerable<int> specialLeagues)
        {
            const string UspName = "Shard_Lrf_LeagueSelRunning";
            using (var db = ObjectDbFactory.CreateInstance(UspName))
            {
                db.SetParameterValue("AgentList", agentList);
                db.SetParameterValue("betTypes", betTypes);
                db.SetParameterValue("IsIncludeTestLeague", !IsProduction);
                return await ReadLeagues(db, specialLeagues).ConfigureAwait(false);
            }
        }

        private async Task<IEnumerable<League>> ReadLeagues(IObjectDb db, IEnumerable<int> specialLeagues)
        {
            var dataList = new List<League>();
            using (var reader = await db.ExecuteReaderAsync().ConfigureAwait(false))
            {
                while (reader.Read())
                {
                    var leagueId = reader.ToInt("leagueid");
                    var hasLeagueGroupId = reader.HasColumn("LeagueGroupID");

                    if (!specialLeagues.Contains(leagueId))
                    {
                        dataList.Add(new League
                        {
                            LeagueId = leagueId,
                            LeagueName = _cacheRefAdapter.GetLeagueName(leagueId),
                            LeagueGroupId = hasLeagueGroupId ? reader.ToInt("LeagueGroupID") : 0,
                        });
                    }
                }

                return dataList;
            }
        }
    }
}
