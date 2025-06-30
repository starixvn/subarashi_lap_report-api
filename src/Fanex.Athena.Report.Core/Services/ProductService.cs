using Fanex.Athena.Report.Common.Helpers;
using Fanex.Athena.Report.Common.Settings;
using Fanex.Athena.Report.Core.ApiClients.Abstraction;
using Fanex.Athena.Report.Core.Extensions;
using Fanex.Athena.Report.Core.Models.DTOs;
using Fanex.Athena.Report.Core.Repositories.Abstraction;
using Fanex.Athena.Report.Core.Services.Interfaces;
using Fanex.Athena.Report.Resources.Constants;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Caching.Memory;
using NPOI.SS.Formula.Functions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace Fanex.Athena.Report.Core.Services
{
    public class ProductService : IProductService
    {
        private readonly ICommonApiClient _commonApiClient;
        private const string ProductAccessUrl = "api/productaccess";
        private const string ProductResourceUrl = "api/productresource";

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