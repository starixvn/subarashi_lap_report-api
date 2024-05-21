using System.Collections.Generic;
using Fanex.Athena.Report.Common.Model.DTO;

namespace Fanex.Athena.Report.Common.Model.MetaModels
{
    public interface IProductOptionsMetaModel
    {
        IEnumerable<ProductOption> ProductOptions { get; set; }

        IEnumerable<ProductOption> OriginalProductOptions { get; set; }
    }
}