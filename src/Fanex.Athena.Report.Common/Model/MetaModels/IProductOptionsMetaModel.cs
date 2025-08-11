namespace Fanex.Athena.Report.Common.Model.MetaModels
{
    using System.Collections.Generic;
    using DTO;

    public interface IProductOptionsMetaModel
    {
        IEnumerable<ProductOption> ProductOptions { get; set; }

        IEnumerable<ProductOption> OriginalProductOptions { get; set; }
    }
}