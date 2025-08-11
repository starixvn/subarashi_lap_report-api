namespace Fanex.Athena.Report.Core.Services.Interfaces
{
    using System.Collections.Generic;
    using Common.Model.DTO;
    using Enums;

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

        IEnumerable<string> OfferedOddsGroupsList { get; }

        bool IsShowIpVn { get; }

        bool IsShowIp { get; }

        bool IsHideSaba { get; }

        AppMode ApplicationMode { get; }

        string OperatorId { get; }

        string ClientIP { get; }
    }
}