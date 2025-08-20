namespace Fanex.Athena.Report.Core.Repositories
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Abstraction;
    using Common.Extensions;
    using Common.Services.Interfaces;
    using Data;
    using Models.DTOs;
    using Resources.Constants;
    using Services.Interfaces;

    public class SportRepository : ISportRepository
    {
        private static readonly int[] HiddenSports = new[]
        {
            SportType.UnknowThaiSport,
            SportType.DuplicatedCricket // Duplicated with sport 50
        };

        private readonly ICacheRefAdapter _cacheRefAdapter;
        private readonly IProductService _productService;

        public SportRepository(
            ICacheRefAdapter cacheRefAdapter,
            IProductService productService)
        {
            _cacheRefAdapter = cacheRefAdapter;
            _productService = productService;
        }

        public async Task<IEnumerable<SportProductItem>> GetSportsAndProducts(
            int licenseeId, string languageCode, string productCodes = "")
        {
            const string UspName = "Lrf_SelSportDetail";
            const int ProductType = -1;

            using (var db = ObjectDbFactory.CreateInstance(UspName))
            {
                db.SetParameterValue("licenseeId", licenseeId);
                db.SetParameterValue("ProductCodes", productCodes);

                using (var reader = await db.ExecuteReaderAsync().ConfigureAwait(false))
                {
                    var result = new List<SportProductItem>();

                    while (reader.Read())
                    {
                        var id = reader.GetVal<int>("ID");
                        var parentId = reader.GetVal("ParentId", ProductType);

                        if (HiddenSports.Contains(id))
                        {
                            continue;
                        }

                        var name = parentId == ProductType ?
                            _productService.GetProductName(reader.GetVal<string>("Name")) :
                            _cacheRefAdapter.GetSportName(id, languageCode);

                        result.Add(new SportProductItem
                        {
                            Checked = true,
                            Id = id,
                            Name = name,
                            ParentId = parentId
                        });
                    }

                    return result;
                }
            }
        }

        public async Task<IEnumerable<SportForTreeViewItem>> GetSportsForTreeView(int licenseeId, string productCodes)
        {
            const string UspName = "LAP_Rpt_WinlossAnalysis_SportSel";

            using (var db = ObjectDbFactory.CreateInstance(UspName))
            {
                return await db.QueryAsync<SportForTreeViewItem>(new { licenseeId, productCodes });
            }
        }
    }
}