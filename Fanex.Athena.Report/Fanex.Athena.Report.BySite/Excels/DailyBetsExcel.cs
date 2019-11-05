namespace Fanex.Athena.Report.BySite.Excels
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.IO;
    using System.Text;
    using System.Threading.Tasks;
    using Fanex.Athena.Report.BySite.Excels.Interfaces;
    using Fanex.Athena.Report.BySite.Extensions;
    using Fanex.Athena.Report.BySite.MetaModels;
    using Fanex.Athena.Report.BySite.Repositories.Interfaces;
    using Fanex.Athena.Report.Common.Model.MetaModels;
    using Fanex.Athena.Report.Common.Services;
    using Fanex.Athena.Report.Core.DTO;
    using Fanex.Athena.Report.Resources;
    using Fanex.Data;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Options;

    public class DailyBetsExcel : IDailyBetsExcel
    {
        private readonly ExcelOptions _excelOptions;

        private readonly IHostingEnvironment _enviroment;

        private readonly IDailyBetsRepository _repository;

        private readonly IFileService _fileService;

        private const string DefaultTextDelimiter = "\"";

        private const string DecimalFormat = "0.00";

        private const string UspName = "Lrf_Rpt_Audit_DailyBets";

        private string _header;

        private string _footer;

        public DailyBetsExcel(
            IDailyBetsRepository dailyBetsRepository,
            IFileService fileService,
            IOptions<ExcelOptions> excelOptionsAccessor,
            IHostingEnvironment enviroment)
        {
            _repository = dailyBetsRepository;
            _fileService = fileService;
            _excelOptions = excelOptionsAccessor.Value;
            _enviroment = enviroment;
        }

        public async Task<string> Export(DailyBetsMetaModel metaModel)
        {
            var files = new List<ZipEntry>();

            metaModel.PageIndex = 1;
            metaModel.PageSize = _excelOptions.MaximumRowPerFile;

            var (totalRows, totalData) = await _repository.GetTotalDataAsync(metaModel).ConfigureAwait(false);
            if (totalRows == 0)
            {
                return null;
            }

            CreateTempFolder();
            _header = GetHeaderString();
            _footer = GetFooterString(totalData.Turnover, totalData.WinLoss, metaModel.BaseCurrency);

            var productNames = DailyBetsMetaModelExtension.GetProductName(metaModel.ProductCodeList);
            var statusNames = DailyBetsMetaModelExtension.GetStatusName();
            var totalPage = (totalRows - 1) / _excelOptions.MaximumRowPerFile + 1;
            var nextPage = metaModel.PageIndex;
            var exportTasks = new List<Task<ZipEntry>>();
            while (nextPage <= totalPage)
            {
                var pageMetaModel = metaModel.ShallowCopy();
                pageMetaModel.PageIndex = nextPage;
                exportTasks.Add(ExportOnePageAsync(pageMetaModel, statusNames, productNames));
                ++nextPage;
            }

            var fileNames = await Task.WhenAll(exportTasks).ConfigureAwait(false);
            files.AddRange(fileNames);

            return ZipExcelFile(files);
        }

        private async Task<ZipEntry> ExportOnePageAsync(
            DailyBetsMetaModel metaModel,
            IDictionary<int, string> statusNames,
            IDictionary<string, string> productNames)
        {
            using (var db = ObjectDbFactory.CreateInstance(UspName))
            {
                metaModel.InjectToDbParameter(db);
                using (var reader = await db.ExecuteReaderAsync().ConfigureAwait(false))
                {
                    var fileName = Path.Combine(_enviroment.ContentRootPath, _excelOptions.Path, $"{Guid.NewGuid()}.dat");
                    using (StreamWriter stw = _fileService.CreateStreamWriter(fileName, false, Encoding.UTF8))
                    {
                        await stw.WriteLineAsync(_header).ConfigureAwait(false);

                        var rowBuilder = new StringBuilder();
                        while (reader.Read())
                        {
                            await WriteMainDataItemAsync(reader, stw, rowBuilder, statusNames, productNames).ConfigureAwait(false);
                        }

                        await stw.WriteLineAsync(_footer).ConfigureAwait(false);
                    }

                    return new ZipEntry
                    {
                        Path = fileName,
                        FileName = $"DailyBets_{metaModel.PageIndex}.csv"
                    };
                }
            }
        }

        private async Task WriteMainDataItemAsync(
            IDataReader reader,
            StreamWriter streamWriter,
            StringBuilder rowBuilder,
            IDictionary<int, string> statusNames,
            IDictionary<string, string> productNames)
        {
            rowBuilder.Append(reader["Index"]);
            rowBuilder.Append(_excelOptions.CSVSeparateCharacter);

            rowBuilder.Append(reader["PlayerName"]);
            rowBuilder.Append(_excelOptions.CSVSeparateCharacter);

            rowBuilder.Append(reader["CurrencyName"]);
            rowBuilder.Append(_excelOptions.CSVSeparateCharacter);

            rowBuilder.Append(_excelOptions.CSVTextDelimiter);
            AddBigNumberAsText(rowBuilder, reader["RefNo"].ToString());
            rowBuilder.Append(_excelOptions.CSVTextDelimiter);
            rowBuilder.Append(_excelOptions.CSVSeparateCharacter);

            rowBuilder.Append(reader["TransTime"]);
            rowBuilder.Append(_excelOptions.CSVSeparateCharacter);

            rowBuilder.Append(productNames[reader["ProductCode"].ToString()]);
            rowBuilder.Append(_excelOptions.CSVSeparateCharacter);

            rowBuilder.Append(_excelOptions.CSVTextDelimiter);
            rowBuilder.Append(reader["BetType"]);
            rowBuilder.Append(_excelOptions.CSVTextDelimiter);
            rowBuilder.Append(_excelOptions.CSVSeparateCharacter);

            rowBuilder.Append(_excelOptions.CSVTextDelimiter);
            rowBuilder.Append(reader["Turnover"]);
            rowBuilder.Append(_excelOptions.CSVTextDelimiter);
            rowBuilder.Append(_excelOptions.CSVSeparateCharacter);

            rowBuilder.Append(_excelOptions.CSVTextDelimiter);
            rowBuilder.Append(reader["WinLoss"]);
            rowBuilder.Append(_excelOptions.CSVTextDelimiter);
            rowBuilder.Append(_excelOptions.CSVSeparateCharacter);

            rowBuilder.Append(statusNames[Convert.ToInt16(reader["Status"])]);

            await streamWriter.WriteLineAsync(rowBuilder.ToString()).ConfigureAwait(false);
            rowBuilder.Clear();
        }

        private void AddBigNumberAsText(StringBuilder rowBuilder, string bigNumber)
        {
            rowBuilder.Append("=TEXT(" + bigNumber + ",");
            rowBuilder.Append(_excelOptions.CSVTextDelimiter == DefaultTextDelimiter ? DefaultTextDelimiter + "\"0\"" + DefaultTextDelimiter + ")" : "\"0\")");
        }

        private string GetHeaderString()
        {
            var headers = new[]
            {
                "#",
                ViewResources.PlayerName,
                ViewResources.Currency,
                ViewResources.RefNo,
                ViewResources.TransTime,
                ViewResources.Product,
                ViewResources.BetType,
                ViewResources.Turnover,
                ViewResources.WinLoss,
                ViewResources.Status
            };

            return string.Join(_excelOptions.CSVSeparateCharacter, headers);
        }

        private string GetFooterString(decimal turnover, decimal winLoss, string baseCurrency)
        {
            var footer = new[]
            {
                string.Empty,
                string.Empty,
                string.Empty,
                string.Empty,
                string.Empty,
                string.Empty,
                string.Format(ViewResources.Totalin, baseCurrency),
                _excelOptions.CSVTextDelimiter + turnover.ToString(DecimalFormat) + _excelOptions.CSVTextDelimiter,
                 _excelOptions.CSVTextDelimiter + winLoss.ToString(DecimalFormat) + _excelOptions.CSVTextDelimiter,
                string.Empty
            };

            return string.Join(_excelOptions.CSVSeparateCharacter, footer);
        }

        private void CreateTempFolder()
        {
            var folderPath = Path.Combine(_enviroment.ContentRootPath, _excelOptions.Path);

            _fileService.CreateFolder(folderPath);
        }

        private string ZipExcelFile(List<ZipEntry> files)
        {
            var zipPath = Path.Combine(_enviroment.ContentRootPath, _excelOptions.Path, $"{Guid.NewGuid()}.zip");

            _fileService.Compress(files, zipPath, true);

            return zipPath;
        }
    }
}