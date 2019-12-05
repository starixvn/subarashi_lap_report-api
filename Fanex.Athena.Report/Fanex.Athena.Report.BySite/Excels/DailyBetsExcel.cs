namespace Fanex.Athena.Report.BySite.Excels
{
    using Fanex.Athena.Report.BySite.Excels.Interfaces;
    using Fanex.Athena.Report.BySite.Extensions;
    using Fanex.Athena.Report.BySite.MetaModels;
    using Fanex.Athena.Report.BySite.Repositories.Interfaces;
    using Fanex.Athena.Report.Common.Excels;
    using Fanex.Athena.Report.Common.Model.MetaModels;
    using Fanex.Athena.Report.Common.Services;
    using Fanex.Athena.Report.Resources;
    using Fanex.Data;

    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Options;

    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.IO;
    using System.Text;
    using System.Threading.Tasks;

    public class DailyBetsExcel : BaseExcel, IDailyBetsExcel
    {
        private readonly IDailyBetsRepository _repository;

        private const string DefaultTextDelimiter = "\"";

        private const string DecimalFormat = "0.00";

        private const string UspName = "Lrf_Rpt_Audit_DailyBets";

        private string _header;

        private string _footer;

        private string _tempFolderPath;

        public DailyBetsExcel(
            IDailyBetsRepository dailyBetsRepository,
            IFileService fileService,
            IOptions<ExcelOptions> excelOptionsAccessor,
            IHostingEnvironment enviroment)
            : base(enviroment, fileService, excelOptionsAccessor)
        {
            _repository = dailyBetsRepository;
        }

        public async Task<string> Export(DailyBetsMetaModel metaModel)
        {
            var files = new List<ZipEntry>();

            metaModel.PageIndex = 1;
            metaModel.PageSize = ExcelOptions.MaximumRowPerFile;

            var (totalRows, totalData) = await _repository.GetTotalDataAsync(metaModel).ConfigureAwait(false);
            if (totalRows == 0)
            {
                return null;
            }

            _tempFolderPath = EnsureFolderPath();
            _header = GetHeaderString();
            _footer = GetFooterString(totalData.Turnover, totalData.WinLoss, metaModel.BaseCurrency);

            var totalPage = (totalRows - 1) / ExcelOptions.MaximumRowPerFile + 1;
            var nextPage = metaModel.PageIndex;
            var exportTasks = new List<Task<ZipEntry>>();
            while (nextPage <= totalPage)
            {
                var pageMetaModel = metaModel.ShallowCopy();
                pageMetaModel.PageIndex = nextPage;
                exportTasks.Add(ExportOnePageAsync(pageMetaModel));
                ++nextPage;
            }

            var fileNames = await Task.WhenAll(exportTasks).ConfigureAwait(false);
            files.AddRange(fileNames);

            return ZipExcelFile(files, _tempFolderPath);
        }

        private async Task<ZipEntry> ExportOnePageAsync(DailyBetsMetaModel metaModel)
        {
            using (var db = ObjectDbFactory.CreateInstance(UspName))
            {
                metaModel.InjectToDbParameter(db);
                using (var reader = await db.ExecuteReaderAsync().ConfigureAwait(false))
                {
                    var fileName = Path.Combine(_tempFolderPath, $"{Guid.NewGuid()}.dat");
                    using (StreamWriter stw = FileService.CreateStreamWriter(fileName, false, Encoding.UTF8))
                    {
                        await stw.WriteLineAsync(_header).ConfigureAwait(false);

                        var rowBuilder = new StringBuilder();
                        while (reader.Read())
                        {
                            await WriteMainDataItemAsync(reader, stw, rowBuilder).ConfigureAwait(false);
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
            StringBuilder rowBuilder)
        {
            rowBuilder.Append(reader["Index"]);
            rowBuilder.Append(ExcelOptions.CSVSeparateCharacter);

            rowBuilder.Append(reader["PlayerName"]);
            rowBuilder.Append(ExcelOptions.CSVSeparateCharacter);

            rowBuilder.Append(reader["CurrencyName"]);
            rowBuilder.Append(ExcelOptions.CSVSeparateCharacter);

            rowBuilder.Append(ExcelOptions.CSVTextDelimiter);
            AddBigNumberAsText(rowBuilder, reader["RefNo"].ToString());
            rowBuilder.Append(ExcelOptions.CSVTextDelimiter);
            rowBuilder.Append(ExcelOptions.CSVSeparateCharacter);

            rowBuilder.Append(reader["TransTime"]);
            rowBuilder.Append(ExcelOptions.CSVSeparateCharacter);

            rowBuilder.Append(DailyBetsMetaModelExtension.GetProductName[reader["ProductCode"].ToString()]());
            rowBuilder.Append(ExcelOptions.CSVSeparateCharacter);

            rowBuilder.Append(ExcelOptions.CSVTextDelimiter);
            rowBuilder.Append(reader["BetType"]);
            rowBuilder.Append(ExcelOptions.CSVTextDelimiter);
            rowBuilder.Append(ExcelOptions.CSVSeparateCharacter);

            rowBuilder.Append(ExcelOptions.CSVTextDelimiter);
            rowBuilder.Append(reader["Turnover"]);
            rowBuilder.Append(ExcelOptions.CSVTextDelimiter);
            rowBuilder.Append(ExcelOptions.CSVSeparateCharacter);

            rowBuilder.Append(ExcelOptions.CSVTextDelimiter);
            rowBuilder.Append(reader["WinLoss"]);
            rowBuilder.Append(ExcelOptions.CSVTextDelimiter);
            rowBuilder.Append(ExcelOptions.CSVSeparateCharacter);

            rowBuilder.Append(DailyBetsMetaModelExtension.GetStatusName[Convert.ToInt16(reader["Status"])]());

            await streamWriter.WriteLineAsync(rowBuilder.ToString()).ConfigureAwait(false);
            rowBuilder.Clear();
        }

        private void AddBigNumberAsText(StringBuilder rowBuilder, string bigNumber)
        {
            rowBuilder.Append("=TEXT(" + bigNumber + ",");
            rowBuilder.Append(ExcelOptions.CSVTextDelimiter == DefaultTextDelimiter ? DefaultTextDelimiter + "\"0\"" + DefaultTextDelimiter + ")" : "\"0\")");
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

            return string.Join(ExcelOptions.CSVSeparateCharacter, headers);
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
                ExcelOptions.CSVTextDelimiter + turnover.ToString(DecimalFormat) + ExcelOptions.CSVTextDelimiter,
                 ExcelOptions.CSVTextDelimiter + winLoss.ToString(DecimalFormat) + ExcelOptions.CSVTextDelimiter,
                string.Empty
            };

            return string.Join(ExcelOptions.CSVSeparateCharacter, footer);
        }

    }
}