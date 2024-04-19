using System;
using System.Threading.Tasks;
using Fanex.Athena.Report.BetList.MetaModels;
using Fanex.Athena.Report.BetList.Models;
using Fanex.Athena.Report.Common.Services.Interfaces;
using Fanex.Athena.Report.Core.ApiClients.Abstraction;
using Fanex.Athena.Report.Core.Services.Interfaces;
using Fanex.Athena.Report.Resources;
using Microsoft.AspNetCore.Mvc;

namespace Fanex.Athena.Report.BetList.Controllers
{
    [ApiController]
    public class BetListBaseController : ControllerBase
    {
        protected IBalanceUpService BalanceUpService;

        protected IBetListApiClient BetListApiClient;

        protected IContextService ContextService;

        protected ILicenseeService LicenseeService;

        public BetListBaseController(IContextService contextService, ILicenseeService licenseeService,
                                     IBetListApiClient betListApiClient, IBalanceUpService balanceUpService)
        {
            ContextService = contextService;
            LicenseeService = licenseeService;
            BetListApiClient = betListApiClient;
            BalanceUpService = balanceUpService;
        }

        protected internal async Task<IActionResult> GetPagingBetListResult(
            BetListMetaModel metaModel,
            string uspKey,
            int pageIndex,
            int pageSize)
        {
            await AssignDefaultPagingBetListMetaModel(metaModel, uspKey, pageIndex, pageSize);

            var result = await BetListApiClient.PostStringAsync("/BetList/GetBetListPaging",
                metaModel,
                ContextService.Language
            ).ConfigureAwait(false);

            return Ok(result);
        }

        protected internal async Task<IActionResult> GetPagingBetListResultWithSettings(
            BetListMetaModel metaModel,
            BetListSettingModel settings,
            string uspKey,
            int pageIndex,
            int pageSize)
        {
            await AssignDefaultPagingBetListMetaModel(metaModel, uspKey, pageIndex, pageSize);

            var data = await BetListApiClient.PostAsync<BetListPagingDataResultModel>("/BetList/GetBetListPaging",
                metaModel,
                ContextService.Language
            ).ConfigureAwait(false);

            return Ok(new BetListResultModel<BetListPagingDataResultModel>
            {
                Data = data,
                Settings = settings
            });
        }

        protected internal virtual async Task AssignDefaultPagingBetListMetaModel(
            BetListMetaModel metaModel,
            string uspKey,
            int pageIndex,
            int pageSize)
        {
            const int DefaultPageSize = 100;

            await CheckDataPermission(metaModel.CustId, metaModel.TransId);

            metaModel.PageIndex = pageIndex < 1 ? 1 : pageIndex;
            metaModel.PageSize = pageSize < 1 ? DefaultPageSize : pageSize;
            metaModel.LicenseeId = ContextService.LicenseeId;
            metaModel.HasPlayerCom = ContextService.IsAllowedCustomerComm;
            metaModel.UspKey = uspKey;
            metaModel.IsShowIpvn = ContextService.IsShowIpVn;
            metaModel.IsShowIp = ContextService.IsShowIp;
        }

        protected internal async Task<IActionResult> GetBetListResult(
            BetListMetaModel metaModel,
            string uspKey)
        {
            await AssignDefaultBetListMetaModel(metaModel, uspKey);

            var result = await BetListApiClient
                              .PostStringAsync("/BetList/GetBetList", metaModel, ContextService.Language)
                              .ConfigureAwait(false);

            return Ok(result);
        }

        protected internal async Task<IActionResult> GetBetListResultWithSettings(
            BetListMetaModel metaModel,
            BetListSettingModel settings,
            string uspKey)
        {
            await AssignDefaultBetListMetaModel(metaModel, uspKey);

            var data = await BetListApiClient
                            .PostAsync<BetListDataResultModel>("/BetList/GetBetList", metaModel,
                                 ContextService.Language)
                            .ConfigureAwait(false);

            return Ok(new BetListResultModel<BetListDataResultModel>
            {
                Data = data,
                Settings = settings
            });
        }

        protected internal virtual async Task AssignDefaultBetListMetaModel(BetListMetaModel metaModel, string uspKey)
        {
            await CheckDataPermission(metaModel.CustId, metaModel.TransId);

            var balanceUp = await BalanceUpService.GetBalanceUpAsync();
            metaModel.PageIndex = 1;
            metaModel.PageSize = int.MaxValue - 1;
            metaModel.LicenseeId = ContextService.LicenseeId;
            metaModel.BalanceUpDay = DateTime.SpecifyKind(balanceUp.BalanceUpDay.Date, DateTimeKind.Utc);
            metaModel.MoveBetsDay = DateTime.SpecifyKind(balanceUp.MoveBetsDay.Date, DateTimeKind.Utc);
            metaModel.HasPlayerCom = ContextService.IsAllowedCustomerComm;
            metaModel.UspKey = uspKey;
            metaModel.IsShowIpvn = ContextService.IsShowIpVn;
            metaModel.IsShowIp = ContextService.IsShowIp;
        }

        protected static string GetViewInCurrency(BetListMetaModel metaModel) =>
            string.Format(
                ViewResources.Viewin,
                metaModel.IsConvert ? metaModel.BaseCurrency.ToUpperInvariant() : metaModel.CurrencyName);

        protected internal virtual async Task<IActionResult> ProcessExportExcel(
            BetListExcelMetaModel metaModel,
            string fileName,
            string apiPath = null)
        {
            var fileContent = await BetListApiClient
                                   .PostByteArrayAsync(apiPath ?? "/BetList/ExportExcel", metaModel,
                                        ContextService.Language).ConfigureAwait(false);

            return new FileContentResult(fileContent, "application/octet-stream");
        }

        protected async Task CheckDataPermission(int custId, long transId)
        {
            var hasPermission = await LicenseeService.HasPermissionOnData(ContextService.AgentList, custId, transId);
            if (!hasPermission)
            {
                throw new UnauthorizedAccessException();
            }
        }

        protected string GetControllerName() => ControllerContext.RouteData.Values["controller"]?.ToString();
    }
}
