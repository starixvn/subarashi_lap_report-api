namespace Fanex.Athena.Report.BySite.Controllers
{
    using System.Threading.Tasks;
    using Common.Settings;
    using Core.Attributes;
    using Core.Constants;
    using Core.Services.Interfaces;
    using MetaModels;
    using Microsoft.AspNetCore.Mvc;
    using Services.Interfaces;

    [ApiController, Route("api/[controller]/[action]"),
     BearerAuthorize(Roles.PortalSuperAdmin, Roles.SettledMatchCount),]
    public class SettledMatchCountController : Controller
    {
        private readonly AppSettings _appSettings;

        private readonly IContextService _contextService;

        private readonly IProductService _productService;

        private readonly ISettledMatchCountService _service;

        public SettledMatchCountController(ISettledMatchCountService service, IContextService contextService,
                                           IProductService productService, AppSettings appSettings)
        {
            _service = service;
            _contextService = contextService;
            _productService = productService;
            _appSettings = appSettings;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var productAccess = _productService.GetProductAccess(_contextService.LicenseeId, _contextService.Language);

            var metaModel = new SettledMatchCountMetaModel
            {
                IsShowAll = _appSettings.IsShowAllDataForSettleMatchCountReport,
                LicenseeId = _contextService.LicenseeId,
                ProductAccess = productAccess
            };

            return Ok(await _service.GetDataList(metaModel));
        }
    }
}
