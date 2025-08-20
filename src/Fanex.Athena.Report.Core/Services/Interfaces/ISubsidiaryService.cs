namespace Fanex.Athena.Report.Core.Services.Interfaces
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Models.DTOs;

    public interface ISubsidiaryService
    {
        Task<IDictionary<int, int[]>> GetAgentList(string subsidiaryList);

        Task<IDictionary<int, IEnumerable<int>>> GetCurrencyAgentList(string licenseeName, string subsidiaryNames,
                                                                      bool isExcludeTest = false, int currencyId = 0);

        Task<IEnumerable<CheckedItem>> GetWhiteLabelTree(string licenseeName);

        Task<IEnumerable<CheckedItem>> GetSubsidiaryTree(string licenseeName, bool ignoreViewType = false);

        Task<IEnumerable<GroupSubsidiary>> GetGroupSubsidiaryList(string licenseeName, string subsidiaryList,
                                                                  string whiteLabelList);

        Task<IEnumerable<Subsidiary>> GetSubsidiaryList(string licenseeName, string subsidiaryList);

        bool IsIgnoredSubsidiarySearch(bool subsidiarySearch, string subsidiaryList);
    }
}
