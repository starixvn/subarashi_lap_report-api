using Fanex.Athena.Report.Core.Attributes;
using Fanex.Athena.Report.Core.Constants;
using Fanex.Athena.Report.Core.Helpers;
using Fanex.Athena.Report.Core.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Fanex.Athena.Report.Base.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class CommonDataController : Controller
    {
        private readonly IContextService _contextService;

        private readonly IProductService _productService;

        private readonly IBetTypeService _betTypeService;

        private readonly ISportService _sportService;

        public CommonDataController(IContextService contextService, IProductService productService, IBetTypeService betTypeService, ISportService sportService)
        {
            _contextService = contextService;
            _productService = productService;
            _betTypeService = betTypeService;
            _sportService = sportService;
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
                ? await _sportService.GetSportAndProductAndSabaSport(licenseeId, lang, productCode, isHideSaba)
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
    }
}
