using System.Threading.Tasks;
using Fanex.Athena.Report.BySite.MetaModels;
using Fanex.Athena.Report.BySite.Services.Interfaces;
using Fanex.Athena.Report.Common.Settings;
using Fanex.Athena.Report.Core.Attributes;
using Fanex.Athena.Report.Core.Constants;
using Fanex.Athena.Report.Core.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Fanex.Athena.Report.BySite.Controllers
{
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
