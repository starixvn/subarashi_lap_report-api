using Fanex.Athena.Report.Core.Helpers;
using Fanex.Athena.Report.Core.Models.DTOs;
using Fanex.Athena.Report.Core.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fanex.Athena.Report.Core.Attributes;
using Microsoft.Extensions.Caching.Memory;
using ServiceStack.Text;

namespace Fanex.Athena.Report.Base.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class CommonDataController : Controller
    {
        private const string MemoryCacheBetTypeForTreeView = "MemoryCache_BetType_TreeView";

        private readonly IContextService _contextService;

        private readonly IProductService _productService;

        private readonly IBetTypeService _betTypeService;

        private readonly ISportService _sportService;

        private readonly IMemoryCache _cache;

        public CommonDataController(IContextService contextService, IProductService productService, IBetTypeService betTypeService, ISportService sportService, IMemoryCache cache)
        {
            _contextService = contextService;
            _productService = productService;
            _betTypeService = betTypeService;
            _sportService = sportService;
            _cache = cache;
        }

        [HttpGet]
        public async Task<IEnumerable<object>> GetSports(string productCode, bool includeSabaSport = false, bool applyHideSaba = false)
        {
            productCode = _productService.GetAllowedSubProductCodeList(
                _contextService.OfferedSubProductList,
                productCode);

            var licenseeId = _contextService.LicenseeId;
            var lang = _contextService.Language;
            var isHideSaba = SabaHelper.IsHideSaba(_contextService.SpecialPlatform) && applyHideSaba;

            var sports = includeSabaSport
                ? await _sportService.GetSportAndProductAndSabaSport(licenseeId, lang, isHideSaba, productCode)
                : await _sportService.GetSportsAndProducts(licenseeId, lang, productCode);

            return sports.Where(x => x.ParentId > -1).Select(x => new { value = x.Id, label = x.Name, status = true });
        }

        [HttpGet]
        public async Task<IEnumerable<object>> GetBetTypes(string productCode, bool applyHideSaba = false)
        {
            productCode = _productService.GetAllowedSubProductCodeList(
                _contextService.OfferedSubProductList,
                productCode);
            var isHideSaba = SabaHelper.IsHideSaba(_contextService.SpecialPlatform) && applyHideSaba;

            var betTypes = await _betTypeService.GetBetTypesByProduct(
                _contextService.LicenseeId,
                productCode,
                _contextService.Language,
                isHideSaba).ConfigureAwait(false);

            return betTypes.Select(x => new { value = x.ID, label = x.Name, status = true });
        }

        [HttpGet]
        public async Task<IEnumerable<CheckedItem>> GetSportsForTreeView(string productCodes)
        {
            if (string.IsNullOrEmpty(productCodes))
            {
                return new List<CheckedItem>();
            }

            return await _sportService.GetSportsForTreeView(_contextService.LicenseeId, _contextService.Language, productCodes, _contextService.IsHideSaba);
        }

        [HttpGet]
        [CompressFilter(Order = 1)]
        public async Task<IActionResult> GetBetTypeInTreeList(string productCodes)
        {
            const int CacheTime = 60 * 60 * 12; //12 hour
            var cacheKey = $"{MemoryCacheBetTypeForTreeView}_{productCodes}_{_contextService.SiteName}_{_contextService.Language}";
            var jsonBetType = _cache.Get<string>(cacheKey);
            if (!string.IsNullOrWhiteSpace(jsonBetType))
            {
                return Json(JsonSerializer.DeserializeFromString<List<CheckedItem>>(jsonBetType));
            }

            if (string.IsNullOrEmpty(productCodes))
            {
                return Ok(new List<CheckedItem>());
            }

            JsConfig.ExcludeTypeInfo = true;
            var isHideSaba = SabaHelper.IsHideSaba(_contextService.SpecialPlatform);
            var betTypeList = await _betTypeService.GetBetTypeForProducts(_contextService.LicenseeId, _contextService.Language, productCodes, isHideSaba);
            var jsonString = JsonSerializer.SerializeToString(betTypeList);
            _cache.Set(cacheKey, jsonString, DateTimeHelper.Now.AddSeconds(CacheTime));

            return Ok(betTypeList);
        }
    }
}
