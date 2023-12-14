using System.Collections.Generic;
using Fanex.Athena.Report.Core.Models.DTOs;

namespace Fanex.Athena.Report.Core.Services.Interfaces
{
    public interface IProductService
    {
        ProductAccess GetProductAccess(int licenseeId, string languageCode, string functionConfigKey = null);

        IList<ProductInfo> GetProductList(int licenseeId, string languageCode);
    }
}