using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Fanex.Athena.Report.ByPlayer.DTO;
using Fanex.Athena.Report.ByPlayer.Excels.Interfaces;
using Fanex.Athena.Report.ByPlayer.MetaModels;
using Fanex.Athena.Report.ByPlayer.Service.Interfaces;
using Fanex.Athena.Report.Core.Attributes;
using Fanex.Athena.Report.Core.Constants;
using Fanex.Athena.Report.Core.Services.Interfaces;
using Fanex.Athena.Report.Resources.Constants;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace Fanex.Athena.Report.ByPlayer.Controllers
{
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
                isEnableBA = contextService.IsEnableBA,
                productOptions = productAccess.SelectProductOptions(_ => _.ProductCode != Product.BA),
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