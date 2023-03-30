namespace Fanex.Athena.Report.BySite.Controllers
{
    using System.Threading.Tasks;
    using Fanex.Athena.Report.BySite.Params;
    using Fanex.Athena.Report.BySite.Services.Interfaces;
    using Fanex.Athena.Report.Core.Attributes;
    using Fanex.Athena.Report.Core.Constants;
    using Microsoft.AspNetCore.Mvc;

    [ApiController]
    [Route("api/[controller]/[action]")]
    [BearerAuthorize(Roles.PortalSuperAdmin, Roles.WinlossAnalysis)]
    [RequestValidation(validateCurrencyList: true)]
    [AutoBindingMetaModel(bindAgent: true)]
    public class WinlossAnalysisController : ControllerBase
    {
        private readonly IWinlossAnalysisService _service;

        public WinlossAnalysisController(IWinlossAnalysisService service)
        {
            _service = service;
        }

        [HttpPost]
        public async Task<IActionResult> GetData([FromBody] WinlossAnalysisMetaModel metaModel)
            => Ok(await _service.GetData(metaModel));

        //[HttpPost]
        //public async Task<FileContentResult> ExportExcel(
        //   [FromServices] IFileProvider file,
        //   [FromForm] WinlossAnalysisParam metaModel)
        //    => await ExportMemberData(file, memberExcel, metaModel);

        //private async Task<FileContentResult> ExportMemberData(
        //    IFileProvider file)
        //{
        //    var template = file.GetFileInfo("/ExcelTemplate/WinlossAnalysis.xls").PhysicalPath;
        //    var data = await _service.GetData(metaModel).ConfigureAwait(false);
        //    var fileContent = excel.Export(template, data, metaModel);
        //    return new FileContentResult(fileContent, "application/octet-stream");
        //}
    }
}