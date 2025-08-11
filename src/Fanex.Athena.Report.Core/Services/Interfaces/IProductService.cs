namespace Fanex.Athena.Report.Core.Services.Interfaces
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Models.DTOs;

    public interface IProductService
    {
        string GetProductName(string productCode);

        ProductAccess GetProductAccess(int licenseeId, string functionConfigKey = null);

        ProductAccess GetSingleWalletProductAccess(int licenseeId, string functionConfigKey = null);

        IList<ProductInfo> GetProductList(int licenseeId, string functionConfigKey = null);

        Task<ProductSetting> GetProductSetting();

        string GetAllowedSubProductCodeList(IEnumerable<string> offeredSubProductList, string productCodes);

        bool IsOfferCashOutProduct(int licenseeId);
    }
}