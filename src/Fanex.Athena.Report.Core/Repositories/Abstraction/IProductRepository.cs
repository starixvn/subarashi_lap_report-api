using Fanex.Athena.Report.Core.Models.DTOs;
using System.Collections.Generic;

namespace Fanex.Athena.Report.Core.Repositories.Abstraction
{
    public interface IProductRepository
    {
        IList<ProductInfo> GetProductAccessList(int licenseeId);
    }
}
