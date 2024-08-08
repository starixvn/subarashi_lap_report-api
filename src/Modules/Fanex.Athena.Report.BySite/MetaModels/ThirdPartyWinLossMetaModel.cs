namespace Fanex.Athena.Report.BySite.MetaModels
{
    using System;
    using Common.Helpers;

    public class ThirdPartyWinLossMetaModel : BaseWinLossMetaModel
    {
        public int Year { get; set; }

        public int Month { get; set; }

        public int FromDay { get; set; }

        public int ToDay { get; set; }

        public int DrillDownLevel { get; set; }

        public new DateTime FromDate { get; set; }

        public new DateTime ToDate { get; set; }

        public override bool IsValid => MetaModelHelper.ValidateBaseCurrency(BaseCurrency);

        public int ProductId { get; set; }

        public string ProductCode { get; set; }

        public string ProductName { get; set; }

        public string BaseCurrency { get; set; }

        public DateTime? MoveBetsDay { get; set; }

        public DateTime? BalanceUpDay { get; set; }

        public string SubsidiaryList { get; set; }

        public bool SubsidiarySearch { get; set; }

        public int DrillDownCustomerId { get; set; }

        public string DefaultBaseCurrency { get; set; }

        public int CurrencyId { get; set; }

        public string CurrencyName { get; set; }
    }
}