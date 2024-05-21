using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Fanex.Athena.Report.Common.Services.Interfaces;
using Fanex.Athena.Report.Core.Constants;
using Fanex.Athena.Report.Core.Models.DTOs;
using Fanex.Athena.Report.Core.Services.Interfaces;
using Fanex.Athena.Report.Core.Utility.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;

namespace Fanex.Athena.Report.Base.Controllers
{
    [Route("api/[controller]/[action]")]
    public class UserGuideController : Controller
    {
        private readonly IWebHostEnvironment _hostEnvironment;

        protected const string DefaultLanguage = "en-US";

        private static readonly HashSet<string> ListResourceNoBindData = new ()
        {
            FunctionNames.SettledMatchCount.ToLowerInvariant(),
            FunctionNames.WeeklyLiveMatch.ToLowerInvariant(),
        };

        private readonly IContextService _contextService;
        private readonly ICacheRefAdapter _cacheRefAdapter;
        private readonly IFileSystem _fileSystem;

        private readonly IProductService _productService;

        public UserGuideController(
            IContextService contextService,
            IFileSystem fileSystem, 
            IWebHostEnvironment hostEnvironment, 
            IProductService productService)
        {
            _contextService = contextService;
            _fileSystem = fileSystem;
            _hostEnvironment = hostEnvironment;
            _productService = productService;
        }

        [HttpGet]
        public ActionResult<string> Index(string resourceName)
        {
            var content = GetFileContent(resourceName);
            if (ListResourceNoBindData.Contains(resourceName.ToLowerInvariant()))
            {
                return content;
            }

            var products = _productService.GetProductList(_contextService.LicenseeId, _contextService.Language);
            content = AddSpecialProducts(content, products);
            content = AddOpenBalanceInfo(content);
            
            return content;
        }

        private string GetDefaultFilePathIfNotExist(string language, string filePath)
        {
            if (!language.Equals(DefaultLanguage, StringComparison.OrdinalIgnoreCase) && !_fileSystem.Exists(filePath))
            {
                return filePath.Replace($"{language}.htm", $"{DefaultLanguage}.htm");
            }

            return filePath;
        }

        private static string AddSpecialProducts(string content, IEnumerable<ProductInfo> products)
        {
            content = RemoveProductSection(content, products, ProductTypes.VirtualSports2, "VS2");

            return content;
        }

        private static string RemoveProductSection(string content, IEnumerable<ProductInfo> products, int productId, string productCode)
        {
            var openingTag = "{" + productCode + "-productlist-section}";

            var customContent = content;
            if (customContent.Contains(openingTag))
            {
                if (products.Any(x => x.ProductID == productId))
                {
                    var closingTag = "{/" + productCode + "-productlist-section}";
                    customContent = customContent.Replace(openingTag, string.Empty);
                    customContent = customContent.Replace(closingTag, string.Empty);
                }
                else
                {
                    var pattern = @"\{" + productCode + @"-productlist-section\}([\s\S]*?){/" + productCode + @"-productlist-section\}";
                    customContent = new Regex(pattern).Replace(customContent, string.Empty);
                }
            }

            return customContent;
        }

        private string AddOpenBalanceInfo(string content)
            => Replace(content, "{open-balance-section}", "{/open-balance-section}", !_contextService.IsEnableSingleWallet);

        private string GetFileContent(string resourceName)
        {
            var language = _contextService.Language;

            var fullFilePath = Path.GetFullPath(Path.Join(_hostEnvironment.ContentRootPath,
                                                          $@"/App_Data/UserGuide/{resourceName}/{language}.htm"));
            var filePath = GetDefaultFilePathIfNotExist(language, fullFilePath);

            return _fileSystem.ReadFile(filePath);
        }

        private static string Replace(string content, string openingSectionTag, string closingSectionTag, bool shouldKeepSection)
        {
            var result = content;
            if (content.Contains(openingSectionTag))
            {
                if (shouldKeepSection)
                {
                    result = result.Replace(openingSectionTag, string.Empty);
                    result = result.Replace(closingSectionTag, string.Empty);
                }
                else
                {
                    result = new Regex($@"{openingSectionTag}([\s\S]*?){closingSectionTag}").Replace(result, string.Empty);
                }
            }

            return result;
        }
    }
}