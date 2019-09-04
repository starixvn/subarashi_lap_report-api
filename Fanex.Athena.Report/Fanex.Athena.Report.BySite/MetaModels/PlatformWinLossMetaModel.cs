namespace Fanex.Athena.Report.BySite.MetaModels
{
    using System;
    using System.Collections.Generic;

    public class PlatformWinLossMetaModel
    {
        private DateTime _moveBetsDay;

        public int CurrencyId { get; set; }

        public int PlatformType { get; set; }

        public DateTime MoveBetsDay
        {
            set
            {
                _moveBetsDay = value.ToLocalTime();
            }
            get
            {
                return _moveBetsDay;
            }
        }

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

        public string BaseCurrency { get; set; }

        public string CurrencyName { get; set; }
    }
}
