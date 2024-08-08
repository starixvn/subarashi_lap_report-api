using Fanex.Athena.Report.Common.Excel.Interfaces;

namespace Fanex.Athena.Report.BySite.Excels.Options
{
    public class SabaVirtualSportsWinLossExcelOption : IConvertingOption
    {
        public bool SbVisible { get; set; }

        public bool BaVisible { get; set; }

        public bool CoVisible { get; set; }

        public bool CustomerCommVisible { get; set; }

        public string BaseCurrency { get; set; }

        public string CurrencyName { get; set; }

        public bool IsConvert { get; set; }
    }
}
