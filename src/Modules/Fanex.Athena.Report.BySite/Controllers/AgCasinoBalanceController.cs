using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fanex.Athena.Report.BySite.DTOs;
using Fanex.Athena.Report.BySite.Excels.Interfaces;
using Fanex.Athena.Report.BySite.Excels.Options;
using Fanex.Athena.Report.BySite.MetaModels;
using Fanex.Athena.Report.BySite.Services.Interfaces;
using Fanex.Athena.Report.Core.Attributes;
using Fanex.Athena.Report.Core.Constants;
using Fanex.Athena.Report.Core.Helpers;
using Fanex.Athena.Report.Core.Services.Interfaces;
using Fanex.Core.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.FileProviders;

namespace Fanex.Athena.Report.BySite.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    [BearerAuthorize(Roles.PortalSuperAdmin, Roles.AGCasinoBalance)]
    public class AgCasinoBalanceController : ControllerBase
    {
        private readonly IAgCasinoBalanceService _agCasinoBalanceService;

        private readonly IContextService _contextService;

        public AgCasinoBalanceController(IContextService contextService, IAgCasinoBalanceService agCasinoBalanceService)
        {
            _contextService = contextService;
            _agCasinoBalanceService = agCasinoBalanceService;
        }

        [HttpGet]
        public IActionResult Index([FromServices] IBaseCurrencyService baseCurrencyService) =>
            Ok(new
            {
                Currencies = _contextService.CurrencyList.ToArray(),
                CurrencyId = _contextService.DefaultBaseCurrencyId,
                BaseCurrency = _contextService.DefaultBaseCurrency,
                BaseCurrencies = baseCurrencyService.GetBaseCurrencyList().ToArray()
            });

        [HttpPost]
        public async Task<IActionResult> Data([FromBody] ThirdPartyWinLossMetaModel metaModel)
        {
            AssignContextMetaModel(metaModel);
            var result = await _agCasinoBalanceService.GetData(metaModel, _contextService.CurrencyList);

            if (!result.DataList.Any())
            {
                return Ok(GetAgCasinoBalanceEmptyResult());
            }

            return Ok(new
            {
                result.TotalRows,
                Data = result.DataList,
                Total = CalculateTotal(result.DataList),
                result.SummaryTotal
            });
        }

        [HttpPost]
        public async Task<IActionResult> ExportExcel([FromServices] IFileProvider file,
                                                     [FromServices] IAgCasinoBalanceExcel excel,
                                                     [FromBody] ThirdPartyWinLossMetaModel metaModel)
        {
            metaModel.PageIndex = 1;
            metaModel.PageSize = SystemConstants.MaxDataTotalRow;
            AssignContextMetaModel(metaModel);

            var data = await _agCasinoBalanceService.GetData(metaModel, _contextService.CurrencyList);
            var option = new ThirdPartyWinLossExcelOption
            {
                BaseCurrency = metaModel.BaseCurrency,
                CurrencyName = metaModel.CurrencyName
            };

            var template = file.GetFileInfo("/ExcelTemplate/AgCasinoBalance.xls").PhysicalPath;

            var fileContent = excel.Export(template, data, metaModel, option);

            return new FileContentResult(fileContent, "application/octet-stream");
        }

        private static IList<AgCasinoBalance> CalculateTotal(IList<AgCasinoBalance> dataList)
        {
            return dataList.GroupBy(d => d.CurrencyName).Select(g => new AgCasinoBalance
            {
                CurrencyName = g.Key,
                TurnOver = g.Sum(p => p.TurnOver),
                TotalTransaction = g.Sum(p => p.TotalTransaction),
                CustomerWinLoss = g.Sum(p => p.CustomerWinLoss),
                LicenseeWL = g.Sum(p => p.LicenseeWL),
                OpeningBalance = g.Sum(p => p.OpeningBalance),
                TransferIn = g.Sum(p => p.TransferIn),
                TransferOut = g.Sum(p => p.TransferOut),
                ClosingBalance = g.Sum(p => p.ClosingBalance)
            }).ToList();
        }

        private static dynamic GetAgCasinoBalanceEmptyResult() =>
            new
            {
                TotalRows = 0,
                Data = new List<AgCasinoBalance>(),
                Total = new List<AgCasinoBalance>(),
                SummaryTotal = new Dictionary<string, AgCasinoBalance>()
            };

        private void AssignContextMetaModel(ThirdPartyWinLossMetaModel metaModel)
        {
            metaModel.AgentListXml = AuthParameterHelper.GenerateAgentListXml(_contextService.AgentList, metaModel.CurrencyId, metaModel.IsExcludedTestData);
            metaModel.DefaultBaseCurrency = _contextService.DefaultBaseCurrency;
        }
    }
}
