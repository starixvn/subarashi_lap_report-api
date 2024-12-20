namespace Fanex.Athena.Report.BySite.MetaModels
{
    using Fanex.Core.Utils;
    using System;
    using System.Collections.Generic;

    public class DailyBetsMetaModel : ICurrencyIdListMetaModel, IValidationMetaModel
    {
        private const string TotalBaseCurrency = "USD";

        public string LoginName { get; set; }

        public string SiteName { get; set; }

        public DateTime Date { get; set; }

        public string ProductCodeList { get; set; }

        public long RefNo { get; set; }

        public string BaseCurrency { get; set; } = TotalBaseCurrency;

        public string GameCodes { get; set; }

        public int PageIndex { get; set; }

        public int PageSize { get; set; }

        public string SortBy { get; set; }

        public string SortOrder { get; set; }

        public string CurrencyIdList { get; set; }

        public IReadOnlyDictionary<int, string> CurrencyList { get; set; }

        public bool IsValid => !string.IsNullOrWhiteSpace(ProductCodeList) && !string.IsNullOrWhiteSpace(CurrencyIdList);

        public DailyBetsMetaModel ShallowCopy() => (DailyBetsMetaModel)this.MemberwiseClone();
    }
}