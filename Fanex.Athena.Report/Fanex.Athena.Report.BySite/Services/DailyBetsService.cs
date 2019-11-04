namespace Fanex.Athena.Report.BySite.Services
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.IO;
    using System.IO.Compression;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Fanex.Athena.Report.BySite.DTOs;
    using Fanex.Athena.Report.BySite.Extensions;
    using Fanex.Athena.Report.BySite.MetaModels;
    using Fanex.Athena.Report.BySite.Services.Interfaces;
    using Fanex.Athena.Report.Common.Model.ViewModel;
    using Fanex.Athena.Report.Core.DTO;
    using Fanex.Athena.Report.Resources;
    using Fanex.Data;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Options;

    public class DailyBetsService : IDailyBetsService
    {
        private readonly ExcelOptions _excelOptions;

        private readonly IHostingEnvironment _enviroment;

        private const string DefaultTextDelimiter = "\"";

        private const string DecimalFormat = "0.00";

        private const string UspName = "Lrf_Rpt_Audit_DailyBets";

        public DailyBetsService(
            IOptions<ExcelOptions> excelOptionsAccessor,
            IHostingEnvironment enviroment)
        {
            _excelOptions = excelOptionsAccessor.Value;
            _enviroment = enviroment;
        }

        public async Task<ResultModel<AuditTicket>> GetDataAsync(DailyBetsMetaModel metaModel)
        {
            var productNames = GetProductName(metaModel.ProductCodeList);
            var statusNames = GetStatusName();

            return await GetDataFromDBAsync(metaModel, statusNames, productNames);
        }

        public async Task<string> Export(DailyBetsMetaModel metaModel)
        {
            var files = new List<Tuple<int, string>>();

            metaModel.PageIndex = 1;
            metaModel.PageSize = _excelOptions.MaximumRowPerFile;

            var productNames = GetProductName(metaModel.ProductCodeList);
            var statusNames = GetStatusName();

            CreateTempFolder();
            var result = await ExportOnePageAsync(metaModel, statusNames, productNames, true).ConfigureAwait(false);
            if (result.Item1 == 0)
            {
                return null;
            }
  
            files.Add(new Tuple<int, string>(metaModel.PageIndex, result.Item2));

            var totalPage = (result.Item1 - 1) / _excelOptions.MaximumRowPerFile + 1;
            var nextPage = metaModel.PageIndex + 1;
            var exportTasks = new List<Task<Tuple<int, string>>>();
            while (nextPage <= totalPage)
            {
                var pageMetaModel = metaModel.ShallowCopy();
                pageMetaModel.PageIndex = nextPage;
                exportTasks.Add(ExportOnePageAsync(pageMetaModel, statusNames, productNames));
                ++nextPage;
            }

            var fileNames = await Task.WhenAll(exportTasks).ConfigureAwait(false);
            files.AddRange(fileNames.Where(x => x != null));
            return ZipExcelFile(files);
        }

        private static async Task<ResultModel<AuditTicket>> GetDataFromDBAsync(
            DailyBetsMetaModel metaModel,
            IDictionary<int, string> statusNames,
            IDictionary<string, string> productNames)
        {
            var result = new ResultModel<AuditTicket>();

            using (var db = ObjectDbFactory.CreateInstance(UspName))
            {
                UpdateUspParams(db, metaModel);
                using (var reader = await db.ExecuteReaderAsync().ConfigureAwait(false))
                {
                    if (reader.Read())
                    {
                        result.TotalRows = Convert.ToInt32(reader["TotalRows"]);
                        result.TotalData = new AuditTicket
                        {
                            Turnover = Convert.ToDecimal(reader["TotalTurnover"]),
                            WinLoss = Convert.ToDecimal(reader["TotalWinLoss"])
                        };

                        result.DataList = ReadMainData(reader, statusNames, productNames);
                    }              
                }
            }

            return result;
        }

        private static IEnumerable<AuditTicket> ReadMainData(
            IDataReader reader,
            IDictionary<int, string> statusNames,
            IDictionary<string, string> productNames)
        {
            var result = new List<AuditTicket>();
            result.Add(ReadMainDataItem(reader, statusNames, productNames));

            while (reader.Read())
            {
                result.Add(ReadMainDataItem(reader, statusNames, productNames));
            }

            return result;
        }

        private static AuditTicket ReadMainDataItem(
            IDataReader reader,
            IDictionary<int, string> statusNames,
            IDictionary<string, string> productNames)
        {
            return new AuditTicket
            {
                Index = Convert.ToInt32(reader["Index"]),
                PlayerName = reader["PlayerName"].ToString(),
                CurrencyName = reader["CurrencyName"].ToString(),
                RefNo = Convert.ToInt64(reader["RefNo"]),
                TransTime = Convert.ToDateTime(reader["TransTime"]),
                ProductName = productNames[reader["ProductCode"].ToString()],
                BetType = reader["BetType"].ToString(),
                Turnover = Convert.ToDecimal(reader["Turnover"]),
                WinLoss = Convert.ToDecimal(reader["WinLoss"]),
                Status = statusNames[Convert.ToInt16(reader["Status"])]
            };
        }

        private static void UpdateUspParams(IObjectDb db, DailyBetsMetaModel metaModel)
        {
            db.SetParameterValue("SiteName", metaModel.SiteName);
            db.SetParameterValue("Date", metaModel.Date);
            db.SetParameterValue("ProductCodes", metaModel.ProductCodeList);
            db.SetParameterValue("CurrencyList", metaModel.CurrencyListToXml());
            db.SetParameterValue("BaseCurrency", metaModel.BaseCurrency);
            db.SetParameterValue("PageIndex", metaModel.PageIndex);
            db.SetParameterValue("PageSize", metaModel.PageSize);

            if (!string.IsNullOrWhiteSpace(metaModel.SortBy) && !string.IsNullOrWhiteSpace(metaModel.SortOrder))
            {
                db.SetParameterValue("SortBy", metaModel.SortBy);
                db.SetParameterValue("SortOrder", metaModel.SortOrder);
            }
        }

        private void CreateTempFolder()
        {
            var folderPath = Path.Combine(_enviroment.ContentRootPath, _excelOptions.Path);

            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }
        }

        private string ZipExcelFile(List<Tuple<int, string>> files)
        {
            var zipPath = Path.Combine(_enviroment.ContentRootPath, _excelOptions.Path, $"{Guid.NewGuid()}.zip");

            using (ZipArchive archive = ZipFile.Open(zipPath, ZipArchiveMode.Create))
            {
                foreach (var file in files)
                {
                    archive.CreateEntryFromFile(file.Item2, $"DailyBets_{file.Item1}.csv");
                    File.Delete(file.Item2);
                }
            }

            return zipPath;
        }

        public async Task<Tuple<int, string>> ExportOnePageAsync(
            DailyBetsMetaModel metaModel,
            IDictionary<int, string> statusNames,
            IDictionary<string, string> productNames,
            bool isFristPage = false)
        {
            using (var db = ObjectDbFactory.CreateInstance(UspName))
            {
                UpdateUspParams(db, metaModel);
                using (var reader = await db.ExecuteReaderAsync().ConfigureAwait(false))
                {
                    if (reader.Read())
                    {
                        var totalRow = Convert.ToInt32(reader["TotalRows"]);
                        var totalTurnover = Convert.ToDecimal(reader["TotalTurnover"]);
                        var totalWinLoss = Convert.ToDecimal(reader["TotalWinLoss"]);

                        var fileName = Path.Combine(_enviroment.ContentRootPath, _excelOptions.Path, $"{Guid.NewGuid()}.dat");
                        using (StreamWriter stw = new StreamWriter(fileName, false, Encoding.UTF8))
                        {
                            await AddHeaderAsync(stw).ConfigureAwait(false);
                            await WriteMainDataAsync(reader, stw, statusNames, productNames).ConfigureAwait(false);
                            await AddFooterAsync(stw, totalTurnover, totalWinLoss, metaModel.BaseCurrency).ConfigureAwait(false);
                        }

                        return isFristPage ? new Tuple<int, string>(totalRow, fileName) : new Tuple<int, string>(metaModel.PageIndex, fileName);
                    }

                    return new Tuple<int, string>(0, null);
                }
            }
        }

        private async Task WriteMainDataAsync(
            IDataReader reader,
            StreamWriter streamWriter,
            IDictionary<int, string> statusNames,
            IDictionary<string, string> productNames)
        {
            var rowBuilder = new StringBuilder();
            await WriteMainDataItemAsync(reader, streamWriter, rowBuilder, statusNames, productNames).ConfigureAwait(false);

            while (reader.Read())
            {
                await WriteMainDataItemAsync(reader, streamWriter, rowBuilder, statusNames, productNames).ConfigureAwait(false);
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

        private static IDictionary<string, string> GetProductName(string productCodeString)
        {
            var productCodes = productCodeString.Split(",");

            return productCodes.ToDictionary(key => key, value => ViewResources.ResourceManager.GetString(value));
        }

        private static IDictionary<int, string> GetStatusName()
        {
            return new Dictionary<int, string>
            {
                { 0, ViewResources.Draw },
                { 1, ViewResources.Won },
                { -1, ViewResources.Lose }
            };
        }

        private async Task AddHeaderAsync(StreamWriter streamWriter)
        {
            await streamWriter.WriteLineAsync(
                "#"
                + _excelOptions.CSVSeparateCharacter
                + ViewResources.PlayerName
                + _excelOptions.CSVSeparateCharacter
                + ViewResources.Currency
                + _excelOptions.CSVSeparateCharacter
                + ViewResources.RefNo
                + _excelOptions.CSVSeparateCharacter
                + ViewResources.TransTime
                + _excelOptions.CSVSeparateCharacter
                + ViewResources.Product
                + _excelOptions.CSVSeparateCharacter
                + ViewResources.BetType
                + _excelOptions.CSVSeparateCharacter
                + ViewResources.Turnover
                + _excelOptions.CSVSeparateCharacter
                + ViewResources.WinLoss
                + _excelOptions.CSVSeparateCharacter
                + ViewResources.Status).ConfigureAwait(false);
        }

        private async Task AddFooterAsync(StreamWriter streamWriter, decimal turnover, decimal winLoss, string baseCurrency)
        {
            await streamWriter.WriteLineAsync(
                _excelOptions.CSVSeparateCharacter
                + _excelOptions.CSVSeparateCharacter
                + _excelOptions.CSVSeparateCharacter
                + _excelOptions.CSVSeparateCharacter
                + _excelOptions.CSVSeparateCharacter
                + _excelOptions.CSVSeparateCharacter
                + string.Format(ViewResources.Totalin, baseCurrency)
                + _excelOptions.CSVSeparateCharacter
                + _excelOptions.CSVTextDelimiter + turnover.ToString(DecimalFormat) + _excelOptions.CSVTextDelimiter
                + _excelOptions.CSVSeparateCharacter
                + _excelOptions.CSVTextDelimiter + winLoss.ToString(DecimalFormat) + _excelOptions.CSVTextDelimiter
                + _excelOptions.CSVSeparateCharacter).ConfigureAwait(false);
        }

        private void AddBigNumberAsText(StringBuilder rowBuilder, string bigNumber)
        {
            rowBuilder.Append("=TEXT(" + bigNumber);
            const string TextFormular = "=TEXT({0},\"{1}\")";

            rowBuilder.Append(_excelOptions.CSVTextDelimiter == DefaultTextDelimiter ? ",\"0\")" : "," + DefaultTextDelimiter + "\"0\"" + DefaultTextDelimiter + ")");
        }
    }
}