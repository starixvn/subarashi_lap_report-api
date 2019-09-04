namespace Fanex.Athena.Report.BySite.Controllers
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;

    using Fanex.Athena.Report.BySite.DTOs;
    using Fanex.Athena.Report.BySite.Excels.Interfaces;
    using Fanex.Athena.Report.BySite.MetaModels;
    using Fanex.Athena.Report.BySite.Repositories.Interfaces;
    using Fanex.Athena.Report.Common.Extensions;
    using Fanex.Athena.Report.Common.Model.ViewModel;

    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.FileProviders;

    [ApiController]
    [Route("api/[controller]/[action]")]
    public class PlatformWinLossController : ControllerBase
    {
        private readonly IPlatformWinLossRepository _repository;

        private readonly IPlatformWinLossSummaryExcel _excel;
        private readonly IPlatformWinLossMemberExcel _platformWinLossMemberExcel;
        private readonly IFileProvider _file;

        public PlatformWinLossController(IPlatformWinLossRepository repository,
            IPlatformWinLossSummaryExcel excel, IFileProvider file,
            IPlatformWinLossMemberExcel platformWinLossMemberExcel)
        {
            _repository = repository;
            _excel = excel;
            _file = file;
            _platformWinLossMemberExcel = platformWinLossMemberExcel;
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

        [HttpGet]
        public async Task<FileContentResult> ExportMemberData(PlatformWinLossMetaModel metaModel)
        {
            var template = _file.GetFileInfo("/ExcelTemplate/PlatformWinLossMember.xls").PhysicalPath;

            var data = await _repository.GetMemberDataAsync(metaModel).ConfigureAwait(false);

            var workBook = _platformWinLossMemberExcel.Export(template, data, metaModel);
            var stream = new MemoryStream();
            workBook.Write(stream);

            return new FileContentResult(stream.ToArray(), "application/octet-stream");
        }
        [HttpPost]
        public async Task<FileContentResult> ExportSummaryData(PlatformWinLossMetaModel metaModel)
        {
            var template = _file.GetFileInfo("/ExcelTemplate/PlatformWinLossSummary.xls").PhysicalPath;

            var data = await _repository.GetSummaryDataAsync(metaModel).ConfigureAwait(false);

            var workBook = _excel.Export(template, data, metaModel);
            var stream = new MemoryStream();
            workBook.Write(stream);

            return new FileContentResult(stream.ToArray(), "application/octet-stream");
        }
    }
}