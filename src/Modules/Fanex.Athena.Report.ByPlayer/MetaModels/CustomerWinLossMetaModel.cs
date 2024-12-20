using Fanex.Athena.Report.Common.Model.DataDomain;
using Fanex.Athena.Report.Common.Model.DTO;
using Fanex.Athena.Report.Common.Model.MetaModels;
using Fanex.Core.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace Fanex.Athena.Report.ByPlayer.MetaModels
{
    public class CustomerWinLossMetaModel : IAgentMetaModel, IDataTestableMetaModel, IProductOptionsMetaModel, ICurrencyIdMetaModel, IValidationMetaModel, IPlayerComMetaModel
    {
        private string _baseCurrency = string.Empty;

        public bool IsExportExcel { get; set; }

        public int PageSize { get; set; }

        public int PageIndex { get; set; }

        public DateTime FromDate { get; set; }

        public DateTime ToDate { get; set; }

        public IEnumerable<ProductOption> ProductOptions { get; set; }

        public IEnumerable<ProductOption> OriginalProductOptions { get; set; }

        public int OrderItem { get; set; }

        public bool IsAscSort { get; set; }

        public string Username { get; set; }

        public string AgentListXml { get; set; }

        public bool IsExcludedTestData { get; set; }

        public int CurrencyId { get; set; }

        [DefaultValue("")]
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

        [DefaultValue("")]
        public string CurrencyName { get; set; }

        [JsonIgnore]
        public bool IsConvert => SystemBaseCurrency.BaseCurrencyList.Any(t => t.Value.Equals(BaseCurrency, StringComparison.OrdinalIgnoreCase));

        public bool IsAllowedBA { get; set; }

        public bool HasPlayerCom { get; set; }

        public bool SystemIdVisible { get; set; }

        public string CustomerClass { get; set; }

        public string OddsGroup { get; set; }

        public bool IsValid => FromDate <= ToDate && ProductOptions.Any(p => p.Status);
    }
}