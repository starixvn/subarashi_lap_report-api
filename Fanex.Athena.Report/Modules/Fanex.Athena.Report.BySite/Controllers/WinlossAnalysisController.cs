namespace Fanex.Athena.Report.BySite.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using Fanex.Athena.Report.BySite.DTOs;
    using Fanex.Athena.Report.BySite.Enums;
    using Fanex.Athena.Report.BySite.Excels.Interfaces;
    using Fanex.Athena.Report.BySite.Excels.Options;
    using Fanex.Athena.Report.BySite.MetaModels;
    using Fanex.Athena.Report.BySite.Models;
    using Fanex.Athena.Report.BySite.Services.Interfaces;
    using Fanex.Athena.Report.Common.Excel;
    using Fanex.Athena.Report.Common.Model.MetaModels;
    using Fanex.Athena.Report.Common.Services.Interfaces;
    using Fanex.Athena.Report.Core.Attributes;
    using Fanex.Athena.Report.Core.Constants;
    using Fanex.Athena.Report.Core.Services;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.FileProviders;
    using Microsoft.Extensions.Options;

    [ApiController]
    [Route("api/[controller]/[action]")]
    [BearerAuthorize(Roles.PortalSuperAdmin, Roles.WinlossAnalysis)]
    [RequestValidation(validateCurrencyList: true)]
    [AutoBindingMetaModel(bindAgent: true)]
    public class WinlossAnalysisController : ControllerBase
    {
        private const string FILE_NAME_BOTH_GROUP_BY_AND_BREAK_DOWN = "WinLossReportGroupByAndBreakdownBy";
        private const string FILE_NAME_GROUP_BY_ONLY = "WinLossReportGroupBy";

        private readonly IWinlossAnalysisService _service;
        private readonly IFileService _fileService;
        private readonly IWinlossAnalysisExcel _excelService;
        private readonly IFileProvider _fileProvider;
        private readonly IContextService _contextService;
        private readonly ICompressService _compressService;
        private readonly ExcelOptions _option;

        public WinlossAnalysisController(IWinlossAnalysisService service,
           IWinlossAnalysisExcel excelService,
           IFileService fileService,
           IFileProvider fileProvider,
           IContextService contextService,
           ICompressService compressService,
           IOptions<ExcelOptions> options)
        {
            _service = service;
            _excelService = excelService;
            _fileService = fileService;
            _fileProvider = fileProvider;
            _contextService = contextService;
            _compressService = compressService;
            _option = options.Value;
        }

        [HttpPost]
        public async Task<IActionResult> GetData([FromBody] WinlossAnalysisMetaModel metaModel)
        {
            metaModel.SpecialPlatform = _contextService.SpecialPlatform;
            return Ok(await _service.GetDataAsync(metaModel));
        }

        [HttpPost]
        public async Task<IActionResult> ExportExcel([FromBody] WinlossAnalysisMetaModel metaModel)
        {
            const string contentType = "application/octet-stream";
            metaModel.SpecialPlatform = _contextService.SpecialPlatform;
            AssignParamToExportExcel(metaModel);
            var data = await _service.GetDataAsync(metaModel);
            var excelOption = GenOptionFromParam(metaModel);
            var pagingDatas = PagingData(data, metaModel.ExportExcelMode, _option.MaximumRowPerFile);
            if (pagingDatas.Count() > 1)
            {
                var zipFilePath = ExportToZipFile(pagingDatas, excelOption);
                var stream = new FileStream(zipFilePath, FileMode.Open, FileAccess.Read, FileShare.None, 4096, FileOptions.DeleteOnClose);
                return File(stream, "application/zip");
            }
            return File(ExportToExcelFile(data, excelOption), contentType);
        }

        private static void AssignParamToExportExcel(WinlossAnalysisMetaModel metaModel)
        {
            metaModel.PageSize = 0;
            metaModel.PageIndex = 0;
        }

        private IEnumerable<WinlossAnalysisResultModel> PagingData(WinlossAnalysisResultModel resultData, WinlossAnalysisExportMode exportMode, int maxRow)
        {
            var listPagingData = new List<WinlossAnalysisResultModel>();
            var pagingData = new WinlossAnalysisResultModel
            {
                SumTotal = resultData.SumTotal,
                TotalRow = resultData.TotalRow
            };
            var mainData = new List<TreeViewItem<WinlossAnalysisViewModel, WinlossAnalysisViewModel>>();
            pagingData.Data = mainData;
            var totalRow = 0;
            var lastItem = resultData.Data.LastOrDefault();
            foreach (var item in resultData.Data)
            {
                mainData.Add(item);
                var childernRowCount = exportMode == WinlossAnalysisExportMode.All ? item.Children.Count() : 0;
                totalRow = totalRow + 1 + childernRowCount;
                if (item.KeyItem == lastItem?.KeyItem || totalRow >= maxRow)
                {
                    listPagingData.Add(pagingData);
                    pagingData = new WinlossAnalysisResultModel
                    {
                        SumTotal = resultData.SumTotal,
                        TotalRow = resultData.TotalRow
                    };
                    mainData = new List<TreeViewItem<WinlossAnalysisViewModel, WinlossAnalysisViewModel>>();
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
    }
}