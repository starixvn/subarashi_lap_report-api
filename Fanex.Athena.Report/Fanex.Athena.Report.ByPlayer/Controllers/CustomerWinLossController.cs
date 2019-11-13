using System.Threading.Tasks;
using Fanex.Athena.Report.ByPlayer.Excels.Interfaces;
using Fanex.Athena.Report.ByPlayer.MetaModels;
using Fanex.Athena.Report.ByPlayer.Service.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.FileProviders;

namespace Fanex.Athena.Report.ByPlayer.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class CustomerWinLossController : ControllerBase
    {
        private readonly ICustomerWinLossService _customerWinLossService;

        public CustomerWinLossController(ICustomerWinLossService customerWinLossService)
        {
            _customerWinLossService = customerWinLossService;
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

            var stream = excel.Export(template, data, metaModel);

            return File(stream, "application/octet-stream");
        }
    }
}