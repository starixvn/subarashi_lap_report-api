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
    public class CustomerBalanceController : ControllerBase
    {
        private readonly ICustomerBalanceService _customerBalanceService;

        public CustomerBalanceController(ICustomerBalanceService customerBalanceService)
        {
            _customerBalanceService = customerBalanceService;
        }

        [HttpPost]
        public async Task<dynamic> Index(CustomerBalanceMetaModel metaModel)
        {
            var result = await _customerBalanceService.GetDataList(metaModel).ConfigureAwait(false);

            return new
            {
                result.DataList,
                Total = result.TotalData,
                result.TotalRows
            };
        }

        [HttpPost]
        public async Task<FileResult> ExportExcel(
            [FromServices] IFileProvider file,
            [FromServices] ICustomerBalanceExcel excel,
            CustomerBalanceMetaModel metaModel)
        {
            var template = file.GetFileInfo("/ExcelTemplate/CustomerReports/CustomerBalance.xlsx").PhysicalPath;
            var data = await _customerBalanceService.GetDataList(metaModel).ConfigureAwait(false);

            var stream = excel.Export(template, data, metaModel);

            return File(stream, "application/octet-stream");
        }
    }
}
