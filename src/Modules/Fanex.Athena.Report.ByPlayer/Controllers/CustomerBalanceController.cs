namespace Fanex.Athena.Report.ByPlayer.Controllers
{
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using Core.Attributes;
    using Core.Constants;
    using Core.Services.Interfaces;
    using Excels.Interfaces;
    using MetaModels;
    using Microsoft.AspNetCore.Mvc;
    using Service.Interfaces;

    [ApiController]
    [Route("api/[controller]/[action]")]
    [BearerAuthorize(Roles.PortalSuperAdmin, Roles.CustomerBalance)]
    public class CustomerBalanceController : ControllerBase
    {
        private const int ExcelPageSize = int.MaxValue - 1000;

        [HttpPost]
        public IActionResult Index([FromServices] IContextService contextService, [FromServices] IProductService productService)
        {
            var productAccess = productService.GetSingleWalletProductAccess(contextService.LicenseeId, FunctionNames.CustomerBalance);
            return Ok(new
            {
                isEnableSingleWallet = contextService.IsEnableSingleWallet,
                isAllowedCustomerCommission = contextService.IsAllowedCustomerComm,
                productOptions = productAccess.SelectProductOptions(),
                currencies = contextService.CurrencyList.OrderBy(x => x.Value)
            });
        }

        [HttpPost]
        [RequestValidation(validateCurrency: true)]
        [AutoBindingMetaModel(bindAgent: true, bindProductOptions: true)]
        public async Task<IActionResult> GetData([FromBody] CustomerBalanceMetaModel metaModel, [FromServices] ICustomerBalanceService service)
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
        [RequestValidation(validateCurrency: true)]
        [AutoBindingMetaModel(bindAgent: true, bindProductOptions: true)]
        public async Task<FileResult> ExportExcel([FromBody] CustomerBalanceMetaModel metaModel,
            [FromServices] ICustomerBalanceExcel excel,
            [FromServices] IContextService contextService,
            [FromServices] ILicenseeService licenseeService)
        {
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