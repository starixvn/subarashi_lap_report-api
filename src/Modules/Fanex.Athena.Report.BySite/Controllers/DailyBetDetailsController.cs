namespace Fanex.Athena.Report.BySite.Controllers
{
    using System.Threading.Tasks;
    using Core.Attributes;
    using Core.Constants;
    using Core.Services.Interfaces;
    using Excels.Interfaces;
    using MetaModels;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.FileProviders;
    using Services.Interfaces;

    [ApiController]
    [Route("api/[controller]/[action]")]
    [BearerAuthorize(Roles.PortalSuperAdmin, Roles.DailyBetDetails)]
    [RequestValidation(validateCurrencyList: true)]
    public class DailyBetDetailsController : ControllerBase
    {
        private readonly IContextService _contextService;

        public DailyBetDetailsController(IContextService contextService)
        {
            _contextService = contextService;
        }

        [HttpPost]
        public async Task<IActionResult> Index([FromBody] DailyBetDetailsMetaModel metaModel, [FromServices] IDailyBetDetailsService service)
        {
            AssignMetaModel(metaModel);
            var result = await service.GetDataAsync(metaModel, _contextService.AuditCurrencies);
            return Ok(new
            {
                result.DataList,
                result.TotalRows
            });
        }

        [HttpPost]
        public async Task<FileContentResult> Export(
           [FromServices] IDailyBetDetailsService service,
           [FromServices] IFileProvider file,
           [FromServices] IDailyBetDetailsExcel excel,
           [FromBody] DailyBetDetailsMetaModel metaModel)
        {
            var template = file.GetFileInfo("/ExcelTemplate/DailyBetDetails.xls").PhysicalPath;
            AssignMetaModel(metaModel);
            AssignExportMetaModel(metaModel);
            var result = await service.GetDataAsync(metaModel, _contextService.AuditCurrencies).ConfigureAwait(false);
            var fileContent = excel.Export(template, result, metaModel);
            return new FileContentResult(fileContent, "application/octet-stream");
        }

        private static void AssignExportMetaModel(DailyBetDetailsMetaModel metaModel)
        {
            metaModel.PageIndex = 0;
        }

        private void AssignMetaModel(DailyBetDetailsMetaModel metaModel)
        {
            metaModel.CurrencyList = _contextService.CurrencyList;
            metaModel.SiteName = _contextService.SiteName;
        }
    }
}