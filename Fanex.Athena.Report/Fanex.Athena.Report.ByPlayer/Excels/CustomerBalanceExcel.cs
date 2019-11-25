namespace Fanex.Athena.Report.ByPlayer.Excels
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.IO;
    using System.Text;
    using System.Threading.Tasks;
    using Fanex.Athena.Report.ByPlayer.Excels.Interfaces;
    using Fanex.Athena.Report.ByPlayer.Extensions;
    using Fanex.Athena.Report.ByPlayer.MetaModels;
    using Fanex.Athena.Report.ByPlayer.Repositories.Interfaces;
    using Fanex.Athena.Report.Common.Model.MetaModels;
    using Fanex.Athena.Report.Common.Services;
    using Fanex.Athena.Report.Core.DTO;
    using Fanex.Athena.Report.Resources;
    using Fanex.Data;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Options;
    using Fanex.Athena.Report.ByPlayer.Service.Interfaces;

    public class CustomerBalanceExcel : ICustomerBalanceExcel
    {
        private readonly ExcelOptions _excelOptions;

        private readonly IHostingEnvironment _enviroment;

        private readonly ICustomerBalanceService _service;

        private readonly IFileService _fileService;

        private const string DefaultTextDelimiter = "\"";

        private const string DecimalFormat = "0.00";

        private const string UspName = "Lrf_Rpt_CustomerBalance";

        private string _header;

        private string _footer;

        private string _tempFolderPath;

        public CustomerBalanceExcel(
            ICustomerBalanceService customerBalanceService,
            IFileService fileService,
            IOptions<ExcelOptions> excelOptionsAccessor,
            IHostingEnvironment enviroment)
        {
            _service = customerBalanceService;
            _fileService = fileService;
            _excelOptions = excelOptionsAccessor.Value;
            _enviroment = enviroment;
        }

        public async Task<string> Export(CustomerBalanceMetaModel metaModel)
        {
            CreateTempFolder();
            _header = GetHeaderString();

            using (var db = ObjectDbFactory.CreateInstance(UspName))
            {
                metaModel.InjectToDbParameter(db);
                using (var reader = await db.ExecuteReaderAsync().ConfigureAwait(false))
                {
                    var fileName = Path.Combine(_tempFolderPath, $"{Guid.NewGuid()}.csv");
                    using (StreamWriter stw = _fileService.CreateStreamWriter(fileName, false, Encoding.UTF8))
                    {
                        await stw.WriteLineAsync(_header).ConfigureAwait(false);

                        var rowBuilder = new StringBuilder();
                        var index = 1;

                        while (reader.Read())
                        {
                            rowBuilder.Append(index);
                            rowBuilder.Append(_excelOptions.CSVSeparateCharacter);
                            await WriteMainDataItemAsync(reader, stw, rowBuilder).ConfigureAwait(false);
                            index++;
                        }

                        await stw.WriteLineAsync(_footer).ConfigureAwait(false);
                    }

                    return fileName;
                }
            }
        }

        private async Task WriteMainDataItemAsync(
            IDataReader reader,
            StreamWriter streamWriter,
            StringBuilder rowBuilder)
        {
            rowBuilder.Append(reader["nickname"]);
            rowBuilder.Append(_excelOptions.CSVSeparateCharacter);

            rowBuilder.Append(reader["username"]);
            rowBuilder.Append(_excelOptions.CSVSeparateCharacter);

            rowBuilder.Append(reader["openingbalance"]);
            rowBuilder.Append(_excelOptions.CSVSeparateCharacter);

            rowBuilder.Append(reader["deposit"]);
            rowBuilder.Append(_excelOptions.CSVSeparateCharacter);

            rowBuilder.Append(reader["withdrawal"]);
            rowBuilder.Append(_excelOptions.CSVSeparateCharacter);

            rowBuilder.Append(reader["bonus"]);
            rowBuilder.Append(_excelOptions.CSVSeparateCharacter);

            rowBuilder.Append(reader["adjustment"]);
            rowBuilder.Append(_excelOptions.CSVSeparateCharacter);

            rowBuilder.Append(reader["turnover"]);
            rowBuilder.Append(_excelOptions.CSVSeparateCharacter);

            rowBuilder.Append(reader["drawgame"]);
            rowBuilder.Append(_excelOptions.CSVSeparateCharacter);

            rowBuilder.Append(Convert.ToDecimal(reader["turnover"]) - Convert.ToDecimal(reader["drawgame"]));
            rowBuilder.Append(_excelOptions.CSVSeparateCharacter);

            rowBuilder.Append(reader["outstanding"]);
            rowBuilder.Append(_excelOptions.CSVSeparateCharacter);

            rowBuilder.Append(reader["winloss"]);
            rowBuilder.Append(_excelOptions.CSVSeparateCharacter);

            rowBuilder.Append(reader["playercomm"]);
            rowBuilder.Append(_excelOptions.CSVSeparateCharacter);

            rowBuilder.Append(Convert.ToDecimal(reader["winloss"]) + Convert.ToDecimal(reader["playercomm"]));
            rowBuilder.Append(_excelOptions.CSVSeparateCharacter);

            rowBuilder.Append(reader["affiliatetransfer"]);
            rowBuilder.Append(_excelOptions.CSVSeparateCharacter);

            rowBuilder.Append(reader["closingbalance"]);
            rowBuilder.Append(_excelOptions.CSVSeparateCharacter);

            rowBuilder.Append(reader["TurnOverBA"]);
            rowBuilder.Append(_excelOptions.CSVSeparateCharacter);

            rowBuilder.Append(reader["CommBA"]);
            rowBuilder.Append(_excelOptions.CSVSeparateCharacter);

            rowBuilder.Append(reader["AmountDueToBA"]);
            rowBuilder.Append(_excelOptions.CSVSeparateCharacter);

            await streamWriter.WriteLineAsync(rowBuilder.ToString()).ConfigureAwait(false);
            rowBuilder.Clear();
        }

        private string GetHeaderString()
        {
            var headers = new[]
            {
                "#",
                ViewResources.ExternalId,
                ViewResources.SystemId,
                ViewResources.OpeningBalance,
                ViewResources.Deposit,
                ViewResources.WithDrawal,
                ViewResources.Bonus,
                ViewResources.Adjustment,
                ViewResources.Turnover,
                ViewResources.DrawGame,
                ViewResources.NetTurnover,
                ViewResources.PendingFunds,
                $"{ViewResources.Win} {ViewResources.Loss}",
                ViewResources.Comm,
                ViewResources.Total,
                ViewResources.ClosingBalance,
                ViewResources.AffiliateTransfer,
                ViewResources.Turnover,
                ViewResources.Comm,
                ViewResources.AmountDueTo
            };

            return string.Join(_excelOptions.CSVSeparateCharacter, headers);
        }

        //private string GetFooterString(decimal turnover, decimal winLoss, string baseCurrency)
        //{
        //    var footer = new[]
        //    {
        //        string.Empty,
        //        string.Empty,
        //        string.Empty,
        //        string.Empty,
        //        string.Empty,
        //        string.Empty,
        //        string.Format(ViewResources.Totalin, baseCurrency),
        //        _excelOptions.CSVTextDelimiter + turnover.ToString(DecimalFormat) + _excelOptions.CSVTextDelimiter,
        //         _excelOptions.CSVTextDelimiter + winLoss.ToString(DecimalFormat) + _excelOptions.CSVTextDelimiter,
        //        string.Empty
        //    };

        //    return string.Join(_excelOptions.CSVSeparateCharacter, footer);
        //}

        private void CreateTempFolder()
        {
            _tempFolderPath = GetTempFolderPath();
            _fileService.CreateFolder(_tempFolderPath);
        }

        private string GetTempFolderPath()
        {
            var root = _fileService.GetParentDirectory(_enviroment.ContentRootPath);

            return Path.Combine(root.FullName, _excelOptions.Path);
        }
    }
}
