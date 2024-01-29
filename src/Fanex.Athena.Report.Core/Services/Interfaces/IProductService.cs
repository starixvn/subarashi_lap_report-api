using System.Collections.Generic;
using Fanex.Athena.Report.Core.Models.DTOs;

namespace Fanex.Athena.Report.Core.Services.Interfaces
{
    public interface IProductService
    {
        ProductAccess GetProductAccess(int licenseeId, string functionConfigKey = null);

        ProductAccess GetSingleWalletProductAccess(int licenseeId, string functionConfigKey = null);

        IList<ProductInfo> GetProductList(int licenseeId);

        string GetAllowedSubProductCodeList(IEnumerable<string> offeredSubProductList, string productCodes);
    }
}