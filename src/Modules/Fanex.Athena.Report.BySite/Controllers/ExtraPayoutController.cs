using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fanex.Athena.Report.BySite.DTOs;
using Fanex.Athena.Report.BySite.Excels.Interfaces;
using Fanex.Athena.Report.BySite.MetaModels;
using Fanex.Athena.Report.BySite.Models;
using Fanex.Athena.Report.BySite.Services.Interfaces;
using Fanex.Athena.Report.Common.Model.DTO;
using Fanex.Athena.Report.Core.Attributes;
using Fanex.Athena.Report.Core.Constants;
using Fanex.Athena.Report.Core.Services.Interfaces;
using Fanex.Athena.Report.Resources;
using Fanex.Athena.Report.Resources.Constants;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.FileProviders;

namespace Fanex.Athena.Report.BySite.Controllers
{
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
        public async Task<IActionResult> Index([FromServices] IBaseCurrencyService baseCurrencyService,
                                               [FromServices] ISubsidiaryService subsidiaryService)
        {
            var subsidiaryList = await subsidiaryService.GetWhiteLabelTree(_contextService.SiteName);

            var productOptions = _productService.GetSingleWalletProductAccess(
                _contextService.LicenseeId,
                _contextService.Language,
                FunctionNames.ExtraPayout).SelectProductOptions();

            var hasPlayerTipsPermission = _contextService.IsInRoles(Roles.DisplayPlayerTips_ExtraPayoutReportOnly);

            return Ok(new
            {
                currencyList = _contextService.CurrencyList.ToArray(),
                baseCurrencyList = baseCurrencyService.GetBaseCurrencyList().ToArray(),
                isAllowedCustomerCommission = _contextService.IsAllowedCustomerComm,
                viewType = _contextService.SubsidiaryViewType,
                defaultBaseCurrency = _contextService.DefaultBaseCurrency,
                productOptions = hasPlayerTipsPermission ? AddPlayerTipsProduct(productOptions) : productOptions,
                whitelabelList = subsidiaryList,
                subsidiaryList = subsidiaryList.SelectMany(x => x.Children.Select(y => y.Value)).Distinct(),
            });
        }

        private IList<ProductOption> AddPlayerTipsProduct(IList<ProductOption> productList)
        {
            var originalProductList = _productService.GetProductList(_contextService.LicenseeId, _contextService.Language);

            if (originalProductList.Any(_ => _.ProductCode == Product.SB))
            {
                productList.Add(new ProductOption { Code = Product.TIP, Name = ViewResources.ProductTIP, Status = true });
            }

            return productList;
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