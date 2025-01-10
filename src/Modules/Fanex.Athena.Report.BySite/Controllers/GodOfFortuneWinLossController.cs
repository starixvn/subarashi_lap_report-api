namespace Fanex.Athena.Report.BySite.Controllers
{
    using Fanex.Athena.Report.BySite.DTOs;
    using Fanex.Athena.Report.BySite.Excels.Interfaces;
    using Fanex.Athena.Report.BySite.MetaModels;
    using Fanex.Athena.Report.BySite.Models;
    using Fanex.Athena.Report.BySite.Services.Interfaces;
    using Fanex.Athena.Report.Core.Attributes;
    using Fanex.Athena.Report.Core.Constants;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.FileProviders;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    [ApiController]
    [Route("api/[controller]/[action]")]
    [BearerAuthorize(Roles.PortalSuperAdmin, Roles.GodOfFortuneWinLoss)]
    [RequestValidation(validateCurrencyList: true)]
    [AutoBindingMetaModel(bindAgent: true)]
    public class GodOfFortuneWinLossController : ControllerBase
    {
        [HttpPost]
        public async Task<IActionResult> Index([FromForm] GodOfFortuneWinLossMetaModel metaModel, [FromServices] IGodOfFortuneWinLossService service)
        {
            var result = await service.GetDataAsync(metaModel);

            if (!result.DataList.Any())
            {
                return Ok(GetEmptyResult());
            }

            var summaryTotalView = result.SummaryTotal.ToDictionary(d => d.Key, d => d.Value);

            return Ok(new GodOfFortuneWinLossResultModel
            {
                Data = result.DataList.ToList(),
                Total = result.TotalDataList.ToList(),
                SummaryTotal = summaryTotalView
            });
        }

        [HttpPost]
        public async Task<FileContentResult> ExportExcel(
            [FromServices] IGodOfFortuneWinLossService service,
            [FromServices] IFileProvider file,
            [FromServices] IGodOfFortuneWinLossExcel excel,
            [FromForm] GodOfFortuneWinLossMetaModel metaModel)
        {
            var template = file.GetFileInfo("/ExcelTemplate/GodOfFortuneWinLoss.xls").PhysicalPath;
            var result = await service.GetDataAsync(metaModel).ConfigureAwait(false);
            var fileContent = excel.Export(template, result, metaModel);
            return new FileContentResult(fileContent, "application/octet-stream");
        }

        private static GodOfFortuneWinLossResultModel GetEmptyResult()
        {
            return new GodOfFortuneWinLossResultModel
            {
                Data = new List<GodOfFortuneWinLoss>(),
                Total = new List<GodOfFortuneWinLoss>(),
                SummaryTotal = new Dictionary<string, GodOfFortuneWinLoss>(),
            };
        }
    }
}