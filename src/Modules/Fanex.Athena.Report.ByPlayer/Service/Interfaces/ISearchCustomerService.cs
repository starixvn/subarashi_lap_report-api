using System.Collections.Generic;
using System.Threading.Tasks;

namespace Fanex.Athena.Report.ByPlayer.Service.Interfaces
{
    public interface ISearchCustomerService
    {
        Task<IEnumerable<string>> SearchCustomers(int userId, string userName, string agentListXml,
                                                  bool isIncludeDormant);
    }
}
