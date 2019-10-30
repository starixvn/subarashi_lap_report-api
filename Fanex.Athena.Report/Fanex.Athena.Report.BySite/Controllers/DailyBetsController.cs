namespace Fanex.Athena.Report.BySite.Controllers
{
    using System.IO;
    using System.Threading.Tasks;

    using Fanex.Athena.Report.BySite.MetaModels;
    using Fanex.Athena.Report.BySite.Services.Interfaces;
    using Microsoft.AspNetCore.Mvc;

    [ApiController]
    [Route("api/[controller]/[action]")]
    public class DailyBetsController : ControllerBase
    {
        private readonly IDailyBetsService _service;

        public DailyBetsController(IDailyBetsService dailyBetsService)
        {
            _service = dailyBetsService;
        }

        [HttpPost]
        public async Task<dynamic> Index(DailyBetsMetaModel metaModel)
        {
            var result = await _service.GetDataAsync(metaModel).ConfigureAwait(false);

            return new
            {
                result.DataList,
                Total = result.TotalData,
                result.TotalRows
            };
        }

        [HttpPost]
        public async Task<FileStreamResult> Export(DailyBetsMetaModel metaModel)
        {
            var fileName = await _service.Export(metaModel).ConfigureAwait(false);

            var stream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.None, 4096, FileOptions.DeleteOnClose);

            return new FileStreamResult(stream, "application/octet-stream");
        }
    }
}