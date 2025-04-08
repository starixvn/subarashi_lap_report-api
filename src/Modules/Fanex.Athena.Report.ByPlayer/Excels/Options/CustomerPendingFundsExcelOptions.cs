namespace Fanex.Athena.Report.ByPlayer.Excels.Options
{
    using Common.Excel.Interfaces;

    public class CustomerPendingFundsExcelOptions : IConvertingOption
    {
        public bool SystemIdVisible { get; set; }

        public string BaseCurrency { get; set; }

        public string CurrencyName { get; set; }

        public bool IsConvert { get; set; }
    }
}