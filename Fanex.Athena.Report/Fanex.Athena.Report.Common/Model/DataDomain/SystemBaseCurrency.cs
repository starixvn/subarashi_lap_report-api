using System.Collections.Generic;

namespace Fanex.Athena.Report.Common.Model.DataDomain
{
    public static class SystemBaseCurrency
    {
        public const string EUR = "EUR";

        public static IDictionary<int, string> BaseCurrencyList { get; } = new Dictionary<int, string>
        {
            { BaseCurrencyType.Eur, EUR },
            { BaseCurrencyType.Usd, "USD" },
            { BaseCurrencyType.Gbp, "GBP" },
            { BaseCurrencyType.MYR, "MYR" },
            { BaseCurrencyType.CNY, "CNY" },
            { BaseCurrencyType.THB, "THB" },
            { BaseCurrencyType.IDR, "IDR" },
            { BaseCurrencyType.INH, "INH" },
        };

        public static IEnumerable<string> DefaultBaseCurrencyList { get; } = new[] { "GBP", EUR, "USD" };
    }
}