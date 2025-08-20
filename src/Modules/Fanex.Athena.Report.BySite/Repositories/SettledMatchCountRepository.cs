namespace Fanex.Athena.Report.BySite.Repositories
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Data;
    using DTOs;
    using Interfaces;

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
