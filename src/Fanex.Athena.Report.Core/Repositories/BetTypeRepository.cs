namespace Fanex.Athena.Report.Core.Repositories
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Abstraction;
    using Common.Helpers;
    using Common.Model.DataDomain;
    using Common.Services.Interfaces;
    using Data;
    using Data.Extensions.AdoNet;
    using Extensions;
    using Models.DTOs;
    using Resources.Constants;

    public class BetTypeRepository : IBetTypeRepository
    {
        private readonly ICacheRefAdapter _cacheRefAdapter;

        public BetTypeRepository(ICacheRefAdapter cacheRefAdapter)
        {
            _cacheRefAdapter = cacheRefAdapter;
        }

        public async Task<IList<(string ProductPrefix, int BetTypeId)>> GetBettypes(string xmlSportList)
        {
            const string UspName = "Lrf_SelBetTypeBySport";
            IList<(string ProductPrefix, int BetType)> results = new List<(string ProductPrefix, int BetType)>();

            using (var db = ObjectDbFactory.CreateInstance(UspName))
            {
                db.SetParameterValue("sportlist", xmlSportList);
                using (var reader = await db.ExecuteReaderAsync().ConfigureAwait(false))
                {
                    while (reader.Read())
                    {
                        var productPrefix = reader.ToString("ProductPrefix");
                        var betTypeId = reader.ToInt("BetType");
                        results.Add((productPrefix, betTypeId));
                    }
                }
            }

            return results;
        }

        public async Task<IList<CheckedItem>> GetBetTypesByProduct(int licenseeId, string productCode,
                                                                   string languageCode, bool isHideSaba)
        {
            const string UspName = "Lrf_SelBetTypesByProduct";

            var results = new List<CheckedItem>();
            var products = productCode?.ToUpperInvariant().Split(',');

            if (products != null && products.IsPresent(_ => _ == Product.CO))
            {
                results = GetNewCoBetTypes();
                productCode = string.Join(",", products.Where(_ => _ != Product.CO));
            }

            if (string.IsNullOrEmpty(productCode))
            {
                return results;
            }

            IEnumerable<int> betTypeIds;

            using (var db = ObjectDbFactory.CreateInstance(UspName))
            {
                betTypeIds = await db.QueryAsync<int>(new { licenseeId, productCode, });
            }

            results.AddRange(betTypeIds.Select(id =>
            {
                var name = _cacheRefAdapter.GetBetTypeName(id, languageCode, isHideSaba);

                if (MappingHelper.MapBettypeSportByProduct.ContainsKey(id))
                {
                    var suffix =
                        _cacheRefAdapter.GetSportName(MappingHelper.MapBettypeSportByProduct[id], languageCode);
                    name += " (" + suffix + ")";
                }

                return new CheckedItem
                {
                    Checked = true,
                    ID = id,
                    Name = name
                };
            }));

            return results;
        }

        public async Task<IEnumerable<(string ProductCode, int BetType)>> GetBetTypeForProductsAsync(
            int licenseeId, string productCodes)
        {
            const string UspName = "LAP_Rpt_WinlossAnalysis_BettypeSel";

            IList<(string ProductCode, int BetType)> results = new List<(string ProductCode, int BetType)>();

            if (string.IsNullOrEmpty(productCodes))
            {
                return results;
            }

            using (var db = ObjectDbFactory.CreateInstance(UspName))
            {
                db.SetParameterValue("licenseeid", licenseeId);
                db.SetParameterValue("productCodes", productCodes);

                using (var reader = await db.ExecuteReaderAsync().ConfigureAwait(false))
                {
                    while (reader.Read())
                    {
                        var productCode = reader["ProductCode"].ToString();
                        var betType = int.Parse(reader["Bettype"].ToString());

                        results.Add((productCode, betType));
                    }
                }
            }

            return results;
        }

        private List<CheckedItem> GetNewCoBetTypes()
        {
            var betTypes = new List<CheckedItem>();

            betTypes.AddRange(new List<CheckedItem>
            {
                new() { ID = BetType.Handicap, Name = _cacheRefAdapter.GetBettype(BetType.Handicap), },
                new() { ID = BetType.OverUnder, Name = _cacheRefAdapter.GetBettype(BetType.OverUnder), },
                new() { ID = BetType.Ft1X2, Name = _cacheRefAdapter.GetBettype(BetType.Ft1X2), },
                new()
                {
                    ID = BetType.FirstHalfHandicap, Name = _cacheRefAdapter.GetBettype(BetType.FirstHalfHandicap)
                },
                new()
                {
                    ID = BetType.FirstHalfOverUnder, Name = _cacheRefAdapter.GetBettype(BetType.FirstHalfOverUnder)
                },
                new() { ID = BetType.FirstHalf1X2, Name = _cacheRefAdapter.GetBettype(BetType.FirstHalf1X2), }
            });

            return betTypes;
        }
    }
}
