namespace Fanex.Athena.Report.BySite.Controllers
{
    using System.Linq;
    using System.Threading.Tasks;
    using Fanex.Athena.Report.BySite.Constants;
    using Fanex.Athena.Report.BySite.Excels.Interfaces;
    using Fanex.Athena.Report.BySite.Excels.Options;
    using Fanex.Athena.Report.BySite.MetaModels;
    using Fanex.Athena.Report.BySite.Services.Interfaces;
    using Fanex.Athena.Report.Core.Attributes;
    using Fanex.Athena.Report.Core.Constants;
    using Fanex.Athena.Report.Core.Services;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.FileProviders;

    [ApiController]
    [Route("api/[controller]/[action]")]
    [BearerAuthorize(Roles.PortalSuperAdmin, Roles.WinlossAnalysis)]
    [RequestValidation(validateCurrencyList: true)]
    [AutoBindingMetaModel(bindAgent: true )]
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

        [HttpPost]
        public async Task<FileContentResult> ExportExcel(
           [FromServices] IWinlossAnalysisExcel excelService,
           [FromServices] IFileProvider fileProvider,
           [FromServices] IContextService contextService,
           [FromBody] WinlossAnalysisMetaModel metaModel)
        {
            var template = fileProvider.GetFileInfo("/ExcelTemplate/WinlossAnalysis.xls").PhysicalPath;
            AssignParamToExportExcel(metaModel);
            var data = await _service.GetDataAsync(metaModel);
            var excelOption = GenOptionFromParam(contextService, metaModel);
            var fileContent = excelService.Export(template, data, excelOption);
            return new FileContentResult(fileContent, "application/octet-stream");
        }

        private WinlossAnalysisExcelOption GenOptionFromParam(IContextService contextService, WinlossAnalysisMetaModel metaModel)
        {
            var isHasBAProduct = contextService.IsEnableBA
                && (string.Equals(metaModel.ProductCodes, ReportConstant.ALL) || metaModel.ProductCodes.Split(',').Contains(Resources.Constants.Product.BA));
            var licenseeCurrencyIds = string.Join(',', contextService.CurrencyList.Keys);
            var currencyText = metaModel.CurrencyIdList == licenseeCurrencyIds ? ReportConstant.ALL : metaModel.CurrencyIdList;
            return new WinlossAnalysisExcelOption
            {
                BaseCurrency = metaModel.BaseCurrency,
                BreakdownMode = metaModel.BreakdownMode,
                FromDate = metaModel.FromDate,
                ToDate = metaModel.ToDate,
                GroupByMode = metaModel.GroupMode,
                IsHasBAProduct = isHasBAProduct,
                CurrencyList= currencyText,
                ProductCodes = metaModel.ProductCodes
            };
        }

        private void AssignParamToExportExcel(WinlossAnalysisMetaModel metaModel)
        {
            metaModel.PageSize = 0;
            metaModel.PageIndex = 0;
        }

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