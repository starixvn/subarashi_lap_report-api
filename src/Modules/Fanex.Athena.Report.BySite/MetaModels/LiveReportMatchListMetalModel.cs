using Fanex.Athena.Report.Common.Helpers;
using Fanex.Athena.Report.Common.Model.DataDomain;
using Fanex.Athena.Report.Common.Model.DTO;
using Fanex.Core.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Fanex.Athena.Report.BySite.MetaModels
{
    public class LiveReportMatchListMetalModel : IAgentMetaModel, ICurrencyIdMetaModel
    {
        protected const int DefaultPageSize = 1000;

        public LiveReportMatchListMetalModel()
        {
            ProductOptions = new List<ProductOption>();
            FromDate = DateTimeHelper.Now;
            ToDate = DateTimeHelper.Now;
        }


        [Required]
        public int SportId { get; set; }

        public bool IsConvert { get; set; }

        public string AgentListXml { get; set; }

        public string CurrencyName { get; set; }

        public bool IsViewedHistory { get; set; }

        public int LiveIndicator { get; set; }

        public string ProductCodeList { get; set; }

        public string OriginalProductCodeList { get; set; }

        public bool IsExportExcel { get; set; }

        public int PageSize { get; set; }

        public int PageIndex { get; set; }

        public string SubsidiaryList { get; set; }

        public bool SubsidiarySearch { get; set; }

        public int CurrencyId { get; set; }

        public string BaseCurrency { get; set; }

        public DateTime FromDate { get; set; }

        public DateTime ToDate { get; set; }

        public IList<ProductOption> ProductOptions { get; set; }

        public IList<ProductOption> OriginalProductOptions { get; set; }

        public string ProductCode { get; set; }

        public string SortDirection { get; set; }

        public string SortColumn { get; set; }

        public bool IsValid
        {
            get
            {
                return FromDate <= ToDate
                    && (string.IsNullOrEmpty(BaseCurrency) || SystemBaseCurrency.BaseCurrencyList
                                                                             .Any(x => x.Value.Equals(BaseCurrency, StringComparison.OrdinalIgnoreCase)));
            }
        }

    }
}
