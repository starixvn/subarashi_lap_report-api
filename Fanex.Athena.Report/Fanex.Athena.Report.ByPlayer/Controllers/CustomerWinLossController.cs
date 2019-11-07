using System.Threading.Tasks;
using Fanex.Athena.Report.ByPlayer.DTO;
using Fanex.Athena.Report.ByPlayer.Excels.Interfaces;
using Fanex.Athena.Report.ByPlayer.MetaModels;
using Fanex.Athena.Report.ByPlayer.Service.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;

namespace Fanex.Athena.Report.ByPlayer.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class CustomerWinLossController : ControllerBase
    {
        public const string FormatDate = "MM/dd/yyyy";

        private readonly ICustomerWinLossService _customerWinLossService;

        private readonly ExcelFileOptions _excelFileOptions;

        public CustomerWinLossController(ICustomerWinLossService customerWinLossService, IOptions<ExcelFileOptions> excelFileOptions)
        {
            _customerWinLossService = customerWinLossService;
            _excelFileOptions = excelFileOptions.Value;
        }

        [HttpPost]
        public async Task<dynamic> Index(CustomerWinLossMetaModel metaModel)
        {
            var result = await _customerWinLossService.GetDataList(metaModel).ConfigureAwait(false);

            return new
            {
                result.DataList,
                Total = result.TotalData,
                result.TotalRows,
                PromotionPrize = result.AdditionalInfo
            };
        }

        [HttpPost]
        public async Task<FileResult> ExportExcel(
            [FromServices] IFileProvider file,
            [FromServices] ICustomerWinLossExcel excel,
            CustomerWinLossMetaModel metaModel)
        {
            var template = file.GetFileInfo("/ExcelTemplate/CustomerReports/CustomerWinLoss.xlsx").PhysicalPath;
            var data = await _customerWinLossService.GetDataList(metaModel).ConfigureAwait(false);

            var fileContent = excel.Export(template, data, metaModel);

            return File(fileContent, "application/octet-stream");
        }

        [HttpPost]
        public async Task<FileContentResult> ExportFile(
            [FromServices] IFileProvider file,
            [FromServices] ICustomerWinLossExcel excel,
            CustomerWinLossMetaModel metaModel)
        {
            var template = file.GetFileInfo("/ExcelTemplate/CustomerReports/CustomerWinLoss.xlsx").PhysicalPath;
            var data = await _customerWinLossService.GetGroupResult(metaModel, _excelFileOptions.MaxExcelFileRow).ConfigureAwait(false);

            var fileContent = data.Count == 1 ? excel.Export(template, data[0], metaModel) :
                excel.ExportFile(template, data, metaModel);

            var contentType = data.Count == 1 ? "application/octet-stream" : "application/zip";

            return new FileContentResult(fileContent, contentType);
        }
    }
}