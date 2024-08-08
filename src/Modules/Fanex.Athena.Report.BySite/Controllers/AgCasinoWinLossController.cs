using System;
using System.Linq;
using System.Threading.Tasks;
using Fanex.Athena.Report.BySite.DTOs;
using Fanex.Athena.Report.BySite.Excels.Interfaces;
using Fanex.Athena.Report.BySite.Excels.Options;
using Fanex.Athena.Report.BySite.MetaModels;
using Fanex.Athena.Report.BySite.Repositories.Interfaces;
using Fanex.Athena.Report.Common.Model.ViewModel;
using Fanex.Athena.Report.Common.Services.Interfaces;
using Fanex.Athena.Report.Core.Attributes;
using Fanex.Athena.Report.Core.Constants;
using Fanex.Athena.Report.Core.Helpers;
using Fanex.Athena.Report.Core.Services.Interfaces;
using Fanex.Athena.Report.Resources;
using Fanex.Athena.Report.Resources.Constants;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.FileProviders;

namespace Fanex.Athena.Report.BySite.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    [BearerAuthorize(Roles.PortalSuperAdmin, Roles.AGCasinoWinLoss)]
    public class AgCasinoWinLossController : ControllerBase
    {
        private readonly IBalanceUpService _balanceUpService;

        private readonly IContextService _contextService;

        private readonly IThirdPartyWinLossRepository<ThirdPartyWinLoss> _thirdPartyWinLossRepository;

        public AgCasinoWinLossController(IContextService contextService, IBalanceUpService balanceUpService,
                                         IThirdPartyWinLossRepository<ThirdPartyWinLoss> thirdPartyWinLossRepository)
        {
            _contextService = contextService;
            _balanceUpService = balanceUpService;
            _thirdPartyWinLossRepository = thirdPartyWinLossRepository;
        }

        [HttpGet]
        [SkipRequestValidation]
        public IActionResult Index([FromServices] IBaseCurrencyService baseCurrencyService) =>
            Ok(new
            {
                Currencies = _contextService.CurrencyList.ToArray(),
                CurrencyId = _contextService.DefaultBaseCurrencyId,
                BaseCurrency = _contextService.DefaultBaseCurrency,
                BaseCurrencies = baseCurrencyService.GetBaseCurrencyList().ToArray(),
            });

        [HttpPost]
        public async Task<IActionResult> ExportExcel([FromServices] IFileProvider file,
                                                     [FromServices] IThirdPartyWinLossExcel<ThirdPartyWinLoss> excel,
                                                     [FromBody] ThirdPartyWinLossMetaModel metaModel)
        {
            metaModel.AgentListXml = AuthParameterHelper.GenerateAgentListXml(_contextService.AgentList, metaModel.CurrencyId, metaModel.IsExcludedTestData);
            metaModel.DefaultBaseCurrency = _contextService.DefaultBaseCurrency;
            metaModel.ProductName = ViewResources.ProductAGC;

            var data = await GetData(metaModel);
            var option = new ThirdPartyWinLossExcelOption
            {
                BaseCurrency = metaModel.BaseCurrency,
                CurrencyName = metaModel.CurrencyName
            };

            var template = file.GetFileInfo("/ExcelTemplate/ThirdPartyWinLoss.xls").PhysicalPath;

            var fileContent = excel.Export(template, data, metaModel, option);

            return new FileContentResult(fileContent, "application/octet-stream");
        }

        [HttpPost]
        public async Task<IActionResult> Data([FromBody] ThirdPartyWinLossMetaModel metaModel)
        {
            metaModel.AgentListXml = AuthParameterHelper.GenerateAgentListXml(_contextService.AgentList,
                metaModel.CurrencyId, metaModel.IsExcludedTestData);
            metaModel.DefaultBaseCurrency = _contextService.DefaultBaseCurrency;

            var data = await GetData(metaModel);

            return Ok(new
            {
                Data = data.DataList,
                Total = data.TotalDataList,
                data.SummaryTotal
            });
        }

        private async Task<ReportResultModel<ThirdPartyWinLoss>> GetData(ThirdPartyWinLossMetaModel metaModel)
        {
            metaModel.MoveBetsDay = (await _balanceUpService.GetBalanceUpAsync()).MoveBetsDay;

            return (ReportLevel)metaModel.DrillDownLevel switch
            {
                ReportLevel.Main => await _thirdPartyWinLossRepository
                                         .GetSummaryWinloss(metaModel, _contextService.CurrencyList, "Lrf_Rpt_AGCasinoWinloss")
                                         .ConfigureAwait(false),
                ReportLevel.Licensee => await _thirdPartyWinLossRepository
                                             .GetAgentWinlossDetail(metaModel, _contextService.CurrencyList, "LRF_Rpt_AGCasinoWinloss_Ag")
                                             .ConfigureAwait(false),
                ReportLevel.Customer => await _thirdPartyWinLossRepository
                                             .GetCustomerWinlossDetail(metaModel, _contextService.CurrencyList, "LRF_Rpt_AGCasinoWinloss_Mem")
                                             .ConfigureAwait(false),
                _ => throw new ArgumentException("Invalid level: " + metaModel.DrillDownLevel)
            };
        }
    }
}
