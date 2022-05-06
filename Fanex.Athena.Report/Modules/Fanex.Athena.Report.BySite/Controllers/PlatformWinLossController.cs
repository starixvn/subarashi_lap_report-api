namespace Fanex.Athena.Report.BySite.Controllers
{
    using System.Threading.Tasks;
    using Fanex.Athena.Report.BySite.Excels.Interfaces;
    using Fanex.Athena.Report.BySite.MetaModels;
    using Fanex.Athena.Report.BySite.Repositories.Interfaces;
    using Fanex.Athena.Report.Core.Attributes;
    using Fanex.Athena.Report.Core.Constants;
    using Fanex.Athena.Report.Resources.Constants;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.FileProviders;

    [ApiController]
    [Route("api/[controller]/[action]")]
    [BearerAuthorize(Roles.PortalSuperAdmin, Roles.PlatformWinLoss)]
    [RequestValidation(validateCurrency: true)]
    [AutoBindingMetaModel(bindAgent: true, bindPlayerCom: true, bindProductOptions: true)]
    public class PlatformWinLossController : ControllerBase
    {
        private readonly IPlatformWinLossRepository _repository;

        public PlatformWinLossController(IPlatformWinLossRepository repository)
        {
            _repository = repository;
        }

        [HttpPost]
        public async Task<IActionResult> GetData([FromForm] PlatformWinLossMetaModel metaModel)
            => metaModel.Level == (int)ReportLevel.Main ? Ok(await _repository.GetSummaryDataAsync(metaModel))
                : Ok(await _repository.GetMemberDataAsync(metaModel));

        [HttpPost]
        public async Task<FileContentResult> ExportExcel(
           [FromServices] IFileProvider file,
           [FromServices] IPlatformWinLossSummaryExcel summaryExcel,
           [FromServices] IPlatformWinLossMemberExcel memberExcel,
           [FromForm] PlatformWinLossMetaModel metaModel)
            => metaModel.Level == (int)ReportLevel.Main ? await ExportSummaryData(file, summaryExcel, metaModel)
                : await ExportMemberData(file, memberExcel, metaModel);

        private async Task<FileContentResult> ExportMemberData(
            IFileProvider file,
            IPlatformWinLossMemberExcel excel,
            PlatformWinLossMetaModel metaModel)
        {
            var template = file.GetFileInfo("/ExcelTemplate/PlatformWinLossMember.xls").PhysicalPath;
            var data = await _repository.GetMemberDataAsync(metaModel).ConfigureAwait(false);
            var fileContent = excel.Export(template, data, metaModel);
            return new FileContentResult(fileContent, "application/octet-stream");
        }

        private async Task<FileContentResult> ExportSummaryData(
            IFileProvider file,
            IPlatformWinLossSummaryExcel excel,
            PlatformWinLossMetaModel metaModel)
        {
            var template = file.GetFileInfo("/ExcelTemplate/PlatformWinLossSummary.xls").PhysicalPath;
            var data = await _repository.GetSummaryDataAsync(metaModel).ConfigureAwait(false);
            var fileContent = excel.Export(template, data, metaModel);
            return new FileContentResult(fileContent, "application/octet-stream");
        }
    }
}