namespace Fanex.Athena.Report.BySite.Controllers
{
    using System.Linq;
    using System.Threading.Tasks;
    using Common.Settings;
    using Core.Attributes;
    using Core.Constants;
    using Core.Models.DTOs;
    using Core.Services.Interfaces;
    using Excels.Interfaces;
    using Excels.Options;
    using MetaModels;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.FileProviders;
    using Models;
    using Resources;
    using Resources.Constants;
    using Services.Interfaces;

    [ApiController]
    [Route("api/[controller]/[action]")]
    [BearerAuthorize(Roles.PortalSuperAdmin, Roles.Turnover)]
    [RequestValidation(true)]
    [AutoBindingMetaModel(true, bindProductOptions: true)]
    public class TurnoverController : ControllerBase
    {
        private readonly AppSettings _appSettings;

        private readonly IContextService _contextService;

        private readonly IProductService _productService;

        public TurnoverController(
            IContextService contextService,
            AppSettings appSettings,
            IProductService productService)
        {
            _contextService = contextService;
            _appSettings = appSettings;
            _productService = productService;
        }

        [HttpGet]
        [SkipRequestValidation]
        public ActionResult Index([FromServices] IBaseCurrencyService baseCurrencyService)
        {
            var productAccess = GetProductAccess();

            return Ok(new TurnoverViewModel
            {
                Currencies = _contextService.CurrencyList.ToArray(),
                BaseCurrency = _contextService.DefaultBaseCurrency,
                BaseCurrencies = baseCurrencyService.GetBaseCurrencyList().ToArray(),
                ProductOptions = productAccess.SelectProductOptions(product => !product.ProductCode.Equals(Product.BA)),
                IsAllowedCustomerCommission = _contextService.IsAllowedCustomerComm,
                IsAllowedCasinoTransfer = _appSettings.LicenseeAllowCasinoTransfer.Contains(_contextService.LicenseeId),
                IsEnableSingleWallet = _contextService.IsEnableSingleWallet,
                IsFakeLicensee = _contextService.IsFakeLicensee,
                IsHidePlayerWl = !_contextService.IsSuperAdmin &&
                                 _contextService.IsInRoles(Roles.HidePlayerWL_WL_Comm_Total)
            });
        }

        [HttpPost]
        public async Task<ActionResult> GetData(
            [FromServices] ITurnoverService turnoverService,
            [FromServices] IDataPermissionService dataPermissionService,
            [FromBody] TurnoverMetaModel metaModel)
        {
            var placeholderUsername = _contextService.IsFakeLicensee ? ViewResources.ExternalId : ViewResources.ExternalIdOrSystemId;

            metaModel.Username =
                string.IsNullOrWhiteSpace(metaModel.Username) || metaModel.Username.Trim() == placeholderUsername ? null : metaModel.Username;
            var dataList = await turnoverService.GetDataList(metaModel);
            dataList.ToList().ForEach(_ => dataPermissionService.ClearDataByLimitPermission(_));

            return Ok(new TurnoverResultModel
            {
                TotalData = turnoverService.SumTotalData(dataList),
                Data = dataList
            });
        }

        [HttpPost]
        public async Task<ActionResult> ExportExcel(
            [FromServices] IFileProvider file,
            [FromServices] ITurnoverExcel excel,
            [FromServices] ITurnoverService turnoverService,
            [FromBody] TurnoverMetaModel metaModel)
        {
            var placeholderUsername = _contextService.IsFakeLicensee ? ViewResources.ExternalId : ViewResources.ExternalIdOrSystemId;
            metaModel.Username =
                string.IsNullOrWhiteSpace(metaModel.Username) || metaModel.Username.Trim() == placeholderUsername ? null : metaModel.Username;

            var excelOption = new TurnoverExcelOption
            {
                CustomerCommVisible = _contextService.IsAllowedCustomerComm,
                IsShowCasinoTransfer = _appSettings.LicenseeAllowCasinoTransfer.Contains(_contextService.LicenseeId),
                IsEnableSingleWallet = _contextService.IsEnableSingleWallet,
                IsHidePlayerWL = !_contextService.IsSuperAdmin &&
                                 _contextService.IsInRoles(Roles.HidePlayerWL_WL_Comm_Total)
            };

            var template = file.GetFileInfo("/ExcelTemplate/Turnover.xls").PhysicalPath;
            var data = await turnoverService.GetDataList(metaModel);
            var excelData = new TurnoverResultModel
            {
                TotalData = turnoverService.SumTotalData(data),
                Data = data
            };
            AssignBaseCurrency(metaModel);
            var fileContent = excel.Export(template, excelData, metaModel, excelOption);

            return new FileContentResult(fileContent, "application/octet-stream");
        }

        private void AssignBaseCurrency(TurnoverMetaModel metaModel)
        {
            metaModel.BaseCurrency = string.IsNullOrWhiteSpace(metaModel.BaseCurrency)
                ? metaModel.CurrencyName
                : metaModel.BaseCurrency;
        }

        private ProductAccess GetProductAccess() =>
            _productService.GetSingleWalletProductAccess(
                _contextService.LicenseeId,
                FunctionNames.Turnover);
    }
}