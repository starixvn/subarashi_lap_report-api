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
    using Fanex.Athena.Report.Core.Attributes;
    using Fanex.Athena.Report.Core.Constants;
    using Fanex.Athena.Report.Core.Services;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.FileProviders;

    [ApiController]
    [Route("api/[controller]/[action]")]
    [BearerAuthorize(Roles.PortalSuperAdmin, Roles.NewCashOutWinLoss)]
    [RequestValidation(validateCurrencyList: true)]
    [AutoBindingMetaModel(bindAgent: true)]
    public class NewCashOutWinLossController : ControllerBase
    {
        [HttpPost]
        public async Task<NewCashOutWinLossResultModel> GetData([FromBody] NewCashOutWinLossMetaModel metaModel, [FromServices] INewCashOutWinLossService service)
        {
            var result = await service.GetDataAsync(metaModel);

            if (!result.DataList.Any())
            {
                return GetEmptyResult();
            }

            return new NewCashOutWinLossResultModel
            {
                Data = result.DataList.ToList(),
                TotalCurrency = result.TotalDataList.ToList(),
                TotalSummary = result.TotalData,
                TotalRows = result.TotalRows
            };
        }

        [HttpPost]
        public async Task<FileContentResult> ExportExcel(
            [FromServices] INewCashOutWinLossService service,
            [FromServices] IFileProvider file,
            [FromServices] INewCashOutWinLossExcel excel,
            [FromServices] IContextService contextService,
            [FromServices] ILicenseeService licenseeService,
            [FromBody] NewCashOutWinLossMetaModel metaModel)
        {
            metaModel.IsAllowedBA = contextService.IsEnableBA && (await licenseeService.CheckProductPermission(contextService.SiteName, ProductTypes.BA));
            var template = file.GetFileInfo("/ExcelTemplate/NewCashOutWinLoss.xls").PhysicalPath;
            var result = await service.GetDataAsync(metaModel);
            var fileContent = excel.Export(template, result, metaModel);
            return new FileContentResult(fileContent, "application/octet-stream");
        }

        private static dynamic GetEmptyResult()
        {
            return new NewCashOutWinLossResultModel
            {
                Data = new List<CashOutWinLoss>(),
                TotalCurrency = new List<CashOutWinLoss>(),
                TotalSummary = new CashOutWinLoss()
            };
        }
    }
}