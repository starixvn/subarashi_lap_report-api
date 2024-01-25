using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Fanex.Athena.Report.Core.Constants;
using Fanex.Athena.Report.Core.Services.Interfaces;
using Fanex.Data;

namespace Fanex.Athena.Report.Core.Services
{
    public class SubsidiaryService : ISubsidiaryService
    {
        private readonly IContextService _contextService;

        public SubsidiaryService(IContextService contextService)
            => _contextService = contextService;

        public bool IsIgnoredSubsidiarySearch(bool subsidiarySearch, string subsidiaryList)
             => !subsidiarySearch || string.IsNullOrWhiteSpace(subsidiaryList)
                || (_contextService.SubsidiaryViewType == SubsidiaryViewType.NotManagement && !_contextService.IsInRoles(Roles.Subsidiary));

        public async Task<IDictionary<int, int[]>> GetAgentList(string subsidiaryList)
        {
            var needSubsidiaryList = subsidiaryList.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            var userSubsidiaryAgents = await GetUserSubsidiaryAgents(_contextService.AgentList, _contextService.SiteName);
            return userSubsidiaryAgents
                .Where(_ => needSubsidiaryList.Contains(_.OperatorId, StringComparer.InvariantCultureIgnoreCase))
                .GroupBy(_ => _.CurrencyId, (key, group) => KeyValuePair.Create(key, group.Select(_ => _.AgentId).ToArray()))
                .ToDictionary(_ => _.Key, _ => _.Value);
        }

        private async Task<IEnumerable<SubsidiaryAgent>> GetUserSubsidiaryAgents(IReadOnlyDictionary<int, int[]> agentList, string siteName)
        {
            var subsidiaryList = await GetLicenseeSubsidiaryAgentsFromDB(siteName);
            var userAgentList = agentList.SelectMany(i => i.Value);
            return subsidiaryList.Where(i => userAgentList.Contains(i.AgentId));
        }

        private async Task<IEnumerable<SubsidiaryAgent>> GetLicenseeSubsidiaryAgentsFromDB(string siteName)
        {
            var result = new List<SubsidiaryAgent>();

            const string SpName = "Lrf_SubsidiarySel";

            using var objectDb = ObjectDbFactory.CreateInstance(SpName);
            objectDb.SetParameterValue("LicenseeName", siteName);

            using var reader = await objectDb.ExecuteReaderAsync();
            while (reader.Read())
            {
                result.Add(new SubsidiaryAgent
                {
                    AgentId = Convert.ToInt32(reader["AgentID"]),
                    OperatorId = Convert.ToString(reader["OperatorID"]),
                    CurrencyId = Convert.ToInt32(reader["Currency"])
                });
            }

            return result;
        }
    }

    internal class SubsidiaryAgent
    {
        internal int AgentId { get; set; }
        
        internal int CurrencyId { get; set; }
        
        internal string OperatorId { get; set; }

    }
}