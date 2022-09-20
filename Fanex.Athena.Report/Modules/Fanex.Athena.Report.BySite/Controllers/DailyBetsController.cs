namespace Fanex.Athena.Report.BySite.Controllers
{
    using System.IO;
    using System.Threading.Tasks;
    using Fanex.Athena.Report.BySite.Excels.Interfaces;
    using Fanex.Athena.Report.BySite.MetaModels;
    using Fanex.Athena.Report.BySite.Services.Interfaces;
    using Fanex.Athena.Report.Core.Attributes;
    using Fanex.Athena.Report.Core.Constants;
    using Fanex.Athena.Report.Core.Services;
    using Microsoft.AspNetCore.Mvc;

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
            var result = await service.GetDataAsync(metaModel);
            return Ok(new
            {
                result.DataList,
                Total = result.TotalData,
                result.TotalRows
            });
        }

        [HttpPost]
        public async Task<IActionResult> Export([FromBody] DailyBetsMetaModel metaModel, [FromServices] IDailyBetsExcel excel)
        {
            AssignMetaModel(metaModel);
            var fileName = await excel.Export(metaModel);
            if (fileName is null)
            {
                return NoContent();
            }
            var stream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.None, 4096, FileOptions.DeleteOnClose);
            return File(stream, "application/octet-stream");
        }

        private void AssignMetaModel(DailyBetsMetaModel metaModel)
        {
            metaModel.CurrencyList = _contextService.CurrencyList;
            metaModel.SiteName = _contextService.SiteName;
        }
    }
}