namespace Fanex.Athena.Report.BySite.Controllers
{
    using System.IO;
    using System.Threading.Tasks;
    using Fanex.Athena.Report.BySite.Excels.Interfaces;
    using Fanex.Athena.Report.BySite.MetaModels;
    using Fanex.Athena.Report.BySite.Services.Interfaces;
    using Microsoft.AspNetCore.Mvc;

    [ApiController]
    [Route("api/[controller]/[action]")]
    public class DailyBetsController : ControllerBase
    {
        [HttpPost]
        public async Task<dynamic> Index(DailyBetsMetaModel metaModel, [FromServices]IDailyBetsService service)
        {
            var result = await service.GetDataAsync(metaModel).ConfigureAwait(false);

            return new
            {
                result.DataList,
                Total = result.TotalData,
                result.TotalRows
            };
        }

        [HttpPost]
        public async Task<ActionResult> Export(DailyBetsMetaModel metaModel, [FromServices]IDailyBetsExcel excel)
        {
            var fileName = await excel.Export(metaModel).ConfigureAwait(false);

            if (fileName is null)
            {
                return NoContent();
            }
            var stream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.None, 4096, FileOptions.DeleteOnClose);
            return File(stream, "application/octet-stream");
        }
    }
}