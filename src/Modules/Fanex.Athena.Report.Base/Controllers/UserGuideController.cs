namespace Fanex.Athena.Report.Base.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;
    using Common.Constants;
    using Common.Services.Interfaces;
    using Core.Constants;
    using Core.Models.DTOs;
    using Core.Services.Interfaces;
    using Core.Utility.Interfaces;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Mvc;
    using Resources.Constants;

    [Route("api/[controller]/[action]")]
    public class UserGuideController : ControllerBase
    {
        private const string LICENSEE_SUFFIX_SECTION = "licensee-section";

        private const string DefaultLanguage = "en-US";

        private static readonly HashSet<string> ListResourceNoBindData = new()
        {
            FunctionNames.ExtraPayout.ToLowerInvariant(),
            FunctionNames.SettledMatchCount.ToLowerInvariant(),
            FunctionNames.WeeklyLiveMatch.ToLowerInvariant()
        };

        private readonly IBalanceUpService _balanceUpService;

        private readonly IContextService _contextService;
        private readonly IFileSystem _fileSystem;
        private readonly IWebHostEnvironment _hostEnvironment;
        private readonly IProductService _productService;

        public UserGuideController(
            IContextService contextService,
            IBalanceUpService balanceUpService,
            IFileSystem fileSystem,
            IWebHostEnvironment hostEnvironment,
            IProductService productService)
        {
            _contextService = contextService;
            _fileSystem = fileSystem;
            _hostEnvironment = hostEnvironment;
            _productService = productService;
            _balanceUpService = balanceUpService;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string resourceName)
        {
            var content = GetFileContent(resourceName);

            if (ListResourceNoBindData.Contains(resourceName.ToLowerInvariant()))
            {
                return Ok(content);
            }

            var products = _productService.GetProductList(_contextService.LicenseeId);
            content = AddSpecialProducts(content, products);
            content = AddOpenBalanceInfo(content);
            content = await AddBalanceUpDateAsync(content);
            content = RemovePermissionSection(content, Roles.PlayerVIPManagement);
            content = RemoveSabaText(content);
            content = AddLicenseeSection(content, _contextService.SiteName);
            return Ok(content);
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
            content = RemoveProductSection(content, products, ProductTypes.CashOut, Product.CO);
            return content;
        }

        private static string RemoveProductSection(
            string content,
            IEnumerable<ProductInfo> products,
            int productId,
            string productCode)
        {
            var openingTag = "{" + productCode + "-productlist-section}";

            var customContent = content;

            if (!customContent.Contains(openingTag))
            {
                return customContent;
            }

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

            return customContent;
        }

        private string RemoveSabaText(string content)
        {
            const string OpeningTag = "{SABA-section}";
            const string ClosingTag = "{/SABA-section}";
            var customContent = content;

            if (!_contextService.IsHideSaba)
            {
                customContent = customContent.Replace(OpeningTag, string.Empty);
                customContent = customContent.Replace(ClosingTag, string.Empty);
            }
            else
            {
                const string pattern = $"{OpeningTag}[\\s\\S]*{ClosingTag}";
                customContent = new Regex(pattern).Replace(customContent, string.Empty);
            }

            return customContent;
        }

        private static string AddLicenseeSection(string content, string licenseeName)
        {
            var openingTag = $"{{{licenseeName}-{LICENSEE_SUFFIX_SECTION}}}";
            var closingTag = $"{{/{licenseeName}-{LICENSEE_SUFFIX_SECTION}}}";
            var customContent = content;

            if (content.Contains(openingTag))
            {
                customContent = Replace(customContent, openingTag, closingTag, true);
            }

            return RemoveUnShowLicenseeSection(customContent);
        }

        private static string RemoveUnShowLicenseeSection(string content)
        {
            var customContent = content;
            const string licenseeTagPattern = $"{{[\\w\\d]+-{LICENSEE_SUFFIX_SECTION}}}";
            var matchItems = new Regex(licenseeTagPattern).Matches(customContent)
                .Select(_ => _.Value);

            foreach (var matchItem in matchItems)
            {
                var removeLicenseeNameSection = matchItem.Replace($"-{LICENSEE_SUFFIX_SECTION}}}", string.Empty)
                    .Replace("{", string.Empty);
                var openingTag = $"{{{removeLicenseeNameSection}-{LICENSEE_SUFFIX_SECTION}}}";
                var closingTag = $"{{\\/{removeLicenseeNameSection}-{LICENSEE_SUFFIX_SECTION}}}";
                var removePattern = $"{openingTag}([\\s\\S]*?){closingTag}";
                customContent = new Regex(removePattern).Replace(customContent, string.Empty);
            }

            return customContent;
        }

        private string RemovePermissionSection(string content, string permission)
        {
            var openingTag = $"{{{permission}-permission-section}}";
            var closingTag = $"{{/{permission}-permission-section}}";
            var customContent = content;

            if (_contextService.IsInRoles(Roles.PortalSuperAdmin, permission))
            {
                customContent = customContent.Replace(openingTag, string.Empty);
                customContent = customContent.Replace(closingTag, string.Empty);
            }
            else
            {
                var pattern = $"{openingTag}[\\s\\S]*{closingTag}";
                customContent = new Regex(pattern).Replace(customContent, string.Empty);
            }

            return customContent;
        }

        private string AddOpenBalanceInfo(string content)
            => Replace(content, "{open-balance-section}", "{/open-balance-section}",
                !_contextService.IsEnableSingleWallet);

        private async Task<string> AddBalanceUpDateAsync(string content)
        {
            const string PlaceHolder = "{balance-up-date}";
            if (content.Contains(PlaceHolder))
            {
                var date = (await _balanceUpService.GetBalanceUpAsync()).BalanceUpDay;
                return content.Replace(PlaceHolder, date.ToString("MM/dd/yyyy"));
            }

            return content;
        }

        private string GetFileContent(string resourceName)
        {
            var language = _contextService.Language;

            var fullFilePath = Path.GetFullPath(Path.Join(_hostEnvironment.ContentRootPath,
                $@"/App_Data/UserGuide/{resourceName}/{language}.htm"));
            var filePath = GetDefaultFilePathIfNotExist(language, fullFilePath);

            return _fileSystem.ReadFile(filePath);
        }

        private static string Replace(
            string content,
            string openingSectionTag,
            string closingSectionTag,
            bool shouldKeepSection)
        {
            var result = content;

            if (!content.Contains(openingSectionTag))
            {
                return result;
            }

            if (shouldKeepSection)
            {
                result = result.Replace(openingSectionTag, string.Empty);
                result = result.Replace(closingSectionTag, string.Empty);
            }
            else
            {
                result = new Regex($@"{openingSectionTag}([\s\S]*?){closingSectionTag}").Replace(result, string.Empty);
            }

            return result;
        }
    }
}