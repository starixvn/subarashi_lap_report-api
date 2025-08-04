namespace Fanex.Athena.Report.Core.Services
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Globalization;
    using System.Linq;
    using System.Threading.Tasks;
    using Constants;
    using Data;
    using Fanex.Core.Utils;
    using Interfaces;
    using Microsoft.Extensions.Caching.Memory;
    using Models.DTOs;
    using Resources;

    public class SubsidiaryService : ISubsidiaryService
    {
        private const string MemoryCacheGroupSubsidiaryListByLicensee = "MemoryCache_GroupSubsidiaryList_{0}";

        private const string MemoryCacheSubsidiaryListByLicensee = "MemoryCache_SubsidiaryList_{0}";

        private const string OtherWhiteLableId = "-2147483648";

        private const int CurrencyAll = 0;

        private const int CacheTimeoutSeconds = 60;

        private readonly IMemoryCache _cache;

        private readonly IContextService _contextService;

        public SubsidiaryService(IContextService contextService, IMemoryCache cache)
        {
            _contextService = contextService;
            _cache = cache;
        }

        public async Task<IDictionary<int, IEnumerable<int>>> GetCurrencyAgentList(
            string licenseeName,
            string subsidiaryNames,
            bool isExcludeTest = false,
            int currencyId = CurrencyAll)
        {
            if (string.IsNullOrWhiteSpace(subsidiaryNames) || (isExcludeTest && currencyId == CurrencyType.Uus))
            {
                return new Dictionary<int, IEnumerable<int>>();
            }

            var subsidiaryNameList = subsidiaryNames.Split(',', StringSplitOptions.RemoveEmptyEntries);

            var subsidiaryData = (await GetUserSubsidiaries(_contextService.AgentList, licenseeName)).ToList();
            var isFilterByCurrencyId = currencyId != CurrencyAll;
            var result = new List<Subsidiary>();
            foreach (var item in subsidiaryData)
            {
                var item1 = item;

                if (!Array.Exists(subsidiaryNameList, x => x.Equals(item1.OperatorId, StringComparison.OrdinalIgnoreCase)))
                {
                    continue;
                }

                if (isFilterByCurrencyId && item.CurrencyId != currencyId)
                {
                    continue;
                }

                if (isExcludeTest && item.CurrencyId == CurrencyType.Uus)
                {
                    continue;
                }

                result.Add(item);
            }

            return result.GroupBy(x => x.CurrencyId).ToDictionary(x => x.Key, x => x.Select(i => i.AgentId));
        }

        public async Task<IEnumerable<CheckedItem>> GetSubsidiaryTree(string licenseeName, bool ignoreViewType = false)
        {
            if (_contextService.UserLevel == UserLevelType.Subsidiary && !string.IsNullOrWhiteSpace(_contextService.OperatorId))
            {
                return new List<CheckedItem>
                {
                    new()
                    {
                        Value = _contextService.OperatorId,
                        Name = _contextService.OperatorId,
                        Checked = true
                    }
                };
            }

            if (!ignoreViewType && _contextService.SubsidiaryViewType == SubsidiaryViewType.NotManagement)
            {
                return Enumerable.Empty<CheckedItem>();
            }

            return _contextService.SubsidiaryViewType == SubsidiaryViewType.MasterManagement
                ? await GetWhiteLabelTree(licenseeName)
                : await GetOperatorTree(licenseeName);
        }

        public async Task<IEnumerable<GroupSubsidiary>> GetGroupSubsidiaryList(string licenseeName, string subsidiaryList, string whiteLabelList)
        {
            var selectedSubsidiaries = subsidiaryList.Split(',');
            var selectedWhiteLabels = whiteLabelList.Split(',').Select(x => x == OtherWhiteLableId ? (int?)null : int.Parse(x));

            var allGroups = await GetAllGroupSubsidiaries(licenseeName);

            return allGroups
                .Where(x => Array.Exists(selectedSubsidiaries, s => s.Equals(x.OperatorId, StringComparison.OrdinalIgnoreCase)) &&
                            selectedWhiteLabels.Contains(x.WhiteLabelID))
                .ToList();
        }

        public async Task<IEnumerable<Subsidiary>> GetSubsidiaryList(string licenseeName, string subsidiaryList)
        {
            var allSubsidiaries = await GetUserSubsidiaries(_contextService.AgentList, licenseeName);

            if (_contextService.UserLevel == UserLevelType.Subsidiary)
            {
                return allSubsidiaries.Where(x => x.OperatorId.Equals(_contextService.OperatorId, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            var selectedSubsidiaries = subsidiaryList.Split(',');
            return allSubsidiaries.Where(x => Array.Exists(selectedSubsidiaries, s => s.Equals(x.OperatorId, StringComparison.OrdinalIgnoreCase)))
                .ToList();
        }

        public bool IsIgnoredSubsidiarySearch(bool subsidiarySearch, string subsidiaryList)
            => !subsidiarySearch || string.IsNullOrWhiteSpace(subsidiaryList)
                                 || (_contextService.SubsidiaryViewType == SubsidiaryViewType.NotManagement &&
                                     !_contextService.IsInRoles(Roles.Subsidiary));

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

        public async Task<IDictionary<int, int[]>> GetAgentList(string subsidiaryList)
        {
            var needSubsidiaryList = subsidiaryList.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            var userSubsidiarys = await GetUserSubsidiarys(_contextService.AgentList, _contextService.SiteName);
            return userSubsidiarys
                .Where(_ => needSubsidiaryList.Contains(_.OperatorId, StringComparer.InvariantCultureIgnoreCase))
                .GroupBy(_ => _.CurrencyId, (key, group) => KeyValuePair.Create(key, group.Select(_ => _.AgentId).ToArray()))
                .ToDictionary(_ => _.Key, _ => _.Value);
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

        private async Task<IEnumerable<CheckedItem>> GetOperatorTree(string licenseeName)
        {
            var groupSub = await GetAllGroupSubsidiaries(licenseeName);

            return groupSub
                .GroupBy(x => x.OperatorId, (key, _) => new CheckedItem { Value = key, Name = key, Checked = true },
                    StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        private async Task<IEnumerable<GroupSubsidiary>> GetAllGroupSubsidiaries(string licenseeName)
        {
            var cacheKey = string.Format(MemoryCacheGroupSubsidiaryListByLicensee, licenseeName);
            var success = _cache.TryGetValue(string.Format(MemoryCacheGroupSubsidiaryListByLicensee, licenseeName),
                out IEnumerable<GroupSubsidiary> groupSubsidiaryList);

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

        private async Task<IEnumerable<Subsidiary>> GetUserSubsidiaries(IReadOnlyDictionary<int, int[]> currencyAgentList, string licenseeName)
        {
            var cacheKey = string.Format(MemoryCacheSubsidiaryListByLicensee, licenseeName);
            var success = _cache.TryGetValue(cacheKey, out IEnumerable<Subsidiary> subsidiaryList);

            if (!success)
            {
                subsidiaryList = await GetLicenseeSubsidiarysFromDB(licenseeName);
                _cache.Set(cacheKey, subsidiaryList, DateTimeOffset.Now.AddSeconds(CacheTimeoutSeconds));
            }

            var userAgentList = currencyAgentList.SelectMany(i => i.Value);
            return subsidiaryList.Where(i => userAgentList.Contains(i.AgentId));
        }

        private async Task<IEnumerable<Subsidiary>> GetUserSubsidiarys(IReadOnlyDictionary<int, int[]> agentList, string siteName)
        {
            var subsidiaryList = await GetLicenseeSubsidiarysFromDB(siteName);
            var userAgentList = agentList.SelectMany(i => i.Value);
            return subsidiaryList.Where(i => userAgentList.Contains(i.AgentId));
        }

        private async Task<IEnumerable<Subsidiary>> GetLicenseeSubsidiarysFromDB(string siteName)
        {
            var result = new List<Subsidiary>();

            const string SpName = "Lrf_SubsidiarySel";

            using var objectDb = ObjectDbFactory.CreateInstance(SpName);
            objectDb.SetParameterValue("LicenseeName", siteName);

            using var reader = await objectDb.ExecuteReaderAsync();
            while (reader.Read())
            {
                result.Add(new Subsidiary
                {
                    AgentId = Convert.ToInt32(reader["AgentID"]),
                    OperatorId = Convert.ToString(reader["OperatorID"]),
                    CurrencyId = Convert.ToInt32(reader["Currency"])
                });
            }

            return result;
        }
    }
}