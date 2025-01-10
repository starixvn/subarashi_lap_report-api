using Fanex.Athena.Report.Common.Settings;
using Fanex.Athena.Report.Core.Extensions;
using Fanex.Athena.Report.Core.Models.DTOs;
using Fanex.Athena.Report.Core.Repositories.Abstraction;
using Fanex.Athena.Report.Core.Services.Interfaces;
using Fanex.Athena.Report.Resources.Constants;
using Fanex.Athena.Report.Resources.Helpers;
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
        public static readonly string MemoryCacheCommonProduct = "MemoryCache_Common_Product";

        private readonly IMemoryCache _cache;

        private readonly IProductRepository _productRepository;

        private readonly IWebHostEnvironment _webHostEnvironment;

        private readonly AppSettings _appSetting;

        private const int DefaultTimeExpired = 5;

        public ProductService(IMemoryCache cache, IProductRepository productRepository, AppSettings appSettings, IWebHostEnvironment webHostEnvironment)
        {
            _cache = cache;
            _productRepository = productRepository;
            _appSetting = appSettings;
            _webHostEnvironment = webHostEnvironment;
        }

        #region Public Methods and Operators

        public IList<ProductInfo> GetProductList(int licenseeId)
        {
            var cacheKey = MemoryCacheCommonProduct + licenseeId;
            var productList = _cache.Get<IList<ProductInfo>>(cacheKey);

            if (productList != null)
            {
                MapProductName(productList);
                return productList;
            }

            productList = _productRepository.GetProductAccessList(licenseeId);
            _cache.Set(cacheKey, productList, TimeSpan.FromMinutes(DefaultTimeExpired));

            MapProductName(productList);
            return productList;
        }

        public ProductAccess GetProductAccess(int licenseeId, string functionConfigKey = null)
        {
            var allowedProducts = GetAllowedProducts(functionConfigKey);

            var originalProducts = GetProductList(licenseeId);

            var productList = allowedProducts.Any() ? originalProducts.Where(i => allowedProducts.Contains(i.ProductCode)).ToList()
                : originalProducts;
            return new ProductAccess(productList);
        }

        public ProductAccess GetSingleWalletProductAccess(int licenseeId, string functionConfigKey = null)
        {
            var allowedProducts = GetAllowedProducts(functionConfigKey);

            var originalProducts = GetProductList(licenseeId);

            var productList = allowedProducts.Any() ? originalProducts.Where(i => i.IsOfferedSingleWallet && allowedProducts.Contains(i.ProductCode)).ToList()
                : originalProducts;
            return new ProductAccess(productList);
        }

        #endregion Public Methods and Operators

        public string[] GetAllowedProducts(string functionName = null)
        {
            const string functionAllowedProductsKey = "FunctionAllowedProducts";

            var functions = _cache.Get<IEnumerable<XElement>>(functionAllowedProductsKey);
            if (functions == null)
            {
                var filePath = Path.GetFullPath(Path.Join(_webHostEnvironment.ContentRootPath, _appSetting.FunctionAllowedProductFilePath));
                functions = XDocument.Load(filePath).Descendants(nameof(Function));
                if (functions != null && functions.Any())
                {
                    _cache.Set(functionAllowedProductsKey, functions);
                }
            }

            var configFunctionName = string.IsNullOrWhiteSpace(functionName) ? "_Default" : functionName;
            var config = functions.FirstOrDefault(x => x.Attribute("Name").Value.Equals(configFunctionName, StringComparison.OrdinalIgnoreCase));
            return config == null ? new string[] { } : config.Attribute(nameof(Product)).Value.Split(',')
                    .Select(x => x.Trim()).Where(x => !string.IsNullOrEmpty(x)).ToArray();
        }

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

        private static void MapProductName(IList<ProductInfo> productList)
            => productList.ToList().ForEach(p => p.ProductName = DataHelper.GetProductName(p.ProductCode));

    }
}