using Fanex.Athena.Report.Common.Model.DTO;
using Fanex.Athena.Report.Core.Enums;
using System.Collections.Generic;

namespace Fanex.Athena.Report.Core.Services.Interfaces
{
    public interface IContextService
    {
        int LicenseeId { get; }

        string Language { get; }

        string SiteName { get; }

        string UserName { get; }

        string SiteDBServer { get; }

        int UserId { get; }

        byte UserLevel { get; }

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

        public bool IsSuperAdmin { get; }

        public int SpecialPlatformType { get; }

        int WhiteLabelID { get; }

        string DefaultBaseCurrency { get; }

        int DefaultBaseCurrencyId { get; }

        IEnumerable<string> OfferedOddsGroupsList { get; }

        bool IsShowIpVn { get; }

        bool IsShowIp { get; }

        bool IsHideSaba { get; }

        AppMode ApplicationMode { get; }
    }
}