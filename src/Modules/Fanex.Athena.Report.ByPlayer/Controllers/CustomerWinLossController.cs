using Fanex.Athena.Report.ByPlayer.Excels.Interfaces;
using Fanex.Athena.Report.ByPlayer.MetaModels;
using Fanex.Athena.Report.ByPlayer.Service.Interfaces;
using Fanex.Athena.Report.Common.Constants;
using Fanex.Athena.Report.Core.Attributes;
using Fanex.Athena.Report.Core.Constants;
using Fanex.Athena.Report.Core.ModelBinding;
using Fanex.Athena.Report.Core.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.FileProviders;
using System.IO;
using System.Threading.Tasks;

namespace Fanex.Athena.Report.ByPlayer.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    [BearerAuthorize(Roles.PortalSuperAdmin, Roles.CustomerWinLoss, Roles.AccountInfo)]
    [RequestValidation(validateCurrency: true)]
    [AutoBindingMetaModel(bindAgent: true, bindPlayerCom: true, bindProductOptions: true)]
    public class CustomerWinLossController : ControllerBase
    {
        private readonly ICustomerWinLossService _customerWinLossService;
        private readonly IContextService _contextService;
        private readonly ILicenseeService _licenseeService;

        public CustomerWinLossController(
            ICustomerWinLossService customerWinLossService,
            IContextService contextService,
            ILicenseeService licenseeService)
        {
            _customerWinLossService = customerWinLossService;
            _contextService = contextService;
            _licenseeService = licenseeService;
        }

        [HttpPost]
        public async Task<IActionResult> Index([FromBodyOrDefault] CustomerWinLossMetaModel metaModel)
        {
            PrepareMetaModel(metaModel);
            var result = await _customerWinLossService.GetDataList(metaModel, true);
            return Ok(new
            {
                result.DataList,
                Total = result.TotalData,
                result.TotalRows,
                PromotionPrize = result.AdditionalInfo
            });
        }

        [HttpPost]
        public async Task<FileResult> ExportExcel(
            [FromServices] IFileProvider file,
            [FromServices] ICustomerWinLossExcel excel,
            [FromBodyOrDefault] CustomerWinLossMetaModel metaModel)
        {
            var template = file.GetFileInfo("/ExcelTemplate/CustomerReports/CustomerWinLoss.xlsx").PhysicalPath;

            PrepareMetaModel(metaModel);
            var data = await _customerWinLossService.GetDataList(metaModel);
            var fileName = excel.Export(template, data, metaModel);
            var stream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.None, 4096, FileOptions.DeleteOnClose);
            return File(stream, "application/octet-stream");
        }

        private void PrepareMetaModel(CustomerWinLossMetaModel metaModel)
        {
            metaModel.SystemIdVisible = !_contextService.IsFakeLicensee;
        }
    }
}