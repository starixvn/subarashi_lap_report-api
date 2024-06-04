using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fanex.Athena.Report.BySite.DTOs;
using Fanex.Athena.Report.BySite.Excels.Interfaces;
using Fanex.Athena.Report.BySite.MetaModels;
using Fanex.Athena.Report.BySite.Models;
using Fanex.Athena.Report.BySite.Services.Interfaces;
using Fanex.Athena.Report.Core.Attributes;
using Fanex.Athena.Report.Core.Constants;
using Fanex.Athena.Report.Core.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.FileProviders;

namespace Fanex.Athena.Report.BySite.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    [BearerAuthorize(Roles.PortalSuperAdmin, Roles.ExtraPayout)]
    [RequestValidation(validateCurrencyList: true)]
    [AutoBindingMetaModel(bindAgent: true)]
    public class ExtraPayoutController : ControllerBase
    {
        private readonly IContextService _contextService;

        public ExtraPayoutController(IContextService contextService)
        {
            _contextService = contextService;
        }

        [HttpPost]
        public async Task<IActionResult> GetData([FromBody] ExtraPayoutMetaModel metaModel, [FromServices] IExtraPayoutService service)
        {
            AssignCommonMetaModel(metaModel);
            var result = await service.GetDataAsync(metaModel);

            if (!result.DataList.Any())
            {
                return Ok(GetEmptyResult());
            }

            return Ok(new ExtraPayoutResultModel
            {
                Data = result.DataList.ToList(),
                TotalCurrency = result.TotalDataList.ToList(),
                TotalSummary = result.TotalData,
                TotalRows = result.TotalRows
            });
        }

        [HttpPost]
        public async Task<IActionResult> GetDetailData([FromBody] ExtraPayoutDetailMetaModel metaModel, [FromServices] IExtraPayoutService service)
        {
            AssignCommonMetaModel(metaModel);
            var result = await service.GetDetailDataAsync(metaModel).ConfigureAwait(false);

            if (!result.DataList.Any())
            {
                return Ok(new ExtraPayoutDetailResultModel
                {
                    Data = new List<ExtraPayoutDetail>(),
                    TotalSummary = new ExtraPayoutDetail()
                });
            }

            return Ok(new ExtraPayoutDetailResultModel
            {
                Data = result.DataList.ToList(),
                TotalSummary = result.TotalData
            });
        }

        [HttpPost]
        public async Task<FileContentResult> ExportExcel(
            [FromServices] IExtraPayoutService service,
            [FromServices] IFileProvider file,
            [FromServices] IExtraPayoutExcel excel,
            [FromBody] ExtraPayoutMetaModel metaModel)
        {
            AssignCommonMetaModel(metaModel);
            metaModel.IsExportExcel = true;
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
            [FromBody] ExtraPayoutDetailMetaModel metaModel)
        {
            AssignCommonMetaModel(metaModel);
            metaModel.IsExportExcel = true;
            var template = file.GetFileInfo("/ExcelTemplate/ExtraPayoutDetail.xls").PhysicalPath;
            var result = await service.GetDetailDataAsync(metaModel).ConfigureAwait(false);
            var fileContent = excel.Export(template, result, metaModel);
            return new FileContentResult(fileContent, "application/octet-stream");
        }

        private static ExtraPayoutResultModel GetEmptyResult()
        {
            return new ExtraPayoutResultModel
            {
                Data = new List<ExtraPayout>(),
                TotalCurrency = new List<ExtraPayout>(),
                TotalSummary = new ExtraPayout()
            };
        }

        private void AssignCommonMetaModel(ExtraPayoutMetaModel metaModel)
        {
            metaModel.SpecialPlatform = _contextService.SpecialPlatform;
        }
    }
}