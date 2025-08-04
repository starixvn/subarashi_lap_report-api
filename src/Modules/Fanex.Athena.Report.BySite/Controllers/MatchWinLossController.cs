namespace Fanex.Athena.Report.BySite.Controllers
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Common.Excel;
    using Common.Helpers;
    using Common.Model.DataDomain;
    using Common.Services.Interfaces;
    using Core.Attributes;
    using Core.Constants;
    using Core.Services.Interfaces;
    using DTOs;
    using Excels.Interfaces;
    using Excels.Options;
    using Fanex.Common.Extensions;
    using MetaModels;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.FileProviders;
    using Models;
    using Resources.Constants;
    using Services.Interfaces;

    [ApiController]
    [Route("api/[controller]/[action]")]
    [BearerAuthorize(Roles.PortalSuperAdmin, Roles.MatchWinLoss)]
    [RequestValidation(true)]
    [AutoBindingMetaModel(true)]
    public class MatchWinLossController : Controller
    {
        private readonly IBaseCurrencyService _baseCurrencyService;

        private readonly ICacheRefAdapter _cacheRefAdapter;

        private readonly IContextService _contextService;

        private readonly IMatchWinLossService _matchWinLossService;

        private readonly IProductService _productService;

        public MatchWinLossController(
            ICacheRefAdapter cacheRefAdapter,
            IContextService contextService,
            IProductService productService,
            IBaseCurrencyService baseCurrencyService,
            IMatchWinLossService matchWinLossService)
        {
            _cacheRefAdapter = cacheRefAdapter;
            _contextService = contextService;
            _productService = productService;
            _baseCurrencyService = baseCurrencyService;
            _matchWinLossService = matchWinLossService;
        }

        [HttpGet]
        [SkipRequestValidation]
        public IActionResult Index()
        {
            var productAccess = _productService.GetSingleWalletProductAccess(
                _contextService.LicenseeId,
                FunctionNames.MatchWinLoss);

            return Ok(new
            {
                baseCurrency = _contextService.DefaultBaseCurrency,
                baseCurrencies = _baseCurrencyService.GetBaseCurrencyList().ToArray(),
                currencies = _contextService.CurrencyList.ToArray(),
                productOptions = productAccess.SelectProductOptions(),
                isAllowedCustomerCommission = _contextService.IsAllowedCustomerComm
            });
        }

        [HttpPost]
        public async Task<IActionResult> GetData([FromBody] MatchWinLossMetaModel metaModel)
        {
            var reportData = await _matchWinLossService.GetDataList(metaModel).ConfigureAwait(false);
            IList<MatchWinLoss> reportDataOutrightParlay = new List<MatchWinLoss>();

            var outrightParlayProducts = new List<string> { Product.BA, Product.SB, Product.NG, Product.VS };

            if (metaModel.ProductOptions.Any(i => i.Status && outrightParlayProducts.Exists(s => i.Code == s)))
            {
                reportDataOutrightParlay = await _matchWinLossService.GetOutrightParlayList(metaModel).ConfigureAwait(false);
                reportDataOutrightParlay.ForEach(report =>
                    report.BettypeName = _cacheRefAdapter.GetBettypeWithProductName(report.BetType, report.ProductCode));
            }

            var total = GetTotal(reportData);
            var grandTotal = GetGrandTotal(reportDataOutrightParlay, total);

            return Ok(new MatchWinLossResultModel
            {
                Data = reportData,
                LeagueList = CreateLeagueList(reportData, _contextService.IsHideSaba),
                SpecialBetTypeResult = reportDataOutrightParlay,
                ColumnCount = GetColumnCount(_contextService.IsAllowedCustomerComm),
                Total = total,
                GrandTotal = grandTotal
            });
        }

        [HttpPost]
        public async Task<IActionResult> GetDetailData([FromBody] MatchWinLossMetaModel metaModel)
        {
            if (!metaModel.IsValidDetailRequest)
            {
                return BadRequest();
            }

            var reportData = await _matchWinLossService.GetDetailDataList(metaModel).ConfigureAwait(false);

            return Ok(reportData);
        }

        [HttpPost]
        public async Task<IActionResult> ExportExcel(
            [FromServices] IFileProvider file,
            [FromServices] IMatchWinLossExcel excel,
            [FromBody] MatchWinLossMetaModel metaModel)
        {
            if (metaModel.CurrencyId != BaseCurrencyType.All &&
                !_contextService.CurrencyList.ContainsKey(metaModel.CurrencyId))
            {
                return new FileContentResult(ExcelHelper.ExportEmptyFile(), "application/octet-stream");
            }

            const int PageSize = 63000;
            metaModel.PageSize = PageSize;

            var option = new MatchWinLossExcelOption
            {
                CustomerCommVisible = _contextService.IsAllowedCustomerComm,
                IsHideSaba = _contextService.IsHideSaba,
                IsConvert = metaModel.IsConvert,
                BaseCurrency = metaModel.BaseCurrency,
                CurrencyName = metaModel.CurrencyName
            };

            var template = file.GetFileInfo("/ExcelTemplate/MatchWinLoss.xls").PhysicalPath;
            var data = await _matchWinLossService.GetDetailDataSet(metaModel).ConfigureAwait(false);
            var fileContent = excel.Export(template, data, metaModel, option);
            return new FileContentResult(fileContent, "application/octet-stream");
        }

        private static IDictionary<int, string> CreateLeagueList(IList<MatchWinLoss> reportData, bool isHideSaba)
        {
            if (reportData == null || reportData.Count == 0)
            {
                return new Dictionary<int, string>();
            }

            return reportData.GroupBy(i => i.LeagueId).ToDictionary(
                i => i.Key,
                i =>
                {
                    var leagueName = i.FirstOrDefault()?.LeagueName;
                    return isHideSaba ? SabaHelper.RemoveSabaText(leagueName) : leagueName;
                });
        }

        private static MatchWinLoss GetTotal(IList<MatchWinLoss> dataList)
        {
            var totalMatchWinLoss = new MatchWinLoss
            {
                Turnover = dataList.Sum(item => item.Turnover),
                TotalTransaction = dataList.Sum(item => item.TotalTransaction),
                CustomerWL = dataList.Sum(item => item.CustomerWL),
                CustomerComm = dataList.Sum(item => item.CustomerComm),
                LicenseeWL = dataList.Sum(item => item.LicenseeWL)
            };

            return totalMatchWinLoss;
        }

        private static MatchWinLoss GetGrandTotal(IList<MatchWinLoss> dataListOutright, MatchWinLoss total)
        {
            return new MatchWinLoss
            {
                Turnover = dataListOutright.Sum(item => item.Turnover) + total.Turnover,
                TotalTransaction = dataListOutright.Sum(item => item.TotalTransaction) + total.TotalTransaction,
                CustomerWL = dataListOutright.Sum(item => item.CustomerWL) + total.CustomerWL,
                CustomerComm = dataListOutright.Sum(item => item.CustomerComm) + total.CustomerComm,
                LicenseeWL = dataListOutright.Sum(item => item.LicenseeWL) + total.LicenseeWL
            };
        }

        private static int GetColumnCount(bool isAllowedCustomerComm)
        {
            const int AdditionColumns = 2;
            var columnCount = 6;

            if (isAllowedCustomerComm)
            {
                columnCount += AdditionColumns;
            }

            return columnCount;
        }
    }
}