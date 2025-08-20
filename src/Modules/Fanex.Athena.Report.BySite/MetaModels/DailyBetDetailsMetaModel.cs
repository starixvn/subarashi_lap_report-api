namespace Fanex.Athena.Report.BySite.MetaModels
{
    using System;
    using System.Collections.Generic;
    using Fanex.Core.Utils;

    public class DailyBetDetailsMetaModel : ICurrencyIdListMetaModel, IValidationMetaModel
    {
        public string SiteName { get; set; }

        public DateTime Date { get; set; }

        public long RefNo { get; set; }

        public int PageIndex { get; set; }

        public int PageSize { get; set; }

        public string SortBy { get; set; }

        public string SortOrder { get; set; }

        public string CurrencyIdList { get; set; }

        public IReadOnlyDictionary<int, string> CurrencyList { get; set; }

        public bool IsValid => !string.IsNullOrWhiteSpace(CurrencyIdList);

        public DailyBetDetailsMetaModel ShallowCopy() => (DailyBetDetailsMetaModel)MemberwiseClone();
    }
}