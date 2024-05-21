using System;
using System.Collections.Generic;
using System.Globalization;
using Fanex.Athena.Report.Common.Extensions;
using Fanex.Athena.Report.Common.Services.Interfaces;
using Fanex.Data;
using Microsoft.Extensions.Caching.Memory;

namespace Fanex.Athena.Report.Common.Services
{
    public class SubIssueService : ISubIssueService
    {
        private const int CacheTimeoutMinutes = 15;

        private const string CacheKeyPattern = "REPORT_LAP_CACHE_SUBISSUE_{0}";

        private readonly IMemoryCache _cache;

        public SubIssueService(IMemoryCache memoryCache)
        {
            _cache = memoryCache;
        }

        public IDictionary<int, string> GetSubIssueList()
        {
            var CacheKey = string.Format(CacheKeyPattern, CultureInfo.CurrentUICulture.Name);
            var subIssueList = _cache.Get<IDictionary<int, string>>(CacheKey);

            if (subIssueList != null)
            {
                return subIssueList;
            }

            subIssueList = GetSubIssueListFromDb();
            _cache.Set(CacheKey, subIssueList, DateTimeOffset.Now.AddMinutes(CacheTimeoutMinutes));

            return subIssueList;
        }

        private static IDictionary<int, string> GetSubIssueListFromDb()
        {
            const string UspName = "Lrf_AbnormalBet_SubIssueSel";
            var result = new Dictionary<int, string>();

            using (var db = ObjectDbFactory.CreateInstance(UspName))
            {
                db.SetParameterValue("Langkey", CultureInfo.CurrentUICulture.Name);
                using var reader = db.ExecuteReader();

                while (reader.Read())
                {
                    result.Add(reader.GetVal<int>("SubIssueTypeId"), reader.GetVal<string>("SubIssueTypeName"));
                }
            }

            return result;
        }
    }
}