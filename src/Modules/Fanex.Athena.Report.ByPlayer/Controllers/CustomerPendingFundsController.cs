namespace Fanex.Athena.Report.ByPlayer.Controllers
{
    using System.Linq;
    using System.Threading.Tasks;
    using Core.Attributes;
    using Core.Constants;
    using Core.Services.Interfaces;
    using Excels.Interfaces;
    using Excels.Options;
    using MetaModels;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.FileProviders;
    using Models;
    using Service.Interfaces;

    [ApiController]
    [Route("api/[controller]/[action]")]
    [BearerAuthorize(Roles.PortalSuperAdmin, Roles.CustomerPendingFunds)]
    [RequestValidation(true)]
    [AutoBindingMetaModel(true, bindProductOptions: true)]
    public class CustomerPendingFundsController : ControllerBase
    {
        private const int PageSizeExcel = int.MaxValue - 1000;

        private readonly IContextService _contextService;

        private readonly IProductService _productService;

        public CustomerPendingFundsController(IContextService contextService, IProductService productService)
        {
            _contextService = contextService;
            _productService = productService;
        }

        [HttpGet]
        [SkipRequestValidation]
        public IActionResult Index([FromServices] IBaseCurrencyService baseCurrencyService)
        {
            var productAccess = _productService.GetSingleWalletProductAccess(_contextService.LicenseeId, FunctionNames.CustomerPendingFunds);
            return Ok(new
            {
                Currencies = _contextService.CurrencyList.ToArray(),
                BaseCurrencies = baseCurrencyService.GetBaseCurrencyList().ToArray(),
                BaseCurrency = _contextService.DefaultBaseCurrency,
                ProductOptions = productAccess.SelectProductOptions(),
                _contextService.IsFakeLicensee
            });
        }

        [HttpPost]
        public async Task<IActionResult> Data(
            [FromServices] ICustomerPendingFundsService customerPendingFundsService,
            [FromBody] CustomerPendingFundsMetaModel metaModel)
        {
            var data = await customerPendingFundsService.GetDataList(metaModel).ConfigureAwait(false);

            return Ok(new CustomerPendingFundsResultModel
            {
                Data = data.DataList,
                Total = data.TotalData,
                TotalRows = data.TotalRows
            });
        }

        [HttpPost]
        public async Task<ActionResult> Excel(
            [FromServices] IFileProvider file,
            [FromServices] ICustomerPendingFundsExcel excelService,
            [FromServices] ICustomerPendingFundsService customerPendingFundsService,
            [FromBody] CustomerPendingFundsMetaModel metaModel)
        {
            metaModel.PageIndex = 1;
            metaModel.PageSize = PageSizeExcel;

            var option = new CustomerPendingFundsExcelOptions
            {
                SystemIdVisible = _contextService.IsFakeLicensee,
                BaseCurrency = metaModel.BaseCurrency,
                CurrencyName = metaModel.CurrencyName,
                IsConvert = metaModel.IsConvert
            };

            var data = await customerPendingFundsService.GetDataList(metaModel).ConfigureAwait(false);

            var template = file.GetFileInfo("/ExcelTemplate/CustomerReports/CustomerPendingFunds.xls").PhysicalPath;
            var fileContent = excelService.Export(template, data, option);

            return new FileContentResult(fileContent, "application/octet-stream");
        }
    }
}