namespace Fanex.Athena.Report.BySite.Controllers
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Core.Attributes;
    using Core.Constants;
    using Core.Helpers;
    using Core.Models.DTOs;
    using Core.Models.MetaModels;
    using Core.Repositories.Abstraction;
    using Core.Services.Interfaces;
    using Core.Utility;
    using DTOs;
    using Excels.Interfaces;
    using Excels.Options;
    using MetaModels;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.FileProviders;
    using Resources.Constants;
    using Services.Interfaces;
    using ServiceStack;

    [ApiController]
    [Route("api/[controller]/[action]")]
    [BearerAuthorize(Roles.PortalSuperAdmin, Roles.SabaVirtualSportsWinLoss)]
    [RequestValidation(true)]
    [AutoBindingMetaModel(bindUserId: true)]
    public class SabaVirtualSportsWinLossController : ControllerBase
    {
        private readonly IContextService _contextService;

        private readonly ILeagueRepository _leagueRepository;

        private readonly IPortalApiService _portalApiService;

        private readonly IProductService _productService;

        private readonly ISabaVirtualSportsWinLossService _sabaVirtualSportsWinLossService;

        private readonly ISportService _sportService;

        public SabaVirtualSportsWinLossController(
            ISportService sportService,
            IContextService contextService,
            IPortalApiService portalApiService,
            ILeagueRepository leagueRepository,
            IProductService productService,
            ISabaVirtualSportsWinLossService sabaVirtualSportsWinLossService)
        {
            _sportService = sportService;
            _contextService = contextService;
            _portalApiService = portalApiService;
            _leagueRepository = leagueRepository;
            _productService = productService;
            _sabaVirtualSportsWinLossService = sabaVirtualSportsWinLossService;
        }

        [HttpGet]
        [SkipRequestValidation]
        public async Task<IActionResult> Index([FromServices] IBaseCurrencyService baseCurrencyService)
        {
            var sports = await _sportService
                .GetSportsAndProducts(_contextService.LicenseeId, _contextService.Language, Product.SB)
                .ConfigureAwait(true);
            return Ok(new
            {
                CurrencyId = _contextService.DefaultBaseCurrencyId,
                BaseCurrency = _contextService.DefaultBaseCurrency,
                BaseCurrencies = baseCurrencyService.GetBaseCurrencyList().ToArray(),
                Currencies = _contextService.CurrencyList.ToArray(),
                IsAllowedCustomerCommission = _contextService.IsAllowedCustomerComm,
                Leagues = new Dictionary<int, string>(),
                Sports = _sabaVirtualSportsWinLossService.GetSportList()
                    .Where(x => sports.Any(_ => _.Id == x.SportId && _.ParentId >= 0))
                    .ToDictionary(x => x.Id, x => x.Name).ToArray()
            });
        }

        [HttpPost]
        public async Task<IActionResult> LeaguesBySport([FromBody] LeagueMetaModel metaModel)
        {
            var sportList = _sabaVirtualSportsWinLossService.GetSportList();

            var sabaSports = _sportService.GetSabaSports();
            var sabaSportIds = sportList.SelectMany(_ => _.SabaSportIds);

            if (metaModel.SportId != 0)
            {
                sabaSportIds = sportList.FirstOrDefault(_ => _.Id == metaModel.SportId).SabaSportIds;
                metaModel.SportId = sportList.FirstOrDefault(_ => _.Id == metaModel.SportId).SportId;
            }

            var leagueGroupIds = sabaSports.Where(_ => sabaSportIds.Contains(_.Id)).SelectMany(_ => _.LeagueGroupIds)
                .ToList();

            var specialLeagues = await _portalApiService.GetSpecialLeaguesAsync(_contextService.SiteName);
            var leagues = (await _leagueRepository.GetLeaguesBySport(metaModel, specialLeagues).ConfigureAwait(false))
                .Where(x => leagueGroupIds.Contains(x.LeagueGroupId)).ToList();

            return Ok(leagues);
        }

        [HttpPost]
        public async Task<IActionResult> Data([FromBody] SabaVirtualSportsWinLossMetaModel metaModel)
        {
            AssignContextMetaModel(metaModel);

            var result = await _sabaVirtualSportsWinLossService.GetDataWithDrillDownAsync(metaModel, _contextService.CurrencyList);

            if (!result.DataList.Any())
            {
                return Ok(GetEmptyResult(result.AdditionalInfo));
            }

            return Ok(new
            {
                Data = result.DataList.Select(MapToGridView).ToList(),
                Total = result.TotalDataList.Select(MapToGridView).ToList(),
                SummaryTotal = result.SummaryTotal.ToDictionary(d => d.Key, d => MapToGridView(d.Value))
            });
        }

        [HttpPost]
        public async Task<IActionResult> ExportExcel(
            [FromServices] IFileProvider file,
            [FromServices] ISabaVirtualSportsWinLossExcel excel,
            [FromServices] ISabaVirtualSportsWinLossService sabaVirtualSportsWinLossService,
            [FromBody] SabaVirtualSportsWinLossMetaModel metaModel)
        {
            AssignContextMetaModel(metaModel);

            var data = await sabaVirtualSportsWinLossService.GetDataWithDrillDownAsync(metaModel, _contextService.CurrencyList);

            if (data.DataList.Any())
            {
                sabaVirtualSportsWinLossService.SortData(data.DataList, metaModel.SortBy, metaModel.SortOrder);
            }

            var productAccess = _productService.GetProductAccess(_contextService.LicenseeId);
            var excelOption = new SabaVirtualSportsWinLossExcelOption
            {
                IsConvert = metaModel.IsConvert,
                BaseCurrency = metaModel.BaseCurrency,
                CurrencyName = metaModel.CurrencyName,
                CoVisible = productAccess.IsAllow(Product.CO),
                SbVisible = productAccess.IsAllowSb,
                BaVisible = productAccess.IsAllowBa,
                CustomerCommVisible = _contextService.IsAllowedCustomerComm
            };

            var template = file.GetFileInfo("/ExcelTemplate/SabaVirtualSportsWinLoss.xls").PhysicalPath;

            var fileContent = excel.Export(template, data, metaModel, excelOption);

            return new FileContentResult(fileContent, "application/octet-stream");
        }

        private static dynamic GetEmptyResult(string promotionPrize = "0") =>
            new
            {
                Data = new List<SabaVirtualSportsWinLoss>(),
                Total = new List<SabaVirtualSportsWinLoss>(),
                SummaryTotal = new Dictionary<string, SabaVirtualSportsWinLoss>(),
                PromotionPrize = promotionPrize
            };

        private static SabaVirtualSportsWinLoss MapToGridView(SabaVirtualSportsWinLossDetail item)
        {
            var sabaVirtualSportsWinLossGrid = new SabaVirtualSportsWinLoss
            {
                Index = item.Index,
                AmtDueToBA = item.AmtDueToBA,
                CashOutCommission = item.CashOutCommission,
                BACommission = item.BACommission,
                CurrencyId = item.CurrencyId,
                SystemId = item.SystemId,
                CurrencyName = item.CurrencyName,
                CustomerComm = item.CustomerComm,
                CustomerWinLoss = item.CustomerWinLoss,
                TotalTransaction = item.TotalTransaction,
                CustomerId = item.CustomerId,
                Date = item.Date,
                ExternalId = item.ExternalId
            };
            SetLicenseeWinLossProductData(sabaVirtualSportsWinLossGrid, item.LicenseeWL);
            SetTurnOverProductData(sabaVirtualSportsWinLossGrid, item.TurnOver);

            return sabaVirtualSportsWinLossGrid;
        }

        private static void SetLicenseeWinLossProductData(
            SabaVirtualSportsWinLoss licenseeWinLoss,
            IDictionary<string, decimal> licenseeWlProducts)
        {
            licenseeWinLoss.LicenseeWLSB = licenseeWlProducts.GetValueOrDefault(Product.SB, 0);
            licenseeWinLoss.LicenseeWLBA = licenseeWlProducts.GetValueOrDefault(Product.BA, 0);
            licenseeWinLoss.LicenseeWLCO = licenseeWlProducts.GetValueOrDefault(Product.CO, 0);
        }

        private static void SetTurnOverProductData(
            SabaVirtualSportsWinLoss licenseeWinLoss,
            IDictionary<string, decimal> turnOverProducts)
        {
            licenseeWinLoss.TurnOverSB = turnOverProducts.GetValueOrDefault(Product.SB, 0);
            licenseeWinLoss.TurnOverBA = turnOverProducts.GetValueOrDefault(Product.BA, 0);
        }

        private void AssignContextMetaModel(SabaVirtualSportsWinLossMetaModel metaModel)
        {
            metaModel.DefaultBaseCurrency = _contextService.DefaultBaseCurrency;
            metaModel.LicenseeId = _contextService.LicenseeId;
            metaModel.ProductOptions = GetProductAccess().SelectProductOptions();
            metaModel.AgentListXml =
                AuthParameterHelper.GenerateAgentListXml(_contextService.AgentList, metaModel.CurrencyId, metaModel.IsExcludedTestData);
        }

        private ProductAccess GetProductAccess() => _productService.GetSingleWalletProductAccess(
            _contextService.LicenseeId, FunctionNames.SabaVirtualSportsWinLoss);
    }
}