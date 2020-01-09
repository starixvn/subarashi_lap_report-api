using System.IO;
using System.Threading.Tasks;
using Fanex.Athena.Report.ByPlayer.Excels.Interfaces;
using Fanex.Athena.Report.ByPlayer.MetaModels;
using Fanex.Athena.Report.ByPlayer.Service.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Fanex.Athena.Report.ByPlayer.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class CustomerBalanceController : ControllerBase
    {
        [HttpPost]
        public async Task<dynamic> Index(CustomerBalanceMetaModel metaModel, [FromServices]ICustomerBalanceService service)
        {
            var result = await service.GetDataList(metaModel).ConfigureAwait(false);

            return new
            {
                Data = result.DataList,
                Total = result.TotalData,
                result.TotalRows
            };
        }

        [HttpPost]
        public async Task<ActionResult> ExportExcel(CustomerBalanceMetaModel metaModel, [FromServices]ICustomerBalanceExcel excel)
        {
            var fileName = await excel.Export(metaModel).ConfigureAwait(false);

            var stream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.None, 4096, FileOptions.DeleteOnClose);

            return File(stream, "application/octet-stream");
        }
    }
}