namespace Fanex.Athena.Report.BySite.Controllers
{
    using Fanex.Athena.Report.BySite.DTOs;
    using Fanex.Athena.Report.BySite.DTOs.WinlossAnalysis;
    using Fanex.Athena.Report.BySite.Enums;
    using Fanex.Athena.Report.BySite.Excels.Interfaces;
    using Fanex.Athena.Report.BySite.Excels.Options;
    using Fanex.Athena.Report.BySite.MetaModels;
    using Fanex.Athena.Report.BySite.Models;
    using Fanex.Athena.Report.BySite.Services.Interfaces;
    using Fanex.Athena.Report.Common.Constants;
    using Fanex.Athena.Report.Common.Excel;
    using Fanex.Athena.Report.Common.Helpers;
    using Fanex.Athena.Report.Common.Model.DTO;
    using Fanex.Athena.Report.Common.Model.MetaModels;
    using Fanex.Athena.Report.Common.Services.Interfaces;
    using Fanex.Athena.Report.Core.Attributes;
    using Fanex.Athena.Report.Core.Constants;
    using Fanex.Athena.Report.Core.Models.DTOs;
    using Fanex.Athena.Report.Core.Services.Interfaces;
    using Fanex.Athena.Report.Resources;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.FileProviders;
    using Microsoft.Extensions.Options;
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using static Fanex.Athena.Report.BySite.Constants.ReportConstant;

    [ApiController]
    [Route("api/[controller]/[action]")]
    [BearerAuthorize(Roles.PortalSuperAdmin, Roles.WinlossAnalysis)]
    [RequestValidation(validateCurrencyList: true)]
    [AutoBindingMetaModel(bindAgent: true)]
    public class WinlossAnalysisController : ControllerBase
    {
        private const int MAX_HOUR = 24;
        private const string FILE_NAME_BOTH_GROUP_BY_AND_BREAK_DOWN = "WinLossReportGroupByAndBreakdownBy";
        private const string FILE_NAME_GROUP_BY_ONLY = "WinLossReportGroupBy";
        private const int MAX_EXCEL_PAGE_SIZE = 100000;

        private readonly IWinlossAnalysisService _service;
        private readonly IFileService _fileService;
        private readonly IWinlossAnalysisExcel _excelService;
        private readonly IFileProvider _fileProvider;
        private readonly IContextService _contextService;
        private readonly ICompressService _compressService;
        private readonly ISubsidiaryService _subsidiaryService;
        private readonly IProductService _productService;
        private readonly IBaseCurrencyService _baseCurrencyService;
        private readonly ICustomerClassService _customerClassService;
        private readonly ExcelOptions _option;

#pragma warning disable S107 // Methods should not have too many parameters
        public WinlossAnalysisController(IWinlossAnalysisService service,
           IWinlossAnalysisExcel excelService,
           IFileService fileService,
           IFileProvider fileProvider,
           IContextService contextService,
           ICompressService compressService,
           IOptions<ExcelOptions> options,
           ISubsidiaryService subsidiaryService,
           IProductService productService,
           IBaseCurrencyService baseCurrencyService,
           ICustomerClassService customerClassService)
#pragma warning restore S107 // Methods should not have too many parameters
        {
            _service = service;
            _excelService = excelService;
            _fileService = fileService;
            _fileProvider = fileProvider;
            _contextService = contextService;
            _compressService = compressService;
            _subsidiaryService = subsidiaryService;
            _productService = productService;
            _baseCurrencyService = baseCurrencyService;
            _customerClassService = customerClassService;
            _option = options.Value;
        }

        [HttpPost]
        [SkipRequestValidation]
        public async Task<IActionResult> Index()
        {
            var productAccess = _productService.GetProductAccess(
                _contextService.LicenseeId,
                FunctionNames.WinLossAnalysis);

            var productOptions = MoveBaProductToBelowSportBook(productAccess.ProductList).Select(i => new ProductOption
            {
                Code = i.ProductCode,
                Name = i.ProductName,
                Status = true
            }).ToList();

            var subsidiaries = await _subsidiaryService.GetWhiteLabelTree(_contextService.SiteName);

            return Ok(new
            {
                currencyList = _contextService.CurrencyList.ToArray(),
                liveStatusList = GetLiveStatusList().ToArray(),
                statusList = GetStatusList(),
                productOptions,
                licenseeName = _contextService.SiteName,
                isOneworksMaster = IsOneworksMaster,
                subsidiaryViewType = IsOneworksMaster ? SubsidiaryViewType.MasterManagement : _contextService.SubsidiaryViewType,
                isAllowedCustomerCommission = _contextService.IsAllowedCustomerComm,
                baseCurrencyList = _baseCurrencyService.GetBaseCurrencyList().ToArray(),
                defaultBaseCurrency = _contextService.DefaultBaseCurrency,
                whitelabelList = subsidiaries,
                subsidiaryList = subsidiaries.SelectMany(x => x.Children.Select(y => y.Value)).Distinct(),
                platformList = PlatformHelper.GetPlatformList(_contextService.SpecialPlatform).ToArray(),
                isHasPermissionViewVip = _contextService.IsInRoles(Roles.PlayerVIPManagement),
                oddsGroupList = _contextService.OfferedOddsGroupsList,
                isOfferCashOutProduct = _productService.IsOfferCashOutProduct(_contextService.LicenseeId),
                riskLevel = await _customerClassService.GetAllRiskLevel(),
                hourList = GetHourList().ToArray()
            });
        }

        private static IDictionary<int, string> GetLiveStatusList()
        {
            return new Dictionary<int, string>
            {
                { LiveStatusTypes.NonLive, ViewResources.NoneLive },
                { LiveStatusTypes.Live, ViewResources.Live },
                { LiveStatusTypes.Mix, ViewResources.MixLiveAndNoneLive }
            };
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

        private static IEnumerable<ProductInfo> MoveBaProductToBelowSportBook(ReadOnlyCollection<ProductInfo> productList)
        {
            var list = productList.ToList();
            var baProduct = list.Find(c => c.ProductID == ProductTypes.BA);
            var sbProduct = list.Find(c => c.ProductID == ProductTypes.Sportsbook);
            if (baProduct != null && sbProduct != null)
            {
                list.Remove(baProduct);
                list.Insert(list.IndexOf(sbProduct) + 1, baProduct);
            }
            return list;
        }

        private bool IsOneworksMaster =>
            _contextService.UserLevel == UserLevelType.OneWorks
         && _contextService.SubsidiaryViewType == SubsidiaryViewType.NormalManagement;

        [HttpPost]
        public async Task<IActionResult> GetData([FromBody] WinlossAnalysisMetaModel metaModel)
        {
            metaModel.SpecialPlatform = _contextService.SpecialPlatform;
            metaModel.SpecialPlatformType = _contextService.SpecialPlatformType;
            return Ok(await _service.GetDataAsync(metaModel));
        }

        [HttpPost]
        public async Task<IActionResult> ExportExcel([FromBody] WinlossAnalysisMetaModel metaModel)
        {
            const string contentType = "application/octet-stream";
            metaModel.SpecialPlatform = _contextService.SpecialPlatform;
            metaModel.SpecialPlatformType = _contextService.SpecialPlatformType;
            AssignParamToExportExcel(metaModel);
            var data = await _service.GetDataAsync(metaModel);
            var excelOption = GenOptionFromParam(metaModel);
            var pagingDatas = PagingData(data, metaModel.ExportExcelMode, _option.MaximumRowPerFile);
            if (pagingDatas.Count() > 1 || metaModel.PageIndex > 1)
            {
                var zipFilePath = ExportToZipFile(pagingDatas, excelOption);
                var stream = new FileStream(zipFilePath, FileMode.Open, FileAccess.Read, FileShare.None, 4096, FileOptions.DeleteOnClose);
                return File(stream, "application/zip");
            }
            return File(ExportToExcelFile(data, excelOption), contentType);

        }

        private static void AssignParamToExportExcel(WinlossAnalysisMetaModel metaModel)
        {
            metaModel.PageSize = metaModel.PageSize > MAX_EXCEL_PAGE_SIZE ? MAX_EXCEL_PAGE_SIZE : metaModel.PageSize;
        }

        private IEnumerable<WinlossAnalysisResultModel> PagingData(WinlossAnalysisResultModel resultData, WinlossAnalysisExportMode exportMode, int maxRow)
        {
            var listPagingData = new LinkedList<WinlossAnalysisResultModel>();
            var pagingData = new WinlossAnalysisResultModel
            {
                SumTotal = resultData.SumTotal,
                TotalRow = resultData.TotalRow
            };
            var mainData = new LinkedList<TreeViewItem<WinlossAnalysisDTO, WinlossAnalysisDTO>>();
            pagingData.Data = mainData;
            var totalRow = 0;
            var lastItem = resultData.Data.LastOrDefault();
            foreach (var item in resultData.Data)
            {
                mainData.AddLast(item);
                var childernRowCount = exportMode == WinlossAnalysisExportMode.All ? item.Children.Count() : 0;
                totalRow = totalRow + 1 + childernRowCount;
                if (item.KeyItem == lastItem?.KeyItem || totalRow >= maxRow)
                {
                    listPagingData.AddLast(pagingData);
                    pagingData = new WinlossAnalysisResultModel
                    {
                        SumTotal = resultData.SumTotal,
                        TotalRow = resultData.TotalRow
                    };
                    mainData = new LinkedList<TreeViewItem<WinlossAnalysisDTO, WinlossAnalysisDTO>>();
                    pagingData.Data = mainData;
                    totalRow = 0;
                }
            }
            return listPagingData;
        }

        private string ExportToZipFile(IEnumerable<WinlossAnalysisResultModel> pagingDatas, WinlossAnalysisExcelOption excelOption)
        {
            var pageIndex = 0;
            var listEntry = new List<ZipEntry>(pagingDatas.Count());
            var prefixFileName = excelOption.ExportExcelMode == WinlossAnalysisExportMode.All
                ? FILE_NAME_BOTH_GROUP_BY_AND_BREAK_DOWN
                : FILE_NAME_GROUP_BY_ONLY;
            foreach (var item in pagingDatas)
            {
                var excelFileInByte = ExportToExcelFile(item, excelOption);
                var entry = WriteExcelFile(excelFileInByte, $"{prefixFileName}_{pageIndex}");
                listEntry.Add(entry);
                pageIndex++;

            }
            return _compressService.CompressDataToFile(listEntry, true);
        }

        private byte[] ExportToExcelFile(WinlossAnalysisResultModel data, WinlossAnalysisExcelOption excelOption)
        {
            var template = _fileProvider.GetFileInfo("/ExcelTemplate/WinLossAnalysis.xls").PhysicalPath;
            return _excelService.Export(template, data, excelOption);
        }

        private ZipEntry WriteExcelFile(byte[] excelFile, string fileName)
        {
            var path = _compressService.EnsureFolderPath();
            var excelFilePath = Path.Combine(path, $"{Guid.NewGuid()}.xls");
            _fileService.WriteAllBytes(excelFilePath, excelFile);
            return new ZipEntry
            {
                Path = excelFilePath,
                FileName = $"{fileName}.xls"
            };
        }

        private WinlossAnalysisExcelOption GenOptionFromParam(WinlossAnalysisMetaModel metaModel)
        {
            var isHasBAProduct = metaModel.ProductCodes.Split(',').Contains(Resources.Constants.Product.BA);
            return new WinlossAnalysisExcelOption
            {
                BaseCurrency = metaModel.BaseCurrencyName,
                BreakdownMode = metaModel.BreakdownMode,
                ViewInTime = metaModel.ViewInTime,
                FromDate = metaModel.FromDate,
                ToDate = metaModel.ToDate,
                GroupByMode = metaModel.GroupMode,
                IsHasBAProduct = isHasBAProduct,
                CurrencyList = metaModel.CurrencyCodes,
                ProductCodes = metaModel.ProductCodes,
                ExportExcelMode = metaModel.ExportExcelMode,
                SearchUsername = metaModel.SearchUserName
            };
        }

        private static IEnumerable<KeyValuePair<int, string>> GetHourList()
            => Enumerable.Range(0, MAX_HOUR).ToDictionary(_ => _, _ => $"{_:00}:00 - {_:00}:59");

    }
}