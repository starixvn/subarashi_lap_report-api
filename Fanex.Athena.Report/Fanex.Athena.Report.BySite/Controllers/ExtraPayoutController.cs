namespace Fanex.Athena.Report.BySite.Controllers
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using Fanex.Athena.Report.BySite.DTOs;
    using Fanex.Athena.Report.BySite.Excels.Interfaces;
    using Fanex.Athena.Report.BySite.MetaModels;
    using Fanex.Athena.Report.BySite.Models;
    using Fanex.Athena.Report.BySite.Services.Interfaces;

    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.FileProviders;

    [ApiController]
    [Route("api/[controller]/[action]")]
    public class ExtraPayoutController : ControllerBase
    {
        [HttpPost]
        public async Task<ExtraPayoutResultModel> GetData(ExtraPayoutMetaModel metaModel, [FromServices] IExtraPayoutService service)
        {
            var result = await service.GetDataAsync(metaModel).ConfigureAwait(false);

            if (!result.DataList.Any())
            {
                return GetEmptyResult();
            }

            return new ExtraPayoutResultModel
            {
                Data = result.DataList.ToList(),
                TotalCurrency = result.TotalDataList.ToList(),
                TotalSummary = result.TotalData,
                TotalRows = result.TotalRows
            };
        }

        [HttpPost]
        public async Task<ExtraPayoutDetailResultModel> GetDetailData(ExtraPayoutDetailMetaModel metaModel, [FromServices] IExtraPayoutService service)
        {
            var result = await service.GetDetailDataAsync(metaModel).ConfigureAwait(false);

            if (!result.DataList.Any())
            {
                return new ExtraPayoutDetailResultModel
                {
                    Data = new List<ExtraPayoutDetail>(),
                    TotalSummary = new ExtraPayoutDetail()
                };
            }

            return new ExtraPayoutDetailResultModel
            {
                Data = result.DataList.ToList(),
                TotalSummary = result.TotalData
            };
        }

        private static dynamic GetEmptyResult()
        {
            return new ExtraPayoutResultModel
            {
                Data = new List<ExtraPayout>(),
                TotalCurrency = new List<ExtraPayout>(),
                TotalSummary = new ExtraPayout()
            };
        }

        [HttpPost]
        public async Task<FileContentResult> ExportExcel(
            [FromServices] IExtraPayoutService service,
            [FromServices] IFileProvider file,
            [FromServices] IExtraPayoutExcel excel,
            ExtraPayoutMetaModel metaModel)
        {
            var template = file.GetFileInfo("/ExcelTemplate/ExtraPayout.xls").PhysicalPath;

            var result = await service.GetDataAsync(metaModel).ConfigureAwait(false);

            var fileContent = excel.Export(template, result, metaModel);

            return new FileContentResult(fileContent, "application/octet-stream");
        }

        [HttpPost]
        public async Task<FileContentResult> ExportDetailExcel(
            [FromServices] IExtraPayoutService service,
            [FromServices] IFileProvider file,
            [FromServices] IExtraPayoutDetailExcel excel,
            ExtraPayoutDetailMetaModel metaModel)
        {
            var template = file.GetFileInfo("/ExcelTemplate/ExtraPayoutDetail.xls").PhysicalPath;

            var result = await service.GetDetailDataAsync(metaModel).ConfigureAwait(false);

            var fileContent = excel.Export(template, result, metaModel);

            return new FileContentResult(fileContent, "application/octet-stream");
        }
    }
}