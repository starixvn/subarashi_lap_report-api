namespace Fanex.Athena.Report.BySite.Controllers
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Common.Model.DTO;
    using Core.Attributes;
    using Core.Constants;
    using Core.Services.Interfaces;
    using DTOs;
    using Excels.Interfaces;
    using MetaModels;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.FileProviders;
    using Models;
    using Resources;
    using Resources.Constants;
    using Services.Interfaces;

    [ApiController]
    [Route("api/[controller]/[action]")]
    [BearerAuthorize(Roles.PortalSuperAdmin, Roles.ExtraPayout)]
    [AutoBindingMetaModel(bindAgent: true)]
    [RequestValidation(validateCurrencyList: true)]
    public class ExtraPayoutController : ControllerBase
    {
        private readonly IContextService _contextService;

        private readonly IProductService _productService;

        public ExtraPayoutController(IContextService contextService, IProductService productService)
        {
            _contextService = contextService;
            _productService = productService;
        }

        [HttpPost]
        [SkipRequestValidation]
        public async Task<IActionResult> Index(
            [FromServices] IBaseCurrencyService baseCurrencyService,
            [FromServices] ISubsidiaryService subsidiaryService)
        {
            var subsidiaryList = await subsidiaryService.GetWhiteLabelTree(_contextService.SiteName);

            var productOptions = _productService.GetSingleWalletProductAccess(
                _contextService.LicenseeId,
                FunctionNames.ExtraPayout).SelectProductOptions();

            var hasPlayerTipsPermission = _contextService.IsInRoles(Roles.PortalSuperAdmin, Roles.DisplayPlayerTips_ExtraPayoutReportOnly);

            return Ok(new
            {
                currencyList = _contextService.CurrencyList.ToArray(),
                baseCurrencyList = baseCurrencyService.GetBaseCurrencyList().ToArray(),
                isAllowedCustomerCommission = _contextService.IsAllowedCustomerComm,
                viewType = !subsidiaryList.Any() ? SubsidiaryViewType.NotManagement : _contextService.SubsidiaryViewType,
                defaultBaseCurrency = _contextService.DefaultBaseCurrency,
                productOptions = hasPlayerTipsPermission ? AddPlayerTipsProduct(productOptions) : productOptions,
                whitelabelList = subsidiaryList,
                subsidiaryList = subsidiaryList.SelectMany(x => x.Children.Select(y => y.Value)).Distinct()
            });
        }

        [HttpGet]
        [SkipRequestValidation]
        public ActionResult GetPromotionTypes(string productCode, [FromServices] IExtraPayoutService service)
        {
            var result = service.BuildPromotionTypeList(productCode).Select(x => new { value = x.ID, label = x.Name, status = true });

            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> GetData([FromBody] ExtraPayoutMetaModel metaModel, [FromServices] IExtraPayoutService service)
        {
            AssignCommonMetaModel(metaModel);
            var result = await service.GetDataAsync(metaModel);

            if (!result.DataList.Any())
            {
                return Ok(GetEmptyResult());
            }

            return Ok(new ExtraPayoutResultModel
            {
                Data = result.DataList.ToList(),
                TotalCurrency = result.TotalDataList.ToList(),
                TotalSummary = result.TotalData,
                TotalRows = result.TotalRows
            });
        }

        [HttpPost]
        public async Task<IActionResult> GetDetailData([FromBody] ExtraPayoutDetailMetaModel metaModel, [FromServices] IExtraPayoutService service)
        {
            AssignCommonMetaModel(metaModel);
            var result = await service.GetDetailDataAsync(metaModel).ConfigureAwait(false);

            if (!result.DataList.Any())
            {
                return Ok(new ExtraPayoutDetailResultModel
                {
                    Data = new List<ExtraPayoutDetail>(),
                    TotalSummary = new ExtraPayoutDetail()
                });
            }

            return Ok(new ExtraPayoutDetailResultModel
            {
                Data = result.DataList.ToList(),
                TotalSummary = result.TotalData
            });
        }

        [HttpPost]
        public async Task<IActionResult> GetOddsBoostBonusDetailData([FromBody] ExtraPayoutMetaModel metaModel, [FromServices] IExtraPayoutService service)
        {
            AssignCommonMetaModel(metaModel);
            var result = await service.GetOddsBoostBonusDetailDataAsync(metaModel).ConfigureAwait(false);

            if (!result.DataList.Any())
            {
                return Ok(new OddsBoostBonusDetailResultModel
                {
                    Data = new List<OddsBoostBonusDetail>(),
                    TotalSummary = new OddsBoostBonusDetail()
                });
            }

            return Ok(new OddsBoostBonusDetailResultModel
            {
                Data = result.DataList.ToList(),
                TotalSummary = result.TotalData
            });
        }

        [HttpPost]
        public async Task<FileContentResult> ExportExcel(
            [FromServices] IExtraPayoutService service,
            [FromServices] IFileProvider file,
            [FromServices] IExtraPayoutExcel excel,
            [FromBody] ExtraPayoutMetaModel metaModel)
        {
            AssignCommonMetaModel(metaModel);
            metaModel.IsExportExcel = true;
            var template = file.GetFileInfo("/ExcelTemplate/ExtraPayout.xls").PhysicalPath;
            var result = await service.GetDataAsync(metaModel).ConfigureAwait(false);
            var fileContent = excel.Export(template, result, metaModel);
            return new FileContentResult(fileContent, "application/octet-stream");
        }

        [HttpPost]
        public async Task<FileContentResult> ExportDetailExcel(
            [FromServices] IExtraPayoutService service,
            [FromServices] IFileProvider file,
            [FromServices] IExtraPayoutDetailExcel excel,
            [FromBody] ExtraPayoutDetailMetaModel metaModel)
        {
            AssignCommonMetaModel(metaModel);
            metaModel.IsExportExcel = true;
            var template = file.GetFileInfo("/ExcelTemplate/ExtraPayoutDetail.xls").PhysicalPath;
            var result = await service.GetDetailDataAsync(metaModel).ConfigureAwait(false);
            var fileContent = excel.Export(template, result, metaModel);
            return new FileContentResult(fileContent, "application/octet-stream");
        }

        [HttpPost]
        public async Task<FileContentResult> ExportOddsBoostBonusDetailExcel(
            [FromServices] IExtraPayoutService service,
            [FromServices] IFileProvider file,
            [FromServices] IOddsBoostBonusDetailExcel excel,
            [FromBody] ExtraPayoutMetaModel metaModel)
        {
            AssignCommonMetaModel(metaModel);
            metaModel.IsExportExcel = true;
            var template = file.GetFileInfo("/ExcelTemplate/OddsBoostBonusDetail.xls").PhysicalPath;
            var result = await service.GetOddsBoostBonusDetailDataAsync(metaModel).ConfigureAwait(false);
            var fileContent = excel.Export(template, result, metaModel);
            return new FileContentResult(fileContent, "application/octet-stream");
        }

        private IList<ProductOption> AddPlayerTipsProduct(IList<ProductOption> productList)
        {
            var originalProductList = _productService.GetProductList(_contextService.LicenseeId);

            if (originalProductList.Any(_ => _.ProductCode == Product.SB))
            {
                productList.Add(new ProductOption { Code = Product.TIP, Name = ViewResources.ProductTIP, Status = true });
            }

            return productList;
        }

        private static ExtraPayoutResultModel GetEmptyResult()
        {
            return new ExtraPayoutResultModel
            {
                Data = new List<ExtraPayout>(),
                TotalCurrency = new List<ExtraPayout>(),
                TotalSummary = new ExtraPayout()
            };
        }

        private void AssignCommonMetaModel(ExtraPayoutMetaModel metaModel)
        {
            metaModel.SpecialPlatform = _contextService.SpecialPlatform;
        }
    }
}