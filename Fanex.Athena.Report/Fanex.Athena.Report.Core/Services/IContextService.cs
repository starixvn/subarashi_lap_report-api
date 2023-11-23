using System.Collections.Generic;
using Fanex.Athena.Report.Common.Model.DTO;

namespace Fanex.Athena.Report.Core.Services
{
    public interface IContextService
    {
        string SiteName { get; }
        string UserName { get; }
        string SiteDBServer { get; }
        IReadOnlyDictionary<int, int[]> AgentList { get; }
        IReadOnlyDictionary<int, string> CurrencyList { get; }
        bool IsAllowedCustomerComm { get; }
        bool IsEnableBA { get; }
        bool IsEnableSingleWallet { get; }
        bool IsFakeLicensee { get; }
        byte SubsidiaryViewType { get; }
        IEnumerable<string> OfferedSubProductList { get; }

        bool IsInRoles(params string[] roles);

        bool ForceIgnoreSharding { get; }
        public IEnumerable<AuditCurrency> AuditCurrencies { get; }

        public int SpecialPlatform { get; }
    }
}