namespace Fanex.Athena.Report.BySite.MetaModels
{
    using System;

    public class DailyBetsMetaModel
    {
        public string SiteName { get; set; }

        public DateTime Date { get; set; }

        public string ProductCodes { get; set; }

        public string CurrencyList { get; set; }

        public string BaseCurrency { get; set; }

        public int PageIndex { get; set; }

        public int PageSize { get; set; }

        public string SortBy { get; set; }

        public string SortOrder { get; set; }
    }
}