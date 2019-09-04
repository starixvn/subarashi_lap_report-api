namespace Fanex.Athena.Report.BySite.MetaModels
{
    using System;
    using System.Collections.Generic;

    using Fanex.Athena.Report.Common.Model.DTO;

    public class PlatformWinLossMetaModel
    {
        public int CurrencyId { get; set; }

        public int PlatformType { get; set; }

        public int DrawFilter { get; set; }

        public int Year { get; set; }

        public int Month { get; set; }

        public int FromDay { get; set; }

        public int ToDay { get; set; }

        public DateTime FromDate
        {
            get { return new DateTime(Year, Month, FromDay); }
        }

        public DateTime ToDate
        {
            get { return new DateTime(Year, Month, ToDay); }
        }

        public string AgentListXml { get; set; }

        public IList<ProductOption> ProductOptions { get; set; }

        public IList<Currency> CurrencyList { get; set; }

        public string BaseCurrency { get; set; }

        public string CurrencyName { get; set; }

        public string SortBy { get; set; }

        public string SortOrder { get; set; }

        public bool HasComm { get; set; }


    }
}
