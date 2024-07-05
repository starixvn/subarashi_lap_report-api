namespace Fanex.Athena.Report.BySite.Controllers
{
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using Core.Attributes;
    using Core.Constants;
    using Core.Services.Interfaces;
    using Excels.Interfaces;
    using MetaModels;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.FileProviders;
    using Models;
    using Services.Interfaces;

    [ApiController]
    [Route("api/[controller]/[action]")]
    [BearerAuthorize(Roles.PortalSuperAdmin, Roles.WinLossByTimeZone)]
    [RequestValidation(true)]
    [AutoBindingMetaModel(true, bindProductCodeList: true)]
    public class WinLossByTimeZoneController : ControllerBase
    {
        private readonly IContextService _contextService;

        private readonly IProductService _productService;

        public WinLossByTimeZoneController(IContextService contextService, IProductService productService)
        {
            _contextService = contextService;
            _productService = productService;
        }

        [HttpGet]
        [SkipRequestValidation]
        public ActionResult Index([FromServices] IBaseCurrencyService baseCurrencyService)
        {
            var productAccess = _productService.GetSingleWalletProductAccess(_contextService.LicenseeId, FunctionNames.WinLossByTimeZone);

            return Ok(new
            {
                ProductOptions = productAccess.SelectProductOptions(),
                Currencies = _contextService.CurrencyList.ToArray(),
                BaseCurrency = _contextService.DefaultBaseCurrency,
                BaseCurrencies = baseCurrencyService.GetBaseCurrencyList().ToArray()
            });
        }

        [HttpPost]
        public async Task<ActionResult> Data(
            [FromServices] IWinLossByTimeZoneService winLossByTimeZoneService,
            [FromBody] WinLossByTimeZoneMetaModel metaModel)
        {
            var result = await winLossByTimeZoneService.GetData(metaModel, _contextService.CurrencyList)
                .ConfigureAwait(false);
            return Ok(new WinLossByTimeZoneResultModel
            {
                Data = result.DataList,
                Total = result.TotalData
            });
        }

        [HttpPost]
        public async Task<ActionResult> Excel(
            [FromServices] IFileProvider file,
            [FromServices] IWinLossByTimeZoneExcel excelService,
            [FromServices] IWinLossByTimeZoneService winLossByTimeZoneService,
            [FromBody] WinLossByTimeZoneMetaModel metaModel)
        {
            var data = await winLossByTimeZoneService.GetData(metaModel, _contextService.CurrencyList)
                .ConfigureAwait(false);

            if (!data.DataList.Any())
            {
                return NoContent();
            }

            var template = file.GetFileInfo("/ExcelTemplate/LicenseeWinLossByTimeZone.xlsx").PhysicalPath;
            var fileContent = excelService.Export(template, data, metaModel);

            var stream = new FileStream(fileContent, FileMode.Open, FileAccess.Read, FileShare.None, 4096, FileOptions.DeleteOnClose);
            return File(stream, "application/octet-stream");
        }
    }
}