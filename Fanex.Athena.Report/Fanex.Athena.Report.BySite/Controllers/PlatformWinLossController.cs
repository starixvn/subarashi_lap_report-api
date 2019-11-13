namespace Fanex.Athena.Report.BySite.Controllers
{
    using System.Threading.Tasks;

    using Fanex.Athena.Report.BySite.DTOs;
    using Fanex.Athena.Report.BySite.Excels.Interfaces;
    using Fanex.Athena.Report.BySite.MetaModels;
    using Fanex.Athena.Report.BySite.Repositories.Interfaces;
    using Fanex.Athena.Report.Common.Model.ViewModel;

    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.FileProviders;

    [ApiController]
    [Route("api/[controller]/[action]")]
    public class PlatformWinLossController : ControllerBase
    {
        private readonly IPlatformWinLossRepository _repository;

        public PlatformWinLossController(IPlatformWinLossRepository repository)
        {
            _repository = repository;
        }

        [HttpPost]
        public async Task<ResultModel<PlatformWinLossSummary>> GetSummaryData(PlatformWinLossMetaModel metaModel)
        {
            return await _repository.GetSummaryDataAsync(metaModel).ConfigureAwait(false);
        }

        [HttpPost]
        public async Task<SortableResultModel<PlatformWinLossMember>> GetMemberData(PlatformWinLossMetaModel metaModel)
        {
            return await _repository.GetMemberDataAsync(metaModel).ConfigureAwait(false);
        }

        [HttpPost]
        public async Task<FileContentResult> ExportMemberData(
            [FromServices] IFileProvider file,
            [FromServices] IPlatformWinLossMemberExcel excel,
            PlatformWinLossMetaModel metaModel)
        {
            var template = file.GetFileInfo("/ExcelTemplate/PlatformWinLossMember.xls").PhysicalPath;

            var data = await _repository.GetMemberDataAsync(metaModel).ConfigureAwait(false);

            var fileContent = excel.Export(template, data, metaModel);

            return new FileContentResult(fileContent, "application/octet-stream");
        }

        [HttpPost]
        public async Task<FileContentResult> ExportSummaryData(
            [FromServices] IFileProvider file,
            [FromServices] IPlatformWinLossSummaryExcel excel,
            PlatformWinLossMetaModel metaModel)
        {
            var template = file.GetFileInfo("/ExcelTemplate/PlatformWinLossSummary.xls").PhysicalPath;

            var data = await _repository.GetSummaryDataAsync(metaModel).ConfigureAwait(false);

            var fileContent = excel.Export(template, data, metaModel);

            return new FileContentResult(fileContent, "application/octet-stream");
        }
    }
}