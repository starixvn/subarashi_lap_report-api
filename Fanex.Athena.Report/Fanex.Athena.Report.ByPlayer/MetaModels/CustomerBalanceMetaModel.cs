namespace Fanex.Athena.Report.ByPlayer.MetaModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Fanex.Athena.Report.Common.Model.DataDomain;
    using Fanex.Athena.Report.Common.Model.DTO;
    using Fanex.Athena.Report.Common.Model.Helper;

    public class CustomerBalanceMetaModel
    {
        private string _baseCurrency = string.Empty;

        public int CurrencyId { get; set; }

        public bool IsExportExcel { get; set; }

        public int LicenseeId { get; set; }

        public int UserId { get; set; }

        public int PageSize { get; set; }

        public int PageIndex { get; set; }

        public DateTime FromDate { get; set; }

        public DateTime ToDate { get; set; }

        public bool IsExcludedTestData { get; set; }

        public IList<ProductOption> ProductOptions { get; set; }

        public int OrderItem { get; set; }

        public string ProductCodeList { get; set; } = string.Empty;

        public int TopCount { get; set; }

        public string AgentListXml { get; set; }

        public string BaseCurrency
        {
            get
            {
                return _baseCurrency;
            }

            set
            {
                _baseCurrency = value == null
                    ? string.Empty
                    : value.ToLowerInvariant();
            }
        }

        public int BaseCurrencyId
        {
            get
            {
                return SystemBaseCurrency.BaseCurrencyList
                     .Where(x => BaseCurrency.Equals(x.Value, StringComparison.OrdinalIgnoreCase))
                     .DefaultIfEmpty(new KeyValuePair<int, string>(BaseCurrencyType.All, string.Empty))
                     .Select(x => x.Key).FirstOrDefault();
            }
        }

        public string CurrencyName { get; set; }

        public bool IsConvert => SystemBaseCurrency.BaseCurrencyList.Any(t => t.Value.Equals(BaseCurrency, StringComparison.OrdinalIgnoreCase));

        public bool IsValid
        {
            get
            {
                return MetaModelValidationHelper.ValidateBaseCurrency(BaseCurrency) && FromDate <= ToDate
                    && ProductOptions.Any(p => p.Status);
            }
        }
    }
}