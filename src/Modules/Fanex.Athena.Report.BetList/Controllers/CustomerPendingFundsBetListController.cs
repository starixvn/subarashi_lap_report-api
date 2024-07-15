namespace Fanex.Athena.Report.BetList.Controllers
{
    using System.Threading.Tasks;
    using Common.Helpers;
    using Common.Services.Interfaces;
    using Core.ApiClients.Abstraction;
    using Core.Attributes;
    using Core.Constants;
    using Core.Models.DTOs;
    using Core.Services.Interfaces;
    using MetaModels;
    using MetaModels.PendingFunds;
    using Microsoft.AspNetCore.Mvc;
    using Models.BetList;
    using Resources;

    [ApiController]
    [Route("api/[controller]/[action]")]
    [BearerAuthorize(Roles.PortalSuperAdmin, Roles.CustomerPendingFunds)]
    [RequestValidation(true)]
    [AutoBindingMetaModel(true)]
    public class CustomerPendingFundsBetListController : BetListBaseController
    {
        private const int ExpandedYears = 1;

        protected const int ExternalIdColumn = 1;

        protected const int SystemIdColumn = 2;

        protected const int WinLostColumn = 7;

        protected const int BetEffectiveColumn = 8;

        protected const int LicenseeWLColumn = 9;

        protected const string ExcelFileName = "LiveReportBetList";

        private static readonly int[] HiddenColList = { ExternalIdColumn, SystemIdColumn, WinLostColumn, BetEffectiveColumn, LicenseeWLColumn };

        public CustomerPendingFundsBetListController(
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
        public async Task<IActionResult> Index([FromQuery] PendingFundsBetListMetaModel metaModel, bool dataOnly = true)
        {
            await AssignDefaultMetaModel(metaModel);

            if (dataOnly)
            {
                return await GetBetListResult(metaModel).ConfigureAwait(false);
            }

            var title = $"{metaModel.ExternalId}{(metaModel.IsFakeLicensee ? string.Empty : $" ({metaModel.SystemId})")}";
            var settings = new BetListSettingModel
            {
                BaseCurrency = metaModel.BaseCurrency,
                Title = $"{ViewResources.BetList} ({ViewResources.Running}) - {title}"
            };

            return await GetBetListResultWithSettings(metaModel, settings).ConfigureAwait(false);
        }

        public async Task<IActionResult> ExportExcel([FromBody] PendingFundsBetListExcelMetaModel metaModel)
        {
            await AssignDefaultMetaModel(metaModel);

            metaModel.StartIndex = (metaModel.PageIndex - 1) * metaModel.PageSize + 1;
            metaModel.Title = GetExcelTitle(metaModel);
            metaModel.HiddenColumns = HiddenColList;
            metaModel.ShowingOption = new ShowingOption
            {
                CustomerCommVisible = ContextService.IsAllowedCustomerComm
            };

            return await ProcessExportExcel(metaModel, ExcelFileName).ConfigureAwait(false);
        }

        private string GetExcelTitle(BetListMetaModel metaModel) => "Running " + ViewResources.BetList
                                                                               + string.Format(
                                                                                   " - {0} {1} - {2}",
                                                                                   metaModel.ExternalId,
                                                                                   metaModel.IsFakeLicensee
                                                                                       ? string.Empty
                                                                                       : $"({metaModel.SystemId})",
                                                                                   GetViewInCurrency(metaModel));

        private async Task AssignDefaultMetaModel(BetListMetaModel metaModel)
        {
            await AssignDefaultBetListMetaModel(metaModel);

            metaModel.UspKey = "Shard_Lrf_BetList_SelRunningByDate_Mem";

            metaModel.FromDate = DateTimeHelper.Now.AddYears(-ExpandedYears);
            metaModel.ToDate = DateTimeHelper.Now.AddYears(ExpandedYears);
        }
    }
}