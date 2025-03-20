using Fanex.Athena.Report.Common.Helpers;
using Fanex.Athena.Report.Common.Model.DTO;
using Fanex.Athena.Report.Common.Model.MetaModels;
using Fanex.Core.Utils;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Fanex.Athena.Report.BySite.MetaModels
{
    public class TurnoverMetaModel : IValidationMetaModel, ICurrencyIdMetaModel, IProductOptionsMetaModel, IAgentMetaModel, IDataTestableMetaModel
    {
        public TurnoverMetaModel()
        {
            ProductOptions = new List<ProductOption>();
            FromDate = DateTimeHelper.Now;
            ToDate = DateTimeHelper.Now;
        }

        public string AgentListXml { get; set; }

        public DateTime FromDate { get; set; }

        public DateTime ToDate { get; set; }

        public string Username { get; set; }

        public string BaseCurrency { get; set; }

        public int CurrencyId { get; set; }

        public string CurrencyName { get; set; }

        public IEnumerable<ProductOption> ProductOptions { get; set; }

        public IEnumerable<ProductOption> OriginalProductOptions { get; set; }

        public bool IsExcludedTestData { get; set; }

        public bool IsValid => ProductOptions.Any() && FromDate <= ToDate;
    }
}