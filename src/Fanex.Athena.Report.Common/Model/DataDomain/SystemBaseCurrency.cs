namespace Fanex.Athena.Report.Common.Model.DataDomain
{
    using System.Collections.Generic;

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
            { BaseCurrencyType.BDT, "BDT" },
            { BaseCurrencyType.INR, "INR" },
            { BaseCurrencyType.PHP, "PHP" },
            { BaseCurrencyType.MP, "MP" },
            { BaseCurrencyType.NPR, "NPR" },
            { BaseCurrencyType.TRY, "TRY" },
            { BaseCurrencyType.JPY, "JPY" },
            { BaseCurrencyType.MXN, "MXN" },
            { BaseCurrencyType.NGN, "NGN" },
            { BaseCurrencyType.BRL, "BRL" },

            // crypto currencies
            { BaseCurrencyType.USDT, "USDT" }
        };

        public static IEnumerable<string> DefaultBaseCurrencyList { get; } = new[] { "GBP", EUR, "USD" };
    }
}