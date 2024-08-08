using Fanex.Athena.Report.Common.Excel.Interfaces;

namespace Fanex.Athena.Report.BySite.Excels.Options
{
    public class ThirdPartyWinLossExcelOption : IConvertingOption
    {
        public string BaseCurrency { get; set; }

        public string CurrencyName { get; set; }

        public bool IsConvert { get; set; }
    }
}
