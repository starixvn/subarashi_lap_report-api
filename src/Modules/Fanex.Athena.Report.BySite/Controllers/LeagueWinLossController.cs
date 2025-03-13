namespace Fanex.Athena.Report.BySite.Controllers
{
    using Fanex.Athena.Report.BySite.DTOs;
    using Fanex.Athena.Report.BySite.Excels.Interfaces;
    using Fanex.Athena.Report.BySite.MetaModels;
    using Fanex.Athena.Report.BySite.Models;
    using Fanex.Athena.Report.BySite.Services.Interfaces;
    using Fanex.Athena.Report.Core.Attributes;
    using Fanex.Athena.Report.Core.Constants;
    using Fanex.Athena.Report.Core.Models.DTOs;
    using Fanex.Athena.Report.Core.Services.Interfaces;
    using Fanex.Athena.Report.Resources;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.FileProviders;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    [ApiController]
    [Route("api/[controller]/[action]")]
    [BearerAuthorize(Roles.PortalSuperAdmin, Roles.LeagueWinLoss)]
    [RequestValidation(validateCurrencyList: true)]
    [AutoBindingMetaModel(bindAgent: true)]
    public class LeagueWinLossController : ControllerBase
    {
        private readonly ILeagueWinLossService _service;
        private readonly IContextService _contextService;

        public LeagueWinLossController(ILeagueWinLossService service, IContextService contextService)
        {
            _service = service;
            _contextService = contextService;
        }

        [HttpPost]
        [SkipRequestValidation]
        public IActionResult Index([FromServices] IBaseCurrencyService baseCurrencyService, [FromServices] IProductService productService, [FromServices] IContextService contextService)
        {
            var productAccess = productService.GetSingleWalletProductAccess(
                contextService.LicenseeId,
                FunctionNames.LeagueWinLoss);

            return Ok(new
            {
                productOptions = productAccess.SelectProductOptions(),
                currencyList = contextService.CurrencyList.ToArray(),
                isAllowedCustomerCommission = contextService.IsAllowedCustomerComm,
                statusList = GetStatusList(),
                defaultBaseCurrency = contextService.DefaultBaseCurrency,
                baseCurrencyList = baseCurrencyService.GetBaseCurrencyList().ToArray(),
            });
        }

        private static IEnumerable<CheckedItem> GetStatusList()
        {
            return new List<CheckedItem>
            {
                new() { ID = StatusTypes.Won, Name = ViewResources.Won, Checked = true },
                new() { ID = StatusTypes.Lose, Name = ViewResources.Lost, Checked = true },
                new() { ID = StatusTypes.Draw, Name = ViewResources.Draw, Checked = true }
            };
        }

        [HttpPost]
        public async Task<LeagueWinLossResultModel> GetMainData([FromBody] LeagueWinLossMetaModel metaModel)
        {
            AssignCommonMetaModel(metaModel);

            var result = await _service.GetDataAsync(metaModel);

            if (!result.DataList.Any())
            {
                return GetEmptyResult();
            }

            return new LeagueWinLossResultModel
            {
                Data = result.DataList.ToList(),
                Total = result.TotalData
            };
        }

        private static LeagueWinLossResultModel GetEmptyResult()
        {
            return new LeagueWinLossResultModel
            {
                Data = new List<LeagueWinLoss>(),
                Total = new LeagueWinLoss()
            };
        }

        [HttpPost]
        public async Task<LeagueWinLossResultModel> GetCurrencyData([FromBody] LeagueWinLossMetaModel metaModel)
        {
            AssignCommonMetaModel(metaModel);

            var result = await _service.GetCurrencyDataAsync(metaModel);

            if (!result.DataList.Any())
            {
                return GetEmptyResult();
            }

            return new LeagueWinLossResultModel
            {
                Data = result.DataList.ToList()
            };
        }

        [HttpPost]
        public async Task<FileContentResult> ExportExcel(
            [FromServices] IFileProvider file,
            [FromServices] ILeagueWinLossExcel excel,
            [FromBody] LeagueWinLossMetaModel metaModel)
        {
            var template = file.GetFileInfo("/ExcelTemplate/LeagueWinLoss.xls").PhysicalPath;

            AssignCommonMetaModel(metaModel);

            var result = await _service.GetDataAsync(metaModel).ConfigureAwait(false);

            var fileContent = excel.Export(template, result, metaModel);

            return new FileContentResult(fileContent, "application/octet-stream");
        }

        private void AssignCommonMetaModel(LeagueWinLossMetaModel metaModel)
        {
            metaModel.SpecialPlatform = _contextService.SpecialPlatform;
        }
    }
}