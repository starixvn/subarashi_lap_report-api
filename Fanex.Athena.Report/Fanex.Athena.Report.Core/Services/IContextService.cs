using System.Collections.Generic;

namespace Fanex.Athena.Report.Core.Services
{
    public interface IContextService
    {
        int LicenseeId { get; }
        string SiteName { get; }
        int UserId { get; }
        string Username { get; }
        IReadOnlyDictionary<int, int[]> AgentList { get; }
        IReadOnlyDictionary<int, string> CurrencyList { get; }
        bool IsAllowedCustomerComm { get; }
        bool IsEnableBA { get; }
        bool IsEnableSingleWallet { get; }
        bool IsFakeLicensee { get; }
        byte SubsidiaryViewType { get; }
        string ClientIP { get; }
        string Language { get; }
        IEnumerable<string> OfferedSubProductList { get; }
        bool IsInRoles(params string[] roles);
    }
}
