using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fanex.Athena.Report.BySite.DTOs;
using Fanex.Athena.Report.BySite.Repositories.Interfaces;
using Fanex.Data;

namespace Fanex.Athena.Report.BySite.Repositories
{
    public class SettledMatchCountRepository : ISettledMatchCountRepository
    {
        public async Task<IList<SettledMatchCount>> GetDataList(bool isShowAll, int licenseeId)
        {
            var uspParams = new
            {
                isShowAll,
                licenseeid = licenseeId
            };

            using var db = ObjectDbFactory.CreateInstance("Lrf_Rpt_MatchSelResultEntered");

            return (await db.QueryAsync<SettledMatchCount>(uspParams).ConfigureAwait(false)).ToList();
        }
    }
}
