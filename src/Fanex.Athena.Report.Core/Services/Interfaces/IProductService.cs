using Fanex.Athena.Report.Core.Models.DTOs;
using System.Collections.Generic;

namespace Fanex.Athena.Report.Core.Services.Interfaces
{
    public interface IProductService
    {
        string GetProductName(string productCode);

        ProductAccess GetProductAccess(int licenseeId, string functionConfigKey = null);

        ProductAccess GetSingleWalletProductAccess(int licenseeId, string functionConfigKey = null);

        IList<ProductInfo> GetProductList(int licenseeId, string functionConfigKey = null);

        string GetAllowedSubProductCodeList(IEnumerable<string> offeredSubProductList, string productCodes);

        bool IsOfferCashOutProduct(int licenseeId);
    }
}