using System.IO;
using System.Threading.Tasks;
using Fanex.Athena.Report.ByPlayer.Excels.Interfaces;
using Fanex.Athena.Report.ByPlayer.MetaModels;
using Fanex.Athena.Report.ByPlayer.Service.Interfaces;
using Fanex.Athena.Report.Core.Attributes;
using Fanex.Athena.Report.Core.Constants;
using Fanex.Athena.Report.Core.Services;
using Microsoft.AspNetCore.Mvc;

namespace Fanex.Athena.Report.ByPlayer.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    [BearerAuthorize(Roles.PortalSuperAdmin, Roles.CustomerBalance)]
    [RequestValidation(validateCurrency: true)]
    [AutoBindingMetaModel(bindAgent: true, bindProductOptions: true)]
    public class CustomerBalanceController : ControllerBase
    {
        private const int ExcelPageSize = int.MaxValue - 1000;

        [HttpPost]
        public async Task<IActionResult> Index([FromForm] CustomerBalanceMetaModel metaModel, [FromServices] ICustomerBalanceService service)
        {
            var result = await service.GetDataList(metaModel);
            return Ok(new
            {
                Data = result.DataList,
                Total = result.TotalData,
                result.TotalRows
            });
        }

        [HttpPost]
        public async Task<FileResult> ExportExcel([FromForm] CustomerBalanceMetaModel metaModel, 
            [FromServices] ICustomerBalanceExcel excel,
            [FromServices] IContextService contextService,
            [FromServices] ILicenseeService licenseeService)
        {
            metaModel.IsAllowedBA = contextService.IsEnableBA && (await licenseeService.CheckProductPermission(contextService.SiteName, ProductTypes.BA));
            metaModel.HasComm = contextService.IsAllowedCustomerComm;
            metaModel.IsEnableSingleWallet = contextService.IsEnableSingleWallet;
            metaModel.PageIndex = 1;
            metaModel.PageSize = ExcelPageSize;

            var fileName = await excel.Export(metaModel);
            var stream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.None, 4096, FileOptions.DeleteOnClose);
            return File(stream, "application/octet-stream");
        }
    }
}