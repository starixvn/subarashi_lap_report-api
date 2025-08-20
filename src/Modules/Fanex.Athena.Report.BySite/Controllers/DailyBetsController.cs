namespace Fanex.Athena.Report.BySite.Controllers
{
    using System.IO;
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
    [BearerAuthorize(Roles.PortalSuperAdmin, Roles.DailyBets)]
    [RequestValidation(validateCurrencyList: true)]
    public class DailyBetsController : ControllerBase
    {
        private readonly IContextService _contextService;

        public DailyBetsController(IContextService contextService)
        {
            _contextService = contextService;
        }

        [HttpPost]
        public async Task<IActionResult> Index([FromBody] DailyBetsMetaModel metaModel, [FromServices] IDailyBetsService service)
        {
            AssignMetaModel(metaModel);
            var result = await service.GetDataAsync(metaModel, _contextService.AuditCurrencies);
            return Ok(new
            {
                result.DataList,
                Total = result.TotalData,
                result.TotalRows
            });
        }

        [HttpPost]
        public async Task<IActionResult> Export(
            [FromBody] DailyBetsMetaModel metaModel,
            [FromServices] IDailyBetsExcel excel,
            [FromServices] IFileProvider file)
        {
            AssignMetaModel(metaModel);
            var template = file.GetFileInfo("/ExcelTemplate/DailyBets.xls").PhysicalPath;
            var zipFilePath = await excel.Export(template, metaModel, _contextService.AuditCurrencies);

            var stream = new FileStream(zipFilePath, FileMode.Open, FileAccess.Read, FileShare.None, 4096, FileOptions.DeleteOnClose);
            return File(stream, "application/zip");
        }

        private void AssignMetaModel(DailyBetsMetaModel metaModel)
        {
            metaModel.CurrencyList = _contextService.CurrencyList;
            metaModel.SiteName = _contextService.SiteName;
            metaModel.LoginName = _contextService.UserName;
        }
    }
}