using Fanex.Athena.Report.Common.Extensions;
using Fanex.Athena.Report.Common.Services.Interfaces;
using Fanex.Athena.Report.Core.Models.DTOs;
using Fanex.Athena.Report.Core.Repositories.Abstraction;
using Fanex.Athena.Report.Resources.Constants;
using Fanex.Athena.Report.Resources.Helpers;
using Fanex.Data;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Fanex.Athena.Report.Core.Repositories
{
    public class SportRepository : ISportRepository
    {
        private static readonly int[] HiddenSports = new[]
        {
            SportType.UnknowThaiSport,
            SportType.DuplicatedCricket // Duplicated with sport 50
        };

        private readonly ICacheRefAdapter _cacheRefAdapter;

        public SportRepository(ICacheRefAdapter cacheRefAdapter)
        {
            _cacheRefAdapter = cacheRefAdapter;
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
                            DataHelper.GetProductName(reader.GetVal<string>("Name")) :
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
