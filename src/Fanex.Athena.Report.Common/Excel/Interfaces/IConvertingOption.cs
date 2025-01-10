namespace Fanex.Athena.Report.Common.Excel.Interfaces
{
    public interface IConvertingOption
    {
        public string BaseCurrency { get; set; }

        public string CurrencyName { get; set; }

        public bool IsConvert { get; set; }
    }
}
