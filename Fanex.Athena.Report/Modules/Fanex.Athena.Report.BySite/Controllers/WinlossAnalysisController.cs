namespace Fanex.Athena.Report.BySite.Controllers
{
    using System;
    using System.ComponentModel;
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
            => Ok(await _service.GetDataAsync(metaModel));

        //[HttpPost]
        //public async Task<FileContentResult> ExportExcel(
        //   [FromServices] IWinlossAnalysisExcel excelService,
        //   [FromServices] IFileProvider file,
        //   [FromForm] WinlossAnalysisMetaModel metaModel)
        //{
        //    var template = file.GetFileInfo("/ExcelTemplate/PlatformWinLossSummary.xls").PhysicalPath;
        //    AssignParamToExportExcel(metaModel);
        //    var data = await _service.GetRawDataAsync(metaModel);
        //    var fileContent = excelService.Export(data, metaModel);
        //    return new FileContentResult(fileContent, "application/octet-stream");
        //}

        //private void AssignParamToExportExcel(WinlossAnalysisMetaModel metaModel)
        //{
        //    metaModel.PageSize = 0;
        //    metaModel.PageIndex = 0;
        //}

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