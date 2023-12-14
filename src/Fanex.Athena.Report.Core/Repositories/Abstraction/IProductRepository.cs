using System.Collections.Generic;
using Fanex.Athena.Report.Core.Models.DTOs;

namespace Fanex.Athena.Report.Core.Repositories.Abstraction
{
    public interface IProductRepository
    {
        IList<ProductInfo> GetProductAccessList(int licenseeId);
    }
}
