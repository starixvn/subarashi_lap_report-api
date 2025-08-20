namespace Fanex.Athena.Report.ByPlayer.Service.Interfaces
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public interface ISearchCustomerService
    {
        Task<IEnumerable<string>> SearchCustomers(int userId, string userName, string agentListXml,
                                                  bool isIncludeDormant);
    }
}
