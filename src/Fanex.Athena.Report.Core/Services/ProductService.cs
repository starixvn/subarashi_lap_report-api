namespace Fanex.Athena.Report.Core.Services
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using ApiClients.Abstraction;
    using Extensions;
    using Interfaces;
    using Models.DTOs;
    using Resources.Constants;

    public class ProductService : IProductService
    {
        private readonly ICommonApiClient _commonApiClient;
        private const string ProductAccessUrl = "api/productaccess";
        private const string ProductResourceUrl = "api/productresource";
        private const string ProductSettingUrl = "api/productSetting";

        public ProductService(ICommonApiClient commonApiClient)
        {
            _commonApiClient = commonApiClient;
        }

        #region Public Methods and Operators

        private static object BuildProductAccessRequest(int licenseeId, string functionConfigKey) => new
        {
            licenseeId,
            functionKey = functionConfigKey
        };

        public IList<ProductInfo> GetProductList(int licenseeId, string functionConfigKey = null) =>
             _commonApiClient.GetAsync<IList<ProductInfo>>(
                ProductAccessUrl,
                BuildProductAccessRequest(licenseeId, functionConfigKey)).ConfigureAwait(false).GetAwaiter().GetResult();

        public string GetProductName(string productCode) =>
            _commonApiClient.GetAsync<string>(
                ProductResourceUrl,
                new { productCode }).ConfigureAwait(false).GetAwaiter().GetResult();

        public async Task<ProductSetting> GetProductSetting() =>
            await _commonApiClient.GetAsync<ProductSetting>(
                ProductSettingUrl).ConfigureAwait(false);

        public ProductAccess GetProductAccess(int licenseeId, string functionConfigKey = null)
        {
            var productList = GetProductList(licenseeId, functionConfigKey);

            return new ProductAccess(productList);
        }

        public ProductAccess GetSingleWalletProductAccess(int licenseeId, string functionConfigKey = null)
        {
            var allowedProducts = GetProductList(licenseeId, functionConfigKey);

            var productList = allowedProducts.Where(i => i.IsOfferedSingleWallet).ToList();

            return new ProductAccess(productList);
        }

        #endregion Public Methods and Operators

        public string GetAllowedSubProductCodeList(IEnumerable<string> offeredSubProductList, string productCodes)
        {
            if (!offeredSubProductList.IsPresent() || !(productCodes?.Split(',')).IsPresent(_ => _ == Product.SB))
            {
                return productCodes;
            }

            return productCodes + "," + string.Join(",", offeredSubProductList);
        }

        public bool IsOfferCashOutProduct(int licenseeId)
        {
            var originalProducts = GetProductList(licenseeId);
            return originalProducts.Any(_ => _.ProductCode == Product.CO);
        }
    }
}