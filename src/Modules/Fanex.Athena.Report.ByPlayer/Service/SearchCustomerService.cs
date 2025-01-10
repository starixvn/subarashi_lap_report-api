using Fanex.Athena.Report.ByPlayer.Service.Interfaces;
using Fanex.Athena.Report.Core.Helpers;
using Fanex.Data;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Fanex.Athena.Report.ByPlayer.Service
{
    public class SearchCustomerService : ISearchCustomerService
    {
        public async Task<IEnumerable<string>> SearchCustomers(int userId, string userName, string agentListXml, bool isIncludeDormant)
        {
            using (var db = ObjectDbFactory.CreateInstance("Lrf_CustomerSelUserSearch"))
            {
                db.SetParameterValue("userid", userId);
                db.SetParameterValue("username", SqlHelper.ToSqlTextSearch(userName));
                db.SetParameterValue("AgentList", agentListXml);
                db.SetParameterValue("IsIncludeDormant", isIncludeDormant);
                return await db.QueryAsync<string>().ConfigureAwait(false);
            }
        }
    }
}
