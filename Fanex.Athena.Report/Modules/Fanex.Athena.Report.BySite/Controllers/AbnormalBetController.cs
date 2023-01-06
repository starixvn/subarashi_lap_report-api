namespace Fanex.Athena.Report.BySite.Controllers
{
    using System.Threading.Tasks;
    using Fanex.Athena.Report.BySite.Excels.Interfaces;
    using Fanex.Athena.Report.BySite.MetaModels;
    using Fanex.Athena.Report.BySite.Services.Interfaces;
    using Fanex.Athena.Report.Core.Attributes;
    using Fanex.Athena.Report.Core.Constants;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.FileProviders;

    [ApiController]
    [Route("api/[controller]/[action]")]
    [BearerAuthorize(Roles.PortalSuperAdmin, Roles.AbnormalBet)]
    [RequestValidation(validateCurrencyList: true)]
    [AutoBindingMetaModel(bindAgent: true)]
    public class AbnormalBetController : ControllerBase
    {
        private readonly IAbnormalBetService _service;

        public AbnormalBetController(IAbnormalBetService service)
            => _service = service;

        [HttpPost]
        public async Task<JsonResult> GetData([FromBody] AbnormalBetMetaModel metaModel)
        {
            var result = await _service.GetDataAsync(metaModel);

            return new JsonResult(new { data = result.DataList });
        }

        [HttpPost]
        public async Task<FileContentResult> ExportSummaryExcel(
            [FromServices] IFileProvider file,
            [FromServices] IAbnormalBetSummaryExcel excel,
            [FromBody] AbnormalBetMetaModel metaModel)
        {
            var template = file.GetFileInfo("/ExcelTemplate/AbnormalBetSummary.xls").PhysicalPath;

            var result = await _service.GetDataAsync(metaModel).ConfigureAwait(false);

            var fileContent = excel.Export(template, result, metaModel);

            return new FileContentResult(fileContent, "application/octet-stream");
        }

        [HttpPost]
        public async Task<FileContentResult> ExportIpSummaryExcel(
            [FromServices] IFileProvider file,
            [FromServices] IAbnormalBetIpSummaryExcel excel,
            [FromBody] AbnormalBetMetaModel metaModel)
        {
            var template = file.GetFileInfo("/ExcelTemplate/AbnormalBetIpSummary.xls").PhysicalPath;

            var result = await _service.GetDataAsync(metaModel).ConfigureAwait(false);

            var fileContent = excel.Export(template, result, metaModel);

            return new FileContentResult(fileContent, "application/octet-stream");
        }

        [HttpPost]
        public async Task<FileContentResult> ExportDetailExcel(
            [FromServices] IFileProvider file,
            [FromServices] IAbnormalBetDetailExcel excel,
            [FromBody] AbnormalBetMetaModel metaModel)
        {
            var template = file.GetFileInfo("/ExcelTemplate/AbnormalBetDetail.xls").PhysicalPath;

            var result = await _service.GetDataAsync(metaModel).ConfigureAwait(false);

            var fileContent = excel.Export(template, result, metaModel);

            return new FileContentResult(fileContent, "application/octet-stream");
        }
    }
}