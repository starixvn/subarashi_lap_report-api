
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fanex.Athena.Report.BySite.Excels.Interfaces;
using Fanex.Athena.Report.BySite.Excels.Options;
using Fanex.Athena.Report.BySite.MetaModels;
using Fanex.Athena.Report.BySite.Models;
using Fanex.Athena.Report.BySite.Services.Interfaces;
using Fanex.Athena.Report.Common.Services.Interfaces;
using Fanex.Athena.Report.Common.Settings;
using Fanex.Athena.Report.Core.Attributes;
using Fanex.Athena.Report.Core.Constants;
using Fanex.Athena.Report.Core.Helpers;
using Fanex.Athena.Report.Core.Models.DTOs;
using Fanex.Athena.Report.Core.Services.Interfaces;
using Fanex.Athena.Report.StatelessCore.Enum;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.FileProviders;

namespace Fanex.Athena.Report.BySite.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    [BearerAuthorize(Roles.PortalSuperAdmin, Roles.LiveReport)]
    [RequestValidation(validateCurrency: true)]
    [AutoBindingMetaModel(bindAgent: true)]
    public class LiveReportController : ControllerBase
    {
        private const string ProductCodeAll = "ALL";

        private readonly AppSettings _appSettings;

        private readonly IBalanceUpService _balanceUpService;

        private readonly IContextService _contextService;

        private readonly ILiveReportService _liveReportService;

        private readonly IProductService _productService;

        private readonly IBaseCurrencyService _baseCurrencyService;

        private bool IsOneworksMaster =>
            _contextService.UserLevel == UserLevelType.OneWorks
         && _contextService.SubsidiaryViewType == SubsidiaryViewType.NormalManagement;

        public LiveReportController(ILiveReportService liveReportService, IBalanceUpService balanceUpService,
                                    IContextService contextService, AppSettings appSettings,
                                    IProductService productService, IBaseCurrencyService baseCurrencyService)
        {
            _liveReportService = liveReportService;
            _balanceUpService = balanceUpService;
            _contextService = contextService;
            _appSettings = appSettings;
            _productService = productService;
            _baseCurrencyService = baseCurrencyService;
        }


        [HttpGet]
        [SkipRequestValidation]
        public async Task<ActionResult> Index([FromServices] ISubsidiaryService subsidiaryService)
        {
            var productAccess = GetProductAccess();
            var subsidiaryList = await subsidiaryService.GetWhiteLabelTree(_contextService.SiteName);

            return Ok(new LiveReportViewModel
            {
                BaseCurrency = _contextService.DefaultBaseCurrency,
                BaseCurrencies = _baseCurrencyService.GetBaseCurrencyList().ToArray(),
                Currencies = _contextService.CurrencyList.ToArray(),
                IsAllowedNumberGame = productAccess.IsAllowNg,
                BalanceUpDate = await GetBalanceUpDay(),
                IsAllowedCustomerCommission = _contextService.IsAllowedCustomerComm,
                IsAllowBa = productAccess.IsAllowBa,
                HasAmountDueToColumn = _appSettings.HasAmountDueToColumn,
                IsFakeLicensee = _contextService.IsFakeLicensee,
                IsOneworksMaster = IsOneworksMaster,
                WhitelabelList = subsidiaryList,
                SubsidiaryList = subsidiaryList.SelectMany(x => x.Children.Select(y => y.Value)).Distinct(),
                SubsidiaryViewType = IsOneworksMaster ? SubsidiaryViewType.MasterManagement : _contextService.SubsidiaryViewType,
                SubsidiarySearch = _contextService.SubsidiaryViewType == SubsidiaryViewType.NotManagement && subsidiaryList.Any() && _contextService.ApplicationMode == AppMode.Parasitic,
            });
        }


        [HttpPost]
        public async Task<ActionResult> Summary([FromBody] LiveReportMetaModel metaModel)
        {
            var productAccess = GetProductAccess();
            metaModel.OriginalProductCodeList = string.Join(",", productAccess.ProductList.Select(x => x.ProductCode));
            metaModel.ProductCodeList =
                _productService.GetAllowedSubProductCodeList(_contextService.OfferedSubProductList,
                    metaModel.OriginalProductCodeList);

            return Ok(await _liveReportService.GetLiveReportSummary(metaModel));
        }

        [HttpPost]
        [AutoBindingMetaModel(bindProductCodeList: true)]
        public async Task<ActionResult> Detail([FromBody] LiveReportMetaModel metaModel)
        {
            var productAccess = GetProductAccess();

            var resultModel = await GetDetailResult(metaModel, productAccess).ConfigureAwait(false);
            resultModel.IsAllowedCustomerCommission = _contextService.IsAllowedCustomerComm;

            var balanceUpDay = await GetBalanceUpDay();
            resultModel.IsShowedDetailLink = metaModel.FromDate >= balanceUpDay;

            return Ok(resultModel);
        }

        private async Task<LiveReportDetailResultModel> GetDetailResult(LiveReportMetaModel metaModel, ProductAccess productAccess)
        {
            IEnumerable<ProductInfo> productList;

            if (metaModel.ProductCode == ProductCodeAll)
            {
                productList = productAccess.ProductList.ToList();
                metaModel.OriginalProductCodeList = string.Join(",", productAccess.ProductList.Select(x => x.ProductCode));
            }
            else
            {
                productList = productAccess.ProductList.Where(i => i.ProductCode == metaModel.ProductCode).ToList();
                metaModel.OriginalProductCodeList = metaModel.ProductCode;
            }

            metaModel.ProductCodeList = _productService.GetAllowedSubProductCodeList(_contextService.OfferedSubProductList, metaModel.OriginalProductCodeList);
            return await _liveReportService.GetLiveReportSummaryDetail(metaModel,
                _contextService.CurrencyList.ToDictionary(x => x.Key, x => x.Value), productList).ConfigureAwait(false);
        }

        [HttpGet]
        public async Task<ActionResult> MatchList([FromQuery] LiveReportMatchListMetalModel metaModel)
        {
            var productAccess = GetProductAccess();

            return Ok(new LiveReportMatchListResultModel
            {
                Data = await _liveReportService.GetLiveReportWinLossByMatch(metaModel).ConfigureAwait(false),
                IsAllowedCustomerCommission = _contextService.IsAllowedCustomerComm,
                HasAmountDueToColumn = _appSettings.HasAmountDueToColumn,
                IsAllowBa = productAccess.IsAllowBa,
            });
        }

        [HttpGet]
        public async Task<ActionResult> MatchListSort([FromQuery] LiveReportMatchListMetalModel metaModel)
        {
            return Ok(await _liveReportService.GetLiveReportWinLossByMatch(metaModel).ConfigureAwait(false));
        }

        [HttpPost]
        public async Task<IActionResult> MatchListExport([FromServices] IFileProvider file,
                                                         [FromServices] ILiveReportMatchListExcel excel,
                                                         [FromServices] ICacheRefAdapter cacheRefAdapter,
                                                         [FromBody] LiveReportMatchListMetalModel metaModel)
        {
            var productAccess = GetProductAccess();

            var excelOption = new LiveReportMatchListExcelOption
            {
                BaVisible = productAccess.IsAllowBa,
                CustomerCommVisible = _contextService.IsAllowedCustomerComm,
                HasAmountDueToColumn = _appSettings.HasAmountDueToColumn,
                IsConvert = metaModel.IsConvert,
                BaseCurrency = metaModel.BaseCurrency,
                CurrencyName = metaModel.CurrencyName,
                SportName = cacheRefAdapter.GetSportName(metaModel.SportId)
            };

            var data = await _liveReportService.GetLiveReportWinLossByMatch(metaModel).ConfigureAwait(false);

            var template = file.GetFileInfo("/ExcelTemplate/LiveReportMatchList.xls").PhysicalPath;
            var fileContent = excel.Export(template, data, metaModel, excelOption);

            return new FileContentResult(fileContent, "application/octet-stream");
        }

        [HttpPost]
        public async Task<ActionResult> ExportExcel([FromServices] IFileProvider file,
                                                    [FromServices] ILiveReportExcel excel,
                                                    [FromBody] LiveReportMetaModel metaModel)
        {
            var productAccess = GetProductAccess();
            metaModel.ProductCodeList = string.Join(",", productAccess.ProductList.Select(x => x.ProductCode));

            var summaryReport = await _liveReportService.GetLiveReportSummary(metaModel);

            var excelOption = new LiveReportExcelOption
            {
                CustomerCommVisible = _contextService.IsAllowedCustomerComm,
                BaVisible = productAccess.IsAllowBa,
                HasAmountDueToColumn = _appSettings.HasAmountDueToColumn,
                IsConvert = metaModel.IsConvert,
                BaseCurrency = metaModel.BaseCurrency,
                CurrencyName = metaModel.CurrencyName
            };

            var resultModel = string.IsNullOrEmpty(metaModel.ProductCode) ? new LiveReportDetailResultModel() : await GetDetailResult(metaModel, productAccess).ConfigureAwait(false);

            var template = file.GetFileInfo("/ExcelTemplate/LiveReport.xls").PhysicalPath;
            var fileContent = excel.Export(template, summaryReport, resultModel, metaModel, excelOption);

            return new FileContentResult(fileContent, "application/octet-stream");
        }

        protected ProductAccess GetProductAccess() =>
            _productService.GetProductAccess(_contextService.LicenseeId, FunctionNames.LiveReport);

        private async Task<DateTime> GetBalanceUpDay()
        {
            var balanceUp = await _balanceUpService.GetBalanceUpAsync();
            return balanceUp?.BalanceUpDay ?? DateTimeHelper.Now;
        }
    }
}
