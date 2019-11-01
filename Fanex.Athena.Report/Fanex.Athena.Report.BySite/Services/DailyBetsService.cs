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
        private const string DecimalFormat = "0.00";

        private const string DefaultTextDelimiter = "\"";

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

            var result = await GetDataFromDBAsync(metaModel, statusNames, productNames).ConfigureAwait(false);
            if (result.TotalRows == 0)
            {
                return null;
            }

            CreateTempFolder();
            var firstFile = await WriteFileAsync(result, metaModel.BaseCurrency);
            files.Add(new Tuple<int, string>(metaModel.PageIndex, firstFile));

            var totalPage = (result.TotalRows - 1) / _excelOptions.MaximumRowPerFile + 1;
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
            files.AddRange(fileNames);
            result.DataList = Enumerable.Empty<AuditTicket>();
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
                    result.DataList = ReadMainData(reader, statusNames, productNames);

                    if (reader.NextResult() && reader.Read())
                    {
                        result.TotalData = ReadTotalData(reader);
                        result.TotalRows = Convert.ToInt32(reader["TotalRows"]);
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

        private static AuditTicket ReadTotalData(IDataReader reader)
        {
            return new AuditTicket
            {
                Turnover = Convert.ToDecimal(reader["Turnover"]),
                WinLoss = Convert.ToDecimal(reader["WinLoss"])
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
            IDictionary<string, string> productNames)
        {
            var fileName = Path.Combine(_enviroment.ContentRootPath, _excelOptions.Path, $"{Guid.NewGuid()}.dat");

            using (var db = ObjectDbFactory.CreateInstance(UspName))
            {
                UpdateUspParams(db, metaModel);
                using (var reader = await db.ExecuteReaderAsync().ConfigureAwait(false))
                {
                    using (StreamWriter stw = new StreamWriter(fileName, false, Encoding.UTF8))
                    {
                        await AddHeaderAsync(stw).ConfigureAwait(false);
                        await WriteMainData(reader, stw, statusNames, productNames).ConfigureAwait(false);

                        if (reader.NextResult() && reader.Read())
                        {
                            await AddFooterAsync(stw, ReadTotalData(reader), metaModel.BaseCurrency);
                        }
                    }
                }
            }

            return new Tuple<int, string>(metaModel.PageIndex, fileName);
        }

        private async Task WriteMainData(
            IDataReader reader,
            StreamWriter streamWriter,
            IDictionary<int, string> statusNames,
            IDictionary<string, string> productNames)
        {
            while (reader.Read())
            {
                var rowBuilder = new StringBuilder();
                rowBuilder.Append(Convert.ToInt32(reader["Index"]));
                rowBuilder.Append(_excelOptions.CSVSeparateCharacter);
                rowBuilder.Append(reader["PlayerName"].ToString());
                rowBuilder.Append(_excelOptions.CSVSeparateCharacter);
                rowBuilder.Append(reader["CurrencyName"].ToString());
                rowBuilder.Append(_excelOptions.CSVSeparateCharacter);
                rowBuilder.Append(ConvertBigNumberToText(Convert.ToInt64(reader["RefNo"])));
                rowBuilder.Append(_excelOptions.CSVSeparateCharacter);
                rowBuilder.Append(Convert.ToDateTime(reader["TransTime"]));
                rowBuilder.Append(_excelOptions.CSVSeparateCharacter);
                rowBuilder.Append(productNames[reader["ProductCode"].ToString()]);
                rowBuilder.Append(_excelOptions.CSVSeparateCharacter);
                rowBuilder.Append(QuoteText(reader["BetType"].ToString()));
                rowBuilder.Append(_excelOptions.CSVSeparateCharacter);
                rowBuilder.Append(QuoteText(Convert.ToDecimal(reader["Turnover"]).ToString(DecimalFormat)));
                rowBuilder.Append(_excelOptions.CSVSeparateCharacter);
                rowBuilder.Append(QuoteText(Convert.ToDecimal(reader["WinLoss"]).ToString(DecimalFormat)));
                rowBuilder.Append(_excelOptions.CSVSeparateCharacter);
                rowBuilder.Append(statusNames[Convert.ToInt16(reader["Status"])]);

                await streamWriter.WriteLineAsync(rowBuilder.ToString()).ConfigureAwait(false);
                rowBuilder.Clear();
            }
        }

        private async Task<string> WriteFileAsync(
            ResultModel<AuditTicket> data,
            string baseCurrency)
        {
            var fileName = Path.Combine(_enviroment.ContentRootPath, _excelOptions.Path, $"{Guid.NewGuid()}.dat");

            using (StreamWriter stw = new StreamWriter(fileName, false, Encoding.UTF8))
            {
                await AddHeaderAsync(stw).ConfigureAwait(false);
                await AddBodyAsync(stw, data.DataList);
                await AddFooterAsync(stw, data.TotalData, baseCurrency);
            }

            return fileName;
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
            var header = new[]
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

            await streamWriter.WriteLineAsync(string.Join(_excelOptions.CSVSeparateCharacter, header)).ConfigureAwait(false);
        }

        private async Task AddFooterAsync(StreamWriter streamWriter, AuditTicket data, string baseCurrency)
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
                QuoteText(data.Turnover.ToString(DecimalFormat)),
                QuoteText(data.WinLoss.ToString(DecimalFormat)),
                string.Empty
            };

            await streamWriter.WriteLineAsync(string.Join(_excelOptions.CSVSeparateCharacter, footer));
        }

        private async Task AddBodyAsync(
            StreamWriter streamWriter,
            IEnumerable<AuditTicket> data)
        {
            var totalRows = data.Count();
            var rowBuilder = new StringBuilder();
            for (int i = 0; i < totalRows; i++)
            {
                var item = data.ElementAt(i);

                rowBuilder.Append(item.Index);
                rowBuilder.Append(_excelOptions.CSVSeparateCharacter);
                rowBuilder.Append(item.PlayerName);
                rowBuilder.Append(_excelOptions.CSVSeparateCharacter);
                rowBuilder.Append(item.CurrencyName);
                rowBuilder.Append(_excelOptions.CSVSeparateCharacter);
                rowBuilder.Append(ConvertBigNumberToText(item.RefNo));
                rowBuilder.Append(_excelOptions.CSVSeparateCharacter);
                rowBuilder.Append(item.TransTimeString);
                rowBuilder.Append(_excelOptions.CSVSeparateCharacter);
                rowBuilder.Append(item.ProductCode);
                rowBuilder.Append(_excelOptions.CSVSeparateCharacter);
                rowBuilder.Append(QuoteText(item.BetType));
                rowBuilder.Append(_excelOptions.CSVSeparateCharacter);
                rowBuilder.Append(QuoteText(item.Turnover.ToString(DecimalFormat)));
                rowBuilder.Append(_excelOptions.CSVSeparateCharacter);
                rowBuilder.Append(QuoteText(item.WinLoss.ToString(DecimalFormat)));
                rowBuilder.Append(_excelOptions.CSVSeparateCharacter);
                rowBuilder.Append(item.Status);
                await streamWriter.WriteLineAsync(rowBuilder.ToString()).ConfigureAwait(false);
                rowBuilder.Clear();
            }
        }

        private string ConvertBigNumberToText(long number)
        {
            const string TextFormular = "=TEXT({0},\"{1}\")";

            var formatString = _excelOptions.CSVTextDelimiter == DefaultTextDelimiter
                ? $"{DefaultTextDelimiter}0{DefaultTextDelimiter}"
                : "0";

            return QuoteText(string.Format(TextFormular, number, formatString));
        }

        private string QuoteText(string text) => $"{_excelOptions.CSVTextDelimiter}{text}{_excelOptions.CSVTextDelimiter}";

    }
}