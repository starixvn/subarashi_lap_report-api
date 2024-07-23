namespace Fanex.Athena.Report.BetList.Controllers
{
    using System;
    using System.Threading.Tasks;
    using Common.Helpers;
    using Common.Services.Interfaces;
    using Core.ApiClients.Abstraction;
    using Core.Attributes;
    using Core.Constants;
    using Core.Models.DTOs;
    using Core.Services.Interfaces;
    using MetaModels;
    using Microsoft.AspNetCore.Mvc;

    [ApiController]
    [Route("api/[controller]/[action]")]
    [BearerAuthorize(Roles.PortalSuperAdmin, Roles.SabaVirtualSportsWinLoss)]
    [RequestValidation(true)]
    [AutoBindingMetaModel(true)]
    public class SabaVirtualSportsWinLossBetListController : BetListBaseController
    {
        private static readonly int[] HiddenColumns = { ExternalIdColumn, SystemIdColumn, BetEffectiveColumn };

        public SabaVirtualSportsWinLossBetListController(
            IContextService contextService,
            ILicenseeService licenseeService,
            IBetListApiClient betListApiClient,
            IBalanceUpService balanceUpService) : base(contextService,
            licenseeService,
            betListApiClient,
            balanceUpService)
        {
        }

        [HttpGet]
        public async Task<IActionResult> Index([FromQuery] BetListMetaModel metaModel, bool dataOnly = true)
        {
            if (!IsValidDateRange(metaModel.FromDate, metaModel.ToDate))
            {
                return BadRequest();
            }

            await AssignDefaultMetaModel(metaModel);
            metaModel.UspKey = "Lrf_BetList_SabaVirtualSports_Mem";

            if (dataOnly)
            {
                return await GetBetListResult(metaModel).ConfigureAwait(false);
            }

            var settings = CreateDefaultDayRangeSettings(metaModel);

            var systemIdTitle = metaModel.IsFakeLicensee ? string.Empty : $" ({metaModel.SystemId})";
            settings.Title = metaModel.ExternalId + systemIdTitle;

            return await GetBetListResultWithSettings(metaModel, settings).ConfigureAwait(false);
        }

        [HttpPost]
        public async Task<IActionResult> ExportExcel([FromBody] BetListExcelMetaModel metaModel)
        {
            await AssignDefaultMetaModel(metaModel);
            metaModel.UspKey = "Lrf_BetList_SabaVirtualSports_Mem";

            var currencyId = metaModel.IsConvert ? metaModel.BaseCurrencyId : metaModel.CurrencyId;
            metaModel.Title = GetCustomerBetListTitle(metaModel, GetViewInCurrency(currencyId, metaModel.CurrencyName));

            metaModel.ShowingOption = new ShowingOption
            {
                CustomerCommVisible = ContextService.IsAllowedCustomerComm
            };
            metaModel.HiddenColumns = HiddenColumns;

            return await ProcessExportExcel(metaModel, "SabaVirtualSportsBetList").ConfigureAwait(false);
        }

        private async Task AssignDefaultMetaModel(BetListMetaModel metaModel)
        {
            await AssignDefaultBetListMetaModel(metaModel);
            InitBaseCurrencyValue(metaModel);
        }

        private static bool IsValidDateRange(DateTime fromDate, DateTime toDate)
        {
            const string MinDate = "08/01/2018";
            var minDate = DateTimeOffset.Parse(MinDate).Date;
            var maxDate = DateTimeHelper.Now.Date;

            return DateTimeHelper.IsBetween(fromDate, minDate, maxDate) &&
                   DateTimeHelper.IsBetween(toDate, minDate, maxDate);
        }
    }
}