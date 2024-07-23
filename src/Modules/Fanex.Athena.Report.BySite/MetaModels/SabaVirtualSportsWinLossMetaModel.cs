namespace Fanex.Athena.Report.BySite.MetaModels
{
    using System;
    using System.Collections.Generic;
    using Common.Helpers;
    using Common.Model.DTO;
    using Fanex.Core.Utils;

    public class SabaVirtualSportsWinLossMetaModel : BaseWinLossMetaModel, ICurrencyIdMetaModel
    {
        public SabaVirtualSportsWinLossMetaModel()
        {
            FromDate = DateTimeHelper.Now;
            ToDate = DateTimeHelper.Now;
        }

        public int Sport { get; set; }

        public int League { get; set; }

        public string UserName { get; set; } = string.Empty;

        public string DefaultBaseCurrency { get; set; }

        public string CustId { get; set; }

        public int DrillDownLevel { get; set; }

        public string CurrencyName { get; set; }

        public IList<ProductOption> ProductOptions { get; set; }

        public int LicenseeId { get; set; }

        public string BaseCurrency { get; set; }

        public bool IsConvert { get; set; }

        public override bool IsValid
        {
            get
            {
                const string MinDate = "08/01/2018";
                var minDate = DateTimeOffset.Parse(MinDate).Date;
                var maxDate = DateTimeHelper.Now.Date;

                return FromDate <= ToDate && DateTimeHelper.IsBetween(FromDate, minDate, maxDate) &&
                       DateTimeHelper.IsBetween(ToDate, minDate, maxDate);
            }
        }

        public int CurrencyId { get; set; }
    }
}