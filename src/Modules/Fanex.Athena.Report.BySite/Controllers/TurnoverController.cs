using System;
using System.Linq;
using System.Threading.Tasks;
using Fanex.Athena.Report.BySite.Excels.Interfaces;
using Fanex.Athena.Report.BySite.Excels.Options;
using Fanex.Athena.Report.BySite.MetaModels;
using Fanex.Athena.Report.BySite.Models;
using Fanex.Athena.Report.BySite.Services.Interfaces;
using Fanex.Athena.Report.Common.Settings;
using Fanex.Athena.Report.Core.Attributes;
using Fanex.Athena.Report.Core.Constants;
using Fanex.Athena.Report.Core.Models.DTOs;
using Fanex.Athena.Report.Core.Services.Interfaces;
using Fanex.Athena.Report.Resources;
using Fanex.Athena.Report.Resources.Constants;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.FileProviders;

namespace Fanex.Athena.Report.BySite.Controllers
{
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

        public TurnoverController(IContextService contextService, AppSettings appSettings,
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
                IsAllowBa = productAccess.IsAllowBa,
                IsEnableSingleWallet = _contextService.IsEnableSingleWallet,
                IsFakeLicensee = _contextService.IsFakeLicensee,
                IsHidePlayerWl = !_contextService.IsSuperAdmin &&
                                 _contextService.IsInRoles(Roles.HidePlayerWL_WL_Comm_Total)
            });
        }

        [HttpPost]
        public async Task<ActionResult> GetData([FromServices] ITurnoverService turnoverService,
                                                [FromServices] IDataPermissionService dataPermissionService,
                                                [FromBody] TurnoverMetaModel metaModel)
        {
            var placeholderUsername = _contextService.IsFakeLicensee ?
                ViewResources.ExternalId :
                ViewResources.ExternalIdOrSystemId;

            metaModel.Username =
                string.IsNullOrWhiteSpace(metaModel.Username) || metaModel.Username.Trim() == placeholderUsername ?
                    null :
                    metaModel.Username;
            var dataList = await turnoverService.GetDataList(metaModel);
            dataList.ToList().ForEach(_ => dataPermissionService.ClearDataByLimitPermission(_));

            return Ok(new TurnoverResultModel
            {
                TotalData = turnoverService.SumTotalData(dataList),
                Data = dataList
            });
        }

        [HttpPost]
        public async Task<ActionResult> ExportExcel([FromServices] IFileProvider file,
                                                    [FromServices] ITurnoverExcel excel,
                                                    [FromServices] ITurnoverService turnoverService,
                                                    [FromBody] TurnoverMetaModel metaModel)
        {
            var placeholderUsername = _contextService.IsFakeLicensee ?
                ViewResources.ExternalId :
                ViewResources.ExternalIdOrSystemId;
            metaModel.Username =
                string.IsNullOrWhiteSpace(metaModel.Username) || metaModel.Username.Trim() == placeholderUsername ?
                    null :
                    metaModel.Username;

            var productAccess = GetProductAccess();
            var excelOption = new TurnoverExcelOption
            {
                CustomerCommVisible = _contextService.IsAllowedCustomerComm,
                BaVisible = productAccess.IsAllowBa,
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
