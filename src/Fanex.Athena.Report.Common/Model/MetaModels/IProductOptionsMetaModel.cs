using Fanex.Athena.Report.Common.Model.DTO;
using System.Collections.Generic;

namespace Fanex.Athena.Report.Common.Model.MetaModels
{
    public interface IProductOptionsMetaModel
    {
        IEnumerable<ProductOption> ProductOptions { get; set; }

        IEnumerable<ProductOption> OriginalProductOptions { get; set; }
    }
}