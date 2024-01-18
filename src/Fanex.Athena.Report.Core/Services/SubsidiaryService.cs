using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Fanex.Athena.Report.Core.Constants;
using Fanex.Athena.Report.Core.Models.DTOs;
using Fanex.Athena.Report.Core.Services.Interfaces;
using Fanex.Athena.Report.Resources;
using Fanex.Data;

namespace Fanex.Athena.Report.Core.Services
{
    public class SubsidiaryService : ISubsidiaryService
    {
        private const string MemoryCacheGroupSubsidiaryListByLicensee = "MemoryCache_GroupSubsidiaryList_{0}";

        private const string MemoryCacheSubsidiaryListByLicensee = "MemoryCache_SubsidiaryList_{0}";

        private const string OtherWhiteLableId = "-2147483648";

        private const int CacheTimeoutSeconds = 60;

        private readonly IMemoryCache _cache;

        private readonly IContextService _contextService;

        public SubsidiaryService(IContextService contextService, IMemoryCache cache)
        {
            _contextService = contextService;
            _cache = cache;
        }

        public bool IsIgnoredSubsidiarySearch(bool subsidiarySearch, string subsidiaryList)
             => !subsidiarySearch || string.IsNullOrWhiteSpace(subsidiaryList)
                || (_contextService.SubsidiaryViewType == SubsidiaryViewType.NotManagement && !_contextService.IsInRoles(Roles.Subsidiary));

        public async Task<IEnumerable<CheckedItem>> GetWhiteLabelTree(string licenseeName)
        {
            var groupSub = await GetAllGroupSubsidiaries(licenseeName);

            return groupSub
                  .OrderByDescending(x => x.WhiteLabelID)
                  .GroupBy(
                       x => new { x.WhiteLabelID, x.WhiteLabelName },
                       (key, group) => new CheckedItem
                       {
                           Value = key.WhiteLabelID?.ToString() ?? OtherWhiteLableId,
                           Name = key.WhiteLabelName ?? ViewResources.Others,
                           Checked = true,
                           Children = group.Select(x => new CheckedItem { Value = x.OperatorId, Name = x.OperatorId, Checked = true }).ToList()
                       }).ToList();
        }

        private async Task<IEnumerable<GroupSubsidiary>> GetAllGroupSubsidiaries(string licenseeName)
        {
            var cacheKey = string.Format(MemoryCacheGroupSubsidiaryListByLicensee, licenseeName);
            var success = _cache.TryGetValue(string.Format(MemoryCacheGroupSubsidiaryListByLicensee, licenseeName), out IEnumerable<GroupSubsidiary> groupSubsidiaryList);

            if (!success)
            {
                groupSubsidiaryList = await GetGroupSubsidiaries(licenseeName);
                _cache.Set(cacheKey, groupSubsidiaryList, DateTimeOffset.Now.AddSeconds(CacheTimeoutSeconds));
            }

            var operators = await GetUserSubsidiaries(_contextService.AgentList, licenseeName);
            var hashSetOperator = new HashSet<string>(operators.Select(o => o.OperatorId.ToLower(CultureInfo.CurrentCulture)).Distinct());
            var whiteLabelId = _contextService.WhiteLabelID;
            return groupSubsidiaryList
                  .Where(x =>
                       (whiteLabelId < 0 || x.WhiteLabelID == whiteLabelId)
                    && hashSetOperator.Contains(x.OperatorId.ToLower(CultureInfo.CurrentCulture)))
                  .ToList();
        }

        private async Task<IEnumerable<SubsidiaryAgent>> GetUserSubsidiaries(IReadOnlyDictionary<int, int[]> currencyAgentList, string licenseeName)
        {
            var cacheKey = string.Format(MemoryCacheSubsidiaryListByLicensee, licenseeName);
            var success = _cache.TryGetValue(cacheKey, out IEnumerable<SubsidiaryAgent> subsidiaryList);

            if (!success)
            {
                subsidiaryList = await GetLicenseeSubsidiaryAgentsFromDB(licenseeName);
                _cache.Set(cacheKey, subsidiaryList, DateTimeOffset.Now.AddSeconds(CacheTimeoutSeconds));
            }

            var userAgentList = currencyAgentList.SelectMany(i => i.Value);
            return subsidiaryList.Where(i => userAgentList.Contains(i.AgentId));
        }

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

        public async Task<IEnumerable<GroupSubsidiary>> GetGroupSubsidiaries(string licenseeName)
        {
            var dataList = new List<GroupSubsidiary>();

            const string SpName = "Portal_Sub_GetSubsidiaryList";
            const int ReportQueryMode = 0;

            using var objectDb = ObjectDbFactory.CreateInstance(SpName);
            using var dataTable = new DataTable();

            dataTable.Columns.Add("Val", typeof(int));
            int[] statusList = { 0, 1, 2, 3 };
            foreach (var status in statusList)
            {
                dataTable.Rows.Add(status);
            }

            var parameter = new
            {
                SiteName = licenseeName,
                StatusList = dataTable,
                ForMgm = ReportQueryMode
            };

            using var reader = await objectDb.ExecuteReaderAsync(parameter);
            while (reader.Read())
            {
                dataList.Add(new GroupSubsidiary
                {
                    WhiteLabelID = reader["WhiteLabelID"] as int?,
                    WhiteLabelName = reader["Name"] as string,
                    OperatorId = reader["OperatorId"] as string
                });
            }

            return dataList;
        }
    }

    internal class SubsidiaryAgent
    {
        internal int AgentId { get; set; }
        
        internal int CurrencyId { get; set; }
        
        internal string OperatorId { get; set; }

    }
}