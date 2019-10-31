namespace Fanex.Athena.Report.BySite.Services
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.IO.Compression;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    using Fanex.Athena.Report.BySite.DTOs;
    using Fanex.Athena.Report.BySite.MetaModels;
    using Fanex.Athena.Report.BySite.Repositories.Interfaces;
    using Fanex.Athena.Report.BySite.Services.Interfaces;
    using Fanex.Athena.Report.Common.Model.ViewModel;
    using Fanex.Athena.Report.Core.DTO;
    using Fanex.Athena.Report.Resources;

    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Options;

    public class DailyBetsService : IDailyBetsService
    {
        private readonly IDailyBetsRepository _repository;

        private readonly ExcelOptions _excelOptions;
        private readonly IHostingEnvironment _enviroment;
        private const string DecimalFormat = "0.00";

        private const string DefaultTextDelimiter = "\"";

        public DailyBetsService(IDailyBetsRepository dailyBetsRepository,
            IOptions<ExcelOptions> excelOptionsAccessor,
            IHostingEnvironment enviroment)
        {
            _repository = dailyBetsRepository;
            _excelOptions = excelOptionsAccessor.Value;
            _enviroment = enviroment;
        }

        public async Task<ResultModel<AuditTicket>> GetDataAsync(DailyBetsMetaModel metaModel)
        {
            var result = await _repository.GetDataAsync(metaModel).ConfigureAwait(false);

            var productCode = GetProductName(metaModel.ProductCodes);
            var statusName = GetStatusName();

            foreach (var item in result.DataList)
            {
                item.ProductName = productCode[item.ProductCode];
                item.Status = statusName[item.StatusId];
            }

            return result;
        }

        public async Task<string> Export(DailyBetsMetaModel metaModel)
        {
            var files = new List<Tuple<int, string>>();

            metaModel.PageIndex = 1;
            metaModel.PageSize = _excelOptions.MaximumRowPerFile;

            var productCodes = GetProductName(metaModel.ProductCodes);
            var statusNames = GetStatusName();

            var result = await _repository.GetDataAsync(metaModel).ConfigureAwait(false);
            var firstFile = WriteFile(result, statusNames, productCodes, metaModel.BaseCurrency);
            files.Add(new Tuple<int, string>(metaModel.PageIndex, firstFile));

            var totalPage = (result.TotalRows - 1) / _excelOptions.MaximumRowPerFile + 1;
            var nextPage = metaModel.PageIndex + 1;
            var exportTasks = new List<Task<Tuple<int, string>>>();
            while (nextPage <= totalPage)
            {
                var pageMetaModel = metaModel.ShallowCopy();
                pageMetaModel.PageIndex = nextPage;
                exportTasks.Add(ExportOnePage(pageMetaModel, statusNames, productCodes));
                ++nextPage;
            }

            var fileNames = await Task.WhenAll(exportTasks).ConfigureAwait(false);
            files.AddRange(fileNames);

            return ZipExcelFile(files);
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

        private async Task<Tuple<int, string>> ExportOnePage(
            DailyBetsMetaModel metaModel,
            IDictionary<int, string> statusNames,
            IDictionary<string, string> productNames)
        {
            var result = await _repository.GetDataAsync(metaModel).ConfigureAwait(false);
            var excelFile = WriteFile(result, statusNames, productNames, metaModel.BaseCurrency);

            result.DataList = null;
            return new Tuple<int, string>(metaModel.PageIndex, excelFile);
        }

        private string WriteFile(
            ResultModel<AuditTicket> data,
            IDictionary<int, string> statusNames,
            IDictionary<string, string> productNames,
            string baseCurrency)
        {
            var fileName = Path.Combine(_enviroment.ContentRootPath, _excelOptions.Path, $"{Guid.NewGuid()}.dat");

            using (StreamWriter stw = new StreamWriter(fileName, false, Encoding.UTF8))
            {
                AddHeader(stw);
                AddBody(stw, data.DataList, statusNames, productNames);
                AddFooter(stw, data.TotalData, baseCurrency);
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

        private void AddHeader(StreamWriter streamWriter)
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

            streamWriter.WriteLine(string.Join(_excelOptions.CSVSeparateCharacter, header));
        }

        private void AddFooter(StreamWriter streamWriter, AuditTicket data, string baseCurrency)
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
                data.Turnover.ToString(DecimalFormat),
                data.WinLoss.ToString(DecimalFormat),
                string.Empty
            };

            streamWriter.WriteLine(string.Join(_excelOptions.CSVSeparateCharacter, footer));
        }

        private void AddBody(
            StreamWriter streamWriter,
            IEnumerable<AuditTicket> data,
            IDictionary<int, string> statusNames,
            IDictionary<string, string> productNames)
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
                rowBuilder.Append(productNames[item.ProductCode]);
                rowBuilder.Append(_excelOptions.CSVSeparateCharacter);
                rowBuilder.Append(item.BetType);
                rowBuilder.Append(_excelOptions.CSVSeparateCharacter);
                rowBuilder.Append(item.Turnover.ToString(DecimalFormat));
                rowBuilder.Append(_excelOptions.CSVSeparateCharacter);
                rowBuilder.Append(item.WinLoss.ToString(DecimalFormat));
                rowBuilder.Append(_excelOptions.CSVSeparateCharacter);
                rowBuilder.Append(statusNames[item.StatusId]);

                streamWriter.WriteLine(rowBuilder.ToString());
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