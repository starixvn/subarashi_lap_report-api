using System;
using System.Threading.Tasks;
using Fanex.Athena.Report.BetList.MetaModels;
using Fanex.Athena.Report.BetList.MetaModels.MatchWinLoss;
using Fanex.Athena.Report.Common.Services.Interfaces;
using Fanex.Athena.Report.Core.ApiClients.Abstraction;
using Fanex.Athena.Report.Core.Attributes;
using Fanex.Athena.Report.Core.Constants;
using Fanex.Athena.Report.Core.Models.DTOs;
using Fanex.Athena.Report.Core.Services.Interfaces;
using Fanex.Athena.Report.Resources;
using Microsoft.AspNetCore.Mvc;

namespace Fanex.Athena.Report.BetList.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    [BearerAuthorize(Roles.PortalSuperAdmin, Roles.MatchWinLoss)]
    [RequestValidation(true)]
    [AutoBindingMetaModel(bindProductCodeList: true, bindAgent: true)]
    public class MatchWinLossBetListController : BetListBaseController
    {
        protected const string ExcelFileName = "MatchWinLossBetList";

        protected const int PageIndex = 1;

        protected const int PageSizeExcel = 63000;

        protected const string SP_BETLIST_MATCHWL = "Lrf_BetList_MatchWL";

        protected const string SP_BETLIST_MATCHWL_OLD = "Lrf_BetList_MatchWL_Old";

        protected static readonly int[] HiddenColumns = { BetEffectiveColumn, };

        private readonly ICacheRefAdapter _cacheRefAdapter;

        public MatchWinLossBetListController(IContextService contextService, ILicenseeService licenseeService,
                                             IBetListApiClient betListApiClient, IBalanceUpService balanceUpService,
                                             ICacheRefAdapter cacheRefAdapter) :
            base(contextService, licenseeService, betListApiClient, balanceUpService)
        {
            _cacheRefAdapter = cacheRefAdapter;
        }

        [HttpGet]
        public async Task<IActionResult> Outright([FromQuery] BetListMetaModel metaModel, bool dataOnly = true)
        {
            await AssignDefaultMetalModel(metaModel);
            metaModel.IsServerPaging = true;

            if (dataOnly)
            {
                return await GetBetListResult(metaModel).ConfigureAwait(false);
            }

            var settings = CreateDefaultSettings(metaModel);
            settings.Title = $"{ViewResources.BetList} - {ViewResources.Outright}";

            return await GetBetListResultWithSettings(metaModel, settings);
        }

        [HttpGet]
        public async Task<IActionResult> MatchBetType([FromQuery] BetListMetaModel metaModel, int page, int rows,
                                                      bool dataOnly = true)
        {
            await AssignDefaultMetalModel(metaModel);

            if (dataOnly)
            {
                return await GetPagingBetListResult(metaModel, page, rows).ConfigureAwait(false);
            }

            var settings = CreateDefaultSettings(metaModel);
            settings.Title = GetMatchBetTypeTitle(metaModel);
            settings.IsServerPaging = true;

            return await GetPagingBetListResultWithSettings(metaModel, settings, page, rows);
        }

        private string GetMatchBetTypeTitle(BetListMetaModel metaModel)
        {
            var matchName = metaModel.HomeId > 0 && metaModel.AwayId > 0 ?
                _cacheRefAdapter.GetTeamName(metaModel.HomeId) + " vs " +
                _cacheRefAdapter.GetTeamName(metaModel.AwayId) :
                string.Empty;
            var betTypeName = metaModel.BetType > 0 ?
                " - " +
                _cacheRefAdapter.GetBettype(metaModel.BetType) :
                string.Empty;

            if (string.IsNullOrEmpty(matchName) && string.IsNullOrEmpty(betTypeName))
            {
                return ViewResources.BetList;
            }

            return $"{ViewResources.BetList} - {matchName}{betTypeName}";
        }

        [HttpGet]
        public async Task<IActionResult> BetTypeWinLoss([FromQuery] BetListMetaModel metaModel, int page, int rows,
                                                        bool dataOnly = true)
        {
            await AssignDefaultMetalModel(metaModel);

            if (dataOnly)
            {
                return await GetPagingBetListResult(metaModel, page, rows).ConfigureAwait(false);
            }

            var settings = CreateDefaultSettings(metaModel);
            settings.Title = $"{ViewResources.BetList} - {_cacheRefAdapter.GetBettype(metaModel.BetType)}";
            settings.IsServerPaging = true;

            return await GetPagingBetListResultWithSettings(metaModel, settings, page, rows);
        }

        [HttpGet]
        public async Task<IActionResult> MixParlay([FromQuery] MixParlayMetaModel metaModel, int page, int rows,
                                                   bool dataOnly = true)
        {
            var apiModel = new BetListMetaModel
            {
                AgentListXml = metaModel.AgentListXml,
                UserId = ContextService.UserId,
                CurrencyId = metaModel.CurrencyId,
                BetType = metaModel.BetType,
                BaseCurrency = metaModel.BaseCurrency ?? string.Empty,
                FromDate = metaModel.FromDate,
                ToDate = metaModel.ToDate,
                SortBy = metaModel.SortBy,
                SortOrder = metaModel.SortOrder,
                ProductCodeList = metaModel.ProductCodeList,
                SubsidiarySearch = metaModel.SubsidiarySearch,
                SubsidiaryList = metaModel.SubsidiaryList
            };

            await AssignDefaultMetalModel(apiModel);

            if (dataOnly)
            {
                return await GetPagingBetListResult(apiModel, page, rows).ConfigureAwait(false);
            }

            var settings = CreateDefaultSettings(apiModel);
            settings.Title = $"{ViewResources.BetList} - {_cacheRefAdapter.GetBettype(metaModel.BetType)}";
            settings.IsServerPaging = true;

            return await GetPagingBetListResultWithSettings(apiModel, settings, page, rows);
        }

        [HttpPost]
        public async Task<IActionResult> ExportExcel(BetListExcelMetaModel metaModel)
        {
            await AssignDefaultMetalModel(metaModel);

            metaModel.PageIndex = PageIndex;
            metaModel.PageSize = PageSizeExcel;
            metaModel.HiddenColumns = HiddenColumns;
            metaModel.IsServerPaging = true;

            metaModel.ShowingOption = new ShowingOption
            {
                CustomerCommVisible = ContextService.IsAllowedCustomerComm
            };

            var matchName = metaModel.HomeId > 0 && metaModel.AwayId > 0 ?
                _cacheRefAdapter.GetTeamName(metaModel.HomeId) + " vs " +
                _cacheRefAdapter.GetTeamName(metaModel.AwayId) + " - " :
                string.Empty;
            var currencyTitle = GetViewInCurrency(metaModel);

            metaModel.Title = ViewResources.BetList
                            + string.Format(
                                  " - {0}{1} - {2}",
                                  matchName + (metaModel.BetType > 0 ?
                                      _cacheRefAdapter.GetBettype(metaModel.BetType) + " - " :
                                      string.Empty),
                                  metaModel.FromDate.ToShortDateString(),
                                  currencyTitle);

            return await ProcessExportExcel(metaModel, ExcelFileName).ConfigureAwait(false);
        }

        private async Task AssignDefaultMetalModel(BetListMetaModel metaModel)
        {
            var balanceUp = await BalanceUpService.GetBalanceUpAsync();
            metaModel.UspKey = GetSpNameBasedOnBalanceUpDay(metaModel.FromDate.Date, balanceUp.BalanceUpDay);

            AssignSpecialPlatformValue(metaModel);
        }

        private string GetSpNameBasedOnBalanceUpDay(DateTime fromDate, DateTime balanceUpDay) =>
            fromDate < balanceUpDay ? SP_BETLIST_MATCHWL_OLD : SP_BETLIST_MATCHWL;
    }
}
