using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Fanex.Athena.Report.ByPlayer.DTO;
using Fanex.Athena.Report.ByPlayer.Excels.Interfaces;
using Fanex.Athena.Report.ByPlayer.Excels.Options;
using Fanex.Athena.Report.ByPlayer.MetaModels;
using Fanex.Athena.Report.ByPlayer.Models;
using Fanex.Athena.Report.ByPlayer.Service.Interfaces;
using Fanex.Athena.Report.Common.Constants;
using Fanex.Athena.Report.Common.Model.ViewModel;
using Fanex.Athena.Report.Common.Services.Interfaces;
using Fanex.Athena.Report.Common.Settings;
using Fanex.Athena.Report.Core.Attributes;
using Fanex.Athena.Report.Core.Constants;
using Fanex.Athena.Report.Core.Models.DTOs;
using Fanex.Athena.Report.Core.Services.Interfaces;
using Fanex.Athena.Report.Resources;
using Fanex.Athena.Report.Resources.Constants;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.FileProviders;

namespace Fanex.Athena.Report.ByPlayer.Controllers
{
    [Route("api/[controller]/[action]")]
    [BearerAuthorize(Roles.PortalSuperAdmin, Roles.CustomerTurnover)]
    [RequestValidation(validateCurrency: true)]
    [AutoBindingMetaModel(true)]
    public class CustomerTurnoverController : ControllerBase
    {
        private readonly AppSettings _appSettings;

        private readonly IContextService _contextService;


        public CustomerTurnoverController(AppSettings appSettings, IContextService contextService)
        {
            _appSettings = appSettings;
            _contextService = contextService;
        }

        [HttpPost]
        [SkipRequestValidation]
        public async Task<ActionResult> Index([FromServices] IProductService productService, [FromServices] ISportService sportService)
        {
            return Ok(new
            {
                currencyList = _contextService.CurrencyList.ToArray(),
                productOptions = await GetSportsAndProducts(productService, sportService),
                isAllowedCustomerCommission = _contextService.IsAllowedCustomerComm
            });
        }

        private async Task<List<SportProductItem>> GetSportsAndProducts(IProductService productService, ISportService sportService)
        {
            var productCode = productService.GetAllowedSubProductCodeList(_contextService.OfferedSubProductList, $"{Product.SB},{Product.NG},{Product.VS}");

            var data = await sportService
                            .GetSportsAndProducts(_contextService.LicenseeId, _contextService.Language, productCode)
                            .ConfigureAwait(false);
            var result = new List<SportProductItem>();
            result.AddRange(data);
            var parlayProduct = new SportProductItem
            {
                Checked = true,
                Id = ProductTypes.Parlay,
                Name = ViewResources.Parlay,
                ParentId = ProductTypes.Sportsbook
            };
            result.Add(parlayProduct);

            return result;
        }

        [HttpPost]
        [SkipRequestValidation]
        public async Task<ActionResult> GetBettypesBySport([FromServices] IBetTypeService betTypeService, [FromBody] IList<SportProductItem> sportProducts)
        {
            if (sportProducts is null || !sportProducts.Any())
            {
                return Ok(new List<BettypeProductItem>());
            }

            var data = await betTypeService.GetBettypes(sportProducts, _contextService.Language, _contextService.IsHideSaba).ConfigureAwait(false);

            return Ok(data);
        }

        [HttpPost]
        public async Task<ActionResult> GetData([FromServices] ICustomerTurnoverService customerTurnoverService, [FromBody] CustomerTurnoverMetaModel metaModel)
        {
            metaModel.IsExportExcel = false;
            var result = await customerTurnoverService.GetDataList(metaModel, _contextService.CurrencyList, _contextService.Language).ConfigureAwait(false);

            return Ok(new CustomerTurnoverResultModel
            {
                Data = result.DataList,
                TotalData = result.TotalData,
                TotalRow = result.TotalRows
            });
        }

        [HttpPost]
        public async Task<IActionResult> ExportExcel([FromServices] IFileProvider file,
                                                     [FromServices] ICompressService compressService,
                                                     [FromServices] ICustomerTurnoverExcel excel,
                                                     [FromServices] ICustomerTurnoverService customerTurnoverService,
                                                     [FromBody] CustomerTurnoverMetaModel metaModel)
        {

            metaModel.IsExportExcel = true;
            var data = await customerTurnoverService.GetDataList(metaModel, _contextService.CurrencyList, _contextService.Language).ConfigureAwait(false);
            var pagingData = PagingData(data).ToList();

            var template = file.GetFileInfo("/ExcelTemplate/CustomerTurnover.xls").PhysicalPath;
            var option = new CustomerTurnoverExcelOptions
            {
                CustomerCommVisible = _contextService.IsAllowedCustomerComm,
                FormatDateTime = metaModel.ReportBy == 1 ? "MM/dd/yyyy" : "MM/yyyy",
                CurrencyName = _contextService.CurrencyList.Where(i => i.Key == metaModel.CurrencyId).Select(i => i.Value).SingleOrDefault()
            };

            if (pagingData.Count <= 1)
            {
                var fileContent = excel.ExportFile(template, data, metaModel, option);
                return new FileContentResult(fileContent, "application/octet-stream");
            }

            var zipContent = excel.ExportZip(template, pagingData, metaModel, option);
            var zipFile = compressService.CompressDataToFile(zipContent, true);

            var stream = new FileStream(zipFile, FileMode.Open, FileAccess.Read, FileShare.None, 4096, FileOptions.DeleteOnClose);
            return new FileStreamResult(stream, "application/zip");
        }

        private IEnumerable<ReportResultModel<CustTurnover>> PagingData(ReportResultModel<CustTurnover> result)
        {
            var pagingData = new List<ReportResultModel<CustTurnover>>();
            var pagingItem = CopyData(result);
            var dataList = new List<CustTurnover>();
            var totalRow = 0;
            var lastIndex = -1;
            var lastRow = result.DataList.LastOrDefault();

            foreach (var item in result.DataList)
            {
                if (lastIndex != -1 && item.Index != lastIndex)
                {
                    pagingItem.DataList = dataList;
                    pagingData.Add(pagingItem);
                    totalRow = 0;
                    lastIndex = -1;
                    pagingItem = CopyData(result);
                    dataList = new List<CustTurnover>();
                }
                totalRow++;
                dataList.Add(item);
                if (item == lastRow)
                {
                    pagingItem.DataList = dataList;
                    pagingData.Add(pagingItem);
                    return pagingData;
                }
                if (totalRow == _appSettings.MaxRowPerExcelFile)
                {
                    lastIndex = item.Index;
                }
            }

            return pagingData;
        }

        private ReportResultModel<CustTurnover> CopyData(ReportResultModel<CustTurnover> result)
        {
            return new ReportResultModel<CustTurnover>
            {
                TotalData = result.TotalData,
                TotalDataList = result.TotalDataList,
                SummaryTotal = result.SummaryTotal,
                TotalRows = result.TotalRows,
            };
        }
    }
}
