namespace Fanex.Athena.Report.BySite.MetaModels
{
    using System;
    using System.Collections.Generic;
    using Fanex.Athena.Report.Common.Model.DTO;

    public class DailyBetsMetaModel
    {
        public string SiteName { get; set; }

        public DateTime Date { get; set; }

        public string ProductCodeList { get; set; }

        public IEnumerable<Currency> CurrencyList { get; set; }

        public string BaseCurrency { get; set; }

        public int PageIndex { get; set; }

        public int PageSize { get; set; }

        public string SortBy { get; set; }

        public string SortOrder { get; set; }

        public DailyBetsMetaModel ShallowCopy() => (DailyBetsMetaModel) this.MemberwiseClone();
    }
}