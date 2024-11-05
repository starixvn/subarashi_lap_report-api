using System.Threading.Tasks;
using Fanex.Athena.Report.BetList.MetaModels;
using Fanex.Athena.Report.BetList.Models;
using Fanex.Athena.Report.BetList.Models.BetList;
using Fanex.Athena.Report.Common.Services.Interfaces;
using Fanex.Athena.Report.Core.ApiClients.Abstraction;
using Fanex.Athena.Report.Core.Attributes;
using Fanex.Athena.Report.Core.Constants;
using Fanex.Athena.Report.Core.Models.DTOs;
using Fanex.Athena.Report.Core.Services.Interfaces;
using Fanex.Athena.Report.Resources;
using Fanex.Athena.Report.Resources.Constants;
using Microsoft.AspNetCore.Mvc;

namespace Fanex.Athena.Report.BetList.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    [BearerAuthorize(Roles.PortalSuperAdmin, Roles.LiveReport)]
    [RequestValidation(true)]
    [AutoBindingMetaModel(true)]
    public class LiveReportBetListController : BetListBaseController
    {
        protected const string ExcelFileName = "LiveReportBetList";

        private const string LiveReportSpKey = "Lrf_BetList_LiveReport";

        private const string MatchSpKey = "Lrf_BetList_LiveReport_SelWinlossByMatch";

        private const string OutRightSpKey = "Lrf_BetList_LiveReport_SelWinlossOutright";

        private const string NumberGameSpKey = "Lrf_BetList_LiveReport_SelWinlossNumberGame";

        private const string MixParlaySpKey = "Lrf_BetList_LiveReport_SelWinlossParlay";

        private const string VirtualSportSpKey = "Lrf_BetList_LiveReport_SelWinlossVHRacing";

        private static readonly int[] HiddenColumns = { BetEffectiveColumn, };

        private readonly ICacheRefAdapter _cacheRefAdapter;

        public LiveReportBetListController(IContextService contextService,
                                           ILicenseeService licenseeService,
                                           IBetListApiClient betListApiClient,
                                           IBalanceUpService balanceUpService,
                                           ICacheRefAdapter cacheRefAdapter) :
            base(contextService,
                licenseeService,
                betListApiClient,
                balanceUpService)
        {
            _cacheRefAdapter = cacheRefAdapter;
        }

        [HttpPost]
        public async Task<IActionResult> MatchExportExcel([FromBody] LiveReportBetListMetaModel metaModel)
        {
            metaModel.UspKey = MatchSpKey;

            return await ExportExcel(metaModel);
        }

        [HttpGet]
        public async Task<IActionResult> Match([FromQuery] LiveReportBetListMetaModel metaModel, bool dataOnly = true)
        {
            metaModel.UspKey = MatchSpKey;

            if (dataOnly)
            {
                return Ok(await GetData(metaModel));
            }

            var settings = CreateDefaultDayRangeSettings(metaModel);
            settings.Title = GetMatchTitle(metaModel);

            return Ok(await GetDataWithSettings(metaModel, settings));
        }

        [HttpGet]
        public async Task<IActionResult> Betradar([FromQuery] LiveReportBetListMetaModel metaModel, int page, int rows,
                             bool dataOnly = true)
        {
            metaModel.UspKey = LiveReportSpKey;

            if (dataOnly)
            {
                return Ok(await GetPagingData(metaModel, page, rows));
            }

            var settings = CreateDefaultDayRangeSettings(metaModel);
            settings.Title = $"{ViewResources.BetList} - {_cacheRefAdapter.GetSportName(metaModel.SportType)}";
            settings.IsServerPaging = true;

            return Ok(await GetPagingDataWithSettings(metaModel, settings, page, rows));
        }

        [HttpPost]
        public async Task<IActionResult> BetradarExportExcel([FromBody] LiveReportBetListMetaModel metaModel)
        {
            metaModel.UspKey = LiveReportSpKey;

            return await ExportExcel(metaModel);
        }

        private string GetMatchTitle(BetListMetaModel metaModel)
        {
            var matchName = metaModel.HomeId > 0 && metaModel.AwayId > 0 ?
                _cacheRefAdapter.GetTeamName(metaModel.HomeId) + " vs " +
                _cacheRefAdapter.GetTeamName(metaModel.AwayId) :
                string.Empty;

            var betTypeName = _cacheRefAdapter.GetBettype(metaModel.BetType);

            if (!string.IsNullOrEmpty(betTypeName))
            {
                betTypeName = $" - {betTypeName}";
            }

            if (string.IsNullOrEmpty(matchName) && string.IsNullOrEmpty(betTypeName))
            {
                return ViewResources.BetList;
            }

            return $"{ViewResources.BetList} - {matchName}{betTypeName}";
        }

        [HttpPost]
        public async Task<IActionResult> OutrightExportExcel([FromBody] LiveReportBetListMetaModel metaModel)
        {
            metaModel.UspKey = OutRightSpKey;

            return await ExportExcel(metaModel, ViewResources.Outright);
        }

        [HttpGet]
        public async Task<IActionResult> Outright([FromQuery] LiveReportBetListMetaModel metaModel,
                                                  bool dataOnly = true)
        {
            metaModel.UspKey = OutRightSpKey;

            if (dataOnly)
            {
                return Ok(await GetData(metaModel));
            }

            var settings = CreateDefaultDayRangeSettings(metaModel);
            settings.Title = $"{ViewResources.BetList} - {ViewResources.Outright}";

            return Ok(await GetDataWithSettings(metaModel, settings));
        }

        [HttpPost]
        public async Task<IActionResult> NumberGameExportExcel([FromBody] LiveReportBetListMetaModel metaModel)
        {
            metaModel.UspKey = NumberGameSpKey;
            metaModel.IsServerPaging = true;

            return await ExportExcel(metaModel, ViewResources.ProductNG);
        }

        [HttpGet]
        public async Task<IActionResult> NumberGame([FromQuery] LiveReportBetListMetaModel metaModel, int page,
                                                    int rows, bool dataOnly = true)
        {
            metaModel.UspKey = NumberGameSpKey;
            metaModel.IsServerPaging = true;

            if (dataOnly)
            {
                return Ok(await GetPagingData(metaModel, page, rows));
            }

            var settings = CreateDefaultDayRangeSettings(metaModel);
            settings.Title = $"{ViewResources.BetList} - {ViewResources.ProductNG}";
            settings.IsServerPaging = true;

            return Ok(await GetPagingDataWithSettings(metaModel, settings, page, rows));
        }

        [HttpPost]
        public async Task<IActionResult> MixParlayExportExcel([FromBody] LiveReportBetListMetaModel metaModel)
        {
            metaModel.UspKey = MixParlaySpKey;

            return await ExportExcel(metaModel);
        }

        [HttpGet]
        public async Task<IActionResult> MixParlay([FromQuery] LiveReportBetListMetaModel metaModel, int page, int rows,
                                                   bool dataOnly = true)
        {
            metaModel.UspKey = MixParlaySpKey;

            if (dataOnly)
            {
                return Ok(await GetPagingData(metaModel, page, rows));
            }

            var settings = CreateDefaultDayRangeSettings(metaModel);
            settings.Title = $"{ViewResources.BetList} - {_cacheRefAdapter.GetBettype(metaModel.BetType)}";
            settings.IsServerPaging = true;

            return Ok(await GetPagingDataWithSettings(metaModel, settings, page, rows));
        }

        [HttpPost]
        public async Task<IActionResult> VirtualSportsExportExcel([FromBody] LiveReportBetListMetaModel metaModel)
        {
            metaModel.UspKey = VirtualSportSpKey;

            return await ExportExcel(metaModel, _cacheRefAdapter.GetSportName(metaModel.SportType));
        }

        [HttpGet]
        public async Task<IActionResult> VirtualSports([FromQuery] LiveReportBetListMetaModel metaModel, int page,
                                                       int rows, bool dataOnly = true)
        {
            metaModel.UspKey = VirtualSportSpKey;

            if (dataOnly)
            {
                return Ok(await GetPagingData(metaModel, page, rows));
            }

            var settings = CreateDefaultSettings(metaModel);
            settings.Title = $"{ViewResources.BetList} - {_cacheRefAdapter.GetSportName(metaModel.SportType)}";
            settings.IsServerPaging = true;

            return Ok(await GetPagingDataWithSettings(metaModel, settings, page, rows));
        }

        [HttpPost]
        public async Task<IActionResult> SportsLotteryExportExcel([FromBody] LiveReportBetListMetaModel metaModel)
        {
            metaModel.UspKey = LiveReportSpKey;
            metaModel.ProductCodeList += $",{Product.SLT}";

            return await ExportExcel(metaModel, _cacheRefAdapter.GetSportName(metaModel.SportType));
        }

        [HttpGet]
        public async Task<IActionResult> SportsLottery([FromQuery] LiveReportBetListMetaModel metaModel, int page,
                                                       int rows, bool dataOnly = true)
        {
            metaModel.UspKey = LiveReportSpKey;
            metaModel.ProductCodeList += $",{Product.SLT}";

            if (dataOnly)
            {
                return Ok(await GetPagingData(metaModel, page, rows));
            }

            var settings = CreateDefaultSettings(metaModel);
            settings.Title = $"{ViewResources.BetList} - {_cacheRefAdapter.GetSportName(metaModel.SportType)}";
            settings.IsServerPaging = true;

            return Ok(await GetPagingDataWithSettings(metaModel, settings, page, rows));
        }

        [HttpPost]
        public async Task<IActionResult> BitCoinExportExcel([FromBody] LiveReportBetListMetaModel metaModel)
        {
            metaModel.UspKey = LiveReportSpKey;
            metaModel.ProductCodeList += $",{Product.BTC}";

            return await ExportExcel(metaModel, metaModel.ProductName);
        }

        [HttpGet]
        public async Task<IActionResult> BitCoin([FromQuery] LiveReportBetListMetaModel metaModel, int page, int rows,
                                                 bool dataOnly = true)
        {
            metaModel.UspKey = LiveReportSpKey;
            metaModel.ProductCodeList += $",{Product.BTC}";

            if (dataOnly)
            {
                return Ok(await GetPagingData(metaModel, page, rows));
            }

            var settings = CreateDefaultDayRangeSettings(metaModel);
            settings.Title = $"{ViewResources.BetList} - {metaModel.ProductName}";
            settings.IsServerPaging = true;

            return Ok(await GetPagingDataWithSettings(metaModel, settings, page, rows));
        }

        [HttpPost]
        public async Task<IActionResult> VS3ExportExcel([FromBody] LiveReportBetListMetaModel metaModel)
        {
            metaModel.UspKey = LiveReportSpKey;

            return await ExportExcel(metaModel);
        }

        [HttpGet]
        public async Task<IActionResult> VS3([FromQuery] LiveReportBetListMetaModel metaModel, int page, int rows,
                                             bool dataOnly = true)
        {
            metaModel.UspKey = LiveReportSpKey;

            if (dataOnly)
            {
                return Ok(await GetPagingData(metaModel, page, rows));
            }

            var settings = CreateDefaultDayRangeSettings(metaModel);
            settings.Title = $"{ViewResources.BetList} - {_cacheRefAdapter.GetSportName(metaModel.SportType)}";
            settings.IsServerPaging = true;

            return Ok(await GetPagingDataWithSettings(metaModel, settings, page, rows));
        }


        private async Task<IActionResult> ExportExcel(LiveReportBetListMetaModel metaModel, string caption = null)
        {
            await AssignDefaultBetListMetaModel(metaModel);

            metaModel.StartIndex = (metaModel.PageIndex - 1) * metaModel.PageSize + 1;
            metaModel.Title = GetExcelTitle(metaModel, caption);
            metaModel.HiddenColumns = HiddenColumns;
            metaModel.ShowingOption = new ShowingOption
            {
                CustomerCommVisible = ContextService.IsAllowedCustomerComm
            };

            return await ProcessExportExcel(metaModel, ExcelFileName, "/BetList/ExportExcelLiveReport")
               .ConfigureAwait(false);
        }

        private string GetExcelTitle(LiveReportBetListMetaModel metaModel, string caption = null)
        {
            var betListCaption = string.IsNullOrEmpty(caption) ?
                GetMatchName(metaModel) + _cacheRefAdapter.GetBettype(metaModel.BetType) :
                caption;
            var viewInCaption = GetViewInCurrency(metaModel);
            return
                $"{ViewResources.BetList} - {betListCaption} - {metaModel.FromDate.ToShortDateString()} --> {metaModel.ToDate.ToShortDateString()} - {viewInCaption}";
        }

        #region Utility

        private string GetMatchName(LiveReportBetListMetaModel model) =>
            model.HomeId > 0 && model.AwayId > 0 ?
                _cacheRefAdapter.GetTeamName(model.HomeId) + " vs " + _cacheRefAdapter.GetTeamName(model.AwayId) + " " :
                string.Empty;

        private async Task<string> GetData(LiveReportBetListMetaModel metaModel)
        {
            await AssignDefaultBetListMetaModel(metaModel);

            return await BetListApiClient.PostStringAsync("/LiveReportBetList/GetData",
                metaModel, ContextService.Language).ConfigureAwait(false);
        }

        private async Task<BetListResultModel<BetListDataResultModel>> GetDataWithSettings(
            LiveReportBetListMetaModel metaModel, BetListSettingModel settings)
        {
            await AssignDefaultBetListMetaModel(metaModel);

            var data = await BetListApiClient.PostAsync<BetListDataResultModel>("/LiveReportBetList/GetData",
                metaModel,
                ContextService.Language
            ).ConfigureAwait(false);

            return new BetListResultModel<BetListDataResultModel>
            {
                Data = data,
                Settings = settings
            };
        }

        private async Task<string> GetPagingData(LiveReportBetListMetaModel metaModel, int pageIndex, int pageSize)
        {
            await AssignDefaultPagingBetListMetaModel(metaModel, pageIndex, pageSize);

            return await BetListApiClient.PostStringAsync("/LiveReportBetList/GetPagingData",
                metaModel, ContextService.Language).ConfigureAwait(false);
        }

        private async Task<BetListResultModel<LiveReportBetListPagingDataResultModel>> GetPagingDataWithSettings(
            LiveReportBetListMetaModel metaModel,
            BetListSettingModel settings,
            int pageIndex,
            int pageSize)
        {
            await AssignDefaultPagingBetListMetaModel(metaModel, pageIndex, pageSize);
            var data = await BetListApiClient.PostAsync<LiveReportBetListPagingDataResultModel>(
                "/LiveReportBetList/GetPagingData",
                metaModel,
                ContextService.Language
            ).ConfigureAwait(false);

            settings.IsAllowedCustomerCommission = metaModel.HasPlayerCom;
            return new BetListResultModel<LiveReportBetListPagingDataResultModel>
            {
                Data = data,
                Settings = settings
            };
        }
        #endregion Utility
    }
}
