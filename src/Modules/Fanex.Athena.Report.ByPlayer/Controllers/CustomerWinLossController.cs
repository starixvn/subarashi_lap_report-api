namespace Fanex.Athena.Report.ByPlayer.Controllers
{
    using System.IO;
    using System.Threading.Tasks;
    using Core.Attributes;
    using Core.Constants;
    using Core.ModelBinding;
    using Core.Services.Interfaces;
    using Excels.Interfaces;
    using MetaModels;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.FileProviders;
    using Service.Interfaces;

    [ApiController]
    [Route("api/[controller]/[action]")]
    [BearerAuthorize(Roles.PortalSuperAdmin, Roles.CustomerWinLoss, Roles.AccountInfo)]
    [RequestValidation(validateCurrency: true)]
    [AutoBindingMetaModel(bindAgent: true, bindPlayerCom: true, bindProductOptions: true)]
    public class CustomerWinLossController : ControllerBase
    {
        private readonly IContextService _contextService;
        private readonly ICustomerWinLossService _customerWinLossService;

        public CustomerWinLossController(
            ICustomerWinLossService customerWinLossService,
            IContextService contextService)
        {
            _customerWinLossService = customerWinLossService;
            _contextService = contextService;
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