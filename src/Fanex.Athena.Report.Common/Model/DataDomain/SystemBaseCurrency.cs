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
            { BaseCurrencyType.PKR, "PKR" },

            // crypto currencies
            { BaseCurrencyType.USDT, "USDT" }
        };

        public static IEnumerable<string> DefaultBaseCurrencyList { get; } = new[] { "GBP", EUR, "USD" };

        public static IDictionary<int, string> CryptoBaseCurrencyList { get; } = new Dictionary<int, string>
        {
            { BaseCurrencyType.USDT, "USDT" }
        };

        public static IDictionary<int, string> CryptoSupportEurBaseCurrencyList { get; } = new Dictionary<int, string>
        {
            { BaseCurrencyType.Eur, "EUR" },
            { BaseCurrencyType.MYR, "MYR" },
            { BaseCurrencyType.CNY, "CNY" }
        };

        public static IDictionary<int, string> JollySportsBaseCurrencyList { get; } = new Dictionary<int, string>
        {
            { BaseCurrencyType.Eur, "EUR" },
            { BaseCurrencyType.Usd, "USD" },
            { BaseCurrencyType.Gbp, "GBP" },
            { BaseCurrencyType.MYR, "MYR" },
            { BaseCurrencyType.CNY, "CNY" },
            { BaseCurrencyType.THB, "THB" },
            { BaseCurrencyType.IDR, "IDR" },
            { BaseCurrencyType.INH, "INH" },
            { BaseCurrencyType.BDT, "BDT" },
            { BaseCurrencyType.INR, "INR" },
            { BaseCurrencyType.MP, "MP" },
            { BaseCurrencyType.NPR, "NPR" },
            { BaseCurrencyType.TRY, "TRY" },
            { BaseCurrencyType.JPY, "JPY" },
            { BaseCurrencyType.MXN, "MXN" },
            { BaseCurrencyType.NGN, "NGN" },
            { BaseCurrencyType.BRL, "BRL" },
            { BaseCurrencyType.PKR, "PKR" }
        };

        public static IDictionary<int, string> FlatBaseCurrencyList { get; } = new Dictionary<int, string>
        {
            { BaseCurrencyType.Eur, "EUR" },
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
            { BaseCurrencyType.NPR, "NPR" },
            { BaseCurrencyType.TRY, "TRY" },
            { BaseCurrencyType.JPY, "JPY" },
            { BaseCurrencyType.MXN, "MXN" },
            { BaseCurrencyType.NGN, "NGN" },
            { BaseCurrencyType.BRL, "BRL" },
            { BaseCurrencyType.PKR, "PKR" }
        };
    }
}