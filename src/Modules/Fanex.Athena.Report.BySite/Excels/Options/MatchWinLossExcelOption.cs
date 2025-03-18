using Fanex.Athena.Report.Common.Excel.Interfaces;

namespace Fanex.Athena.Report.BySite.Excels.Options
{
    public class MatchWinLossExcelOption : IConvertingOption
    {
        public bool IsHideSaba { get; set; }

        public bool CustomerCommVisible { get; set; }

        public bool IsConvert { get; set; } = true;

        public string BaseCurrency { get; set; } = @"EUR";

        public string CurrencyName { get; set; }
    }
}