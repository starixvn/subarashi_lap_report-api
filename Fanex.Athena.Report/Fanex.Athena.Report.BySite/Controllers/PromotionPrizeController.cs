namespace Fanex.Athena.Report.BySite.Controllers
{
    using System.IO;
    using System.Threading.Tasks;

    using Fanex.Athena.Report.BySite.Excels.Interfaces;
    using Fanex.Athena.Report.BySite.MetaModels;
    using Fanex.Athena.Report.BySite.Repositories.Interfaces;

    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.FileProviders;

    [ApiController]
    [Route("api/[controller]/[action]")]
    public class PromotionPrizeController : ControllerBase
    {
        private readonly IPromotionPrizeRepository _repository;

        public PromotionPrizeController(IPromotionPrizeRepository repository)
        {
            _repository = repository;
        }

        [HttpPost]
        public async Task<FileContentResult> Export(
            [FromServices] IFileProvider file,
            [FromServices] IPromotionPrizeExcel excel,
            PromotionPrizeMetaModel metaModel)
        {
            var template = file.GetFileInfo("/ExcelTemplate/PromotionPrize.xls").PhysicalPath;

            var data = await _repository.GetData(metaModel).ConfigureAwait(false);

            var workBook = excel.Export(template, data, metaModel);
            var stream = new MemoryStream();
            workBook.Write(stream);

            return new FileContentResult(stream.ToArray(), "application/octet-stream");
        }
    }
}