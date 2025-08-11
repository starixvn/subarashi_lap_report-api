namespace Fanex.Athena.Report.Core.Repositories.Abstraction
{
    using System.Collections.Generic;
    using Models.DTOs;

    public interface IProductRepository
    {
        IList<ProductInfo> GetProductAccessList(int licenseeId);
    }
}
