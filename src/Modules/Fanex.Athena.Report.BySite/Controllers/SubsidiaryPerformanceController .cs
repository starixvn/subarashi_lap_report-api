namespace Fanex.Athena.Report.BySite.Controllers
{
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using Common.Model.ViewModel;
    using Core.Attributes;
    using Core.Constants;
    using Core.Helpers;
    using Core.Services.Interfaces;
    using DTOs.SubsidiaryPerformance;
    using Excels.Interfaces;
    using Excels.Options;
    using MetaModels;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.FileProviders;
    using Services.Interfaces;

    [ApiController]
    [Route("api/[controller]/[action]")]
    [BearerAuthorize(Roles.PortalSuperAdmin, Roles.SubsidiaryPerformance)]
    [AutoBindingMetaModel(bindProductCodeList: true)]
    public class SubsidiaryPerformanceController : ControllerBase
    {
        private readonly IContextService _contextService;

        private readonly ISubsidiaryPerformanceService _subsidiaryPerformanceService;

        private readonly ISubsidiaryService _subsidiaryService;

        public SubsidiaryPerformanceController(
            IContextService contextService,
            ISubsidiaryService subsidiaryService,
            ISubsidiaryPerformanceService subsidiaryPerformanceService)
        {
            _contextService = contextService;
            _subsidiaryService = subsidiaryService;
            _subsidiaryPerformanceService = subsidiaryPerformanceService;
        }

        [HttpGet]
        [SkipRequestValidation]
        public async Task<IActionResult> Index(
            [FromServices] IBaseCurrencyService baseCurrencyService,
            [FromServices] IProductService productService,
            [FromServices] ISubsidiaryService subsidiaryService)
        {
            var productAccess = productService.GetSingleWalletProductAccess(
                _contextService.LicenseeId,
                FunctionNames.SubsidiaryPerformance);

            return Ok(new
            {
                BaseCurrencies = baseCurrencyService.GetBaseCurrencyList().ToArray(),
                BaseCurrency = _contextService.DefaultBaseCurrency,
                Products = productAccess.SelectProductOptions(),
                SubsidiaryList = await subsidiaryService.GetSubsidiaryTree(_contextService.SiteName),
                ViewType = _contextService.SubsidiaryViewType,
                _contextService.IsHideSaba
            });
        }

        [HttpPost]
        public async Task<IActionResult> Data([FromBody] SubsidiaryPerformanceMetaModel metaModel)
        {
            AssignContextMetaModel(metaModel);
            await AssignDataAgentList(metaModel);
            var result = await _subsidiaryPerformanceService.GetData(metaModel, _contextService.CurrencyList).ConfigureAwait(false);

            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> DetailData([FromBody] SubsidiaryPerformanceMetaModel metaModel)
        {
            AssignContextMetaModel(metaModel);
            await AssignDetailAgentList(metaModel);
            if (string.IsNullOrEmpty(metaModel.AgentListXml))
            {
                var emptyResult = new ReportResultModel<SubsidiaryPerformanceDetail>();
                return Ok(new
                {
                    Data = emptyResult.DataList,
                    Total = emptyResult.SummaryTotal,
                    GrandTotal = emptyResult.TotalData
                });
            }

            var result = await _subsidiaryPerformanceService.GetDetailData(metaModel, _contextService.CurrencyList)
                .ConfigureAwait(false);

            return Ok(new
            {
                Data = result.DataList,
                Total = result.SummaryTotal,
                GrandTotal = result.TotalData
            });
        }

        [HttpPost]
        public async Task<IActionResult> ExportExcel(
            [FromServices] IFileProvider file,
            [FromServices] ISubsidiaryPerformanceSummaryExcel excelService,
            [FromBody] SubsidiaryPerformanceMetaModel metaModel)
        {
            AssignContextMetaModel(metaModel);
            await AssignDataAgentList(metaModel);

            var data = await _subsidiaryPerformanceService.GetData(metaModel, _contextService.CurrencyList).ConfigureAwait(false);
            var excelOption = new SubsidiaryPerformanceExcelOption
            {
                isMasterViewType = _contextService.SubsidiaryViewType == SubsidiaryViewType.MasterManagement
            };

            var template = file.GetFileInfo("/ExcelTemplate/SubsidiaryPerformance/SubsidiaryPerformanceSummary.xlsx").PhysicalPath;
            var fileDataContent = excelService.Export(template, data, metaModel, excelOption);
            var dataStream = new FileStream(fileDataContent, FileMode.Open, FileAccess.Read, FileShare.None, 4096, FileOptions.DeleteOnClose);

            return File(dataStream, "application/octet-stream");
        }

        [HttpPost]
        public async Task<IActionResult> ExportDetailExcel(
            [FromServices] IFileProvider file,
            [FromServices] ISubsidiaryPerformanceDetailExcel excelService,
            [FromBody] SubsidiaryPerformanceMetaModel metaModel)
        {
            AssignContextMetaModel(metaModel);
            await AssignDetailAgentList(metaModel);

            var detailData = await _subsidiaryPerformanceService.GetDetailData(metaModel, _contextService.CurrencyList);

            var template = file.GetFileInfo("/ExcelTemplate/SubsidiaryPerformance/SubsidiaryPerformanceDetail.xlsx").PhysicalPath;
            var fileDetailContent = excelService.Export(template, detailData, metaModel);
            var detailStream = new FileStream(fileDetailContent, FileMode.Open, FileAccess.Read, FileShare.None, 4096, FileOptions.DeleteOnClose);

            return File(detailStream, "application/octet-stream");
        }

        private async Task AssignDataAgentList(SubsidiaryPerformanceMetaModel metaModel)
        {
            string agentListXml;

            if (_contextService.UserLevel == UserLevelType.Subsidiary)
            {
                agentListXml = AuthParameterHelper.GenerateAgentListXml(_contextService.AgentList, isExcludeTestData: metaModel.IsExcludeTest);
            }
            else
            {
                var agentIdListByCurrency =
                    await _subsidiaryService.GetCurrencyAgentList(_contextService.SiteName, metaModel.SubsidiaryList, metaModel.IsExcludeTest);
                agentListXml = AuthParameterHelper.GenerateAgentListXml(_contextService.CurrencyList, agentIdListByCurrency);
            }

            metaModel.AgentListXml = agentListXml;
        }

        private async Task AssignDetailAgentList(SubsidiaryPerformanceMetaModel metaModel)
        {
            var agentIdListByCurrency =
                await _subsidiaryService.GetCurrencyAgentList(_contextService.SiteName, metaModel.SubsidiaryId, metaModel.IsExcludeTest);

            var agentList = agentIdListByCurrency.Any()
                ? AuthParameterHelper.GenerateAgentListXml(_contextService.CurrencyList, agentIdListByCurrency)
                : string.Empty;

            metaModel.AgentListXml = agentList;
        }

        private void AssignContextMetaModel(SubsidiaryPerformanceMetaModel metaModel)
        {
            metaModel.LicenseeName = _contextService.SiteName;
            metaModel.IsHideSaba = _contextService.IsHideSaba;
        }
    }
}