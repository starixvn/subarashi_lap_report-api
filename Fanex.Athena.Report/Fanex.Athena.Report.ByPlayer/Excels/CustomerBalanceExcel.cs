namespace Fanex.Athena.Report.ByPlayer.Excels
{
    using System;
    using System.Data;
    using System.IO;
    using System.Text;
    using System.Threading.Tasks;
    using Fanex.Athena.Report.ByPlayer.Excels.Interfaces;
    using Fanex.Athena.Report.ByPlayer.Extensions;
    using Fanex.Athena.Report.ByPlayer.MetaModels;
    using Fanex.Athena.Report.Common.Services;
    using Fanex.Athena.Report.Core.DTO;
    using Fanex.Athena.Report.Resources;
    using Fanex.Data;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Options;

    public class CustomerBalanceExcel : ICustomerBalanceExcel
    {
        private readonly ExcelOptions _excelOptions;

        private readonly IHostingEnvironment _enviroment;

        private readonly IFileService _fileService;

        private const string UspName = "Lrf_Rpt_CustomerBalance";

        private string _footer;

        private string _tempFolderPath;

        public CustomerBalanceExcel(
            IFileService fileService,
            IOptions<ExcelOptions> excelOptionsAccessor,
            IHostingEnvironment enviroment)
        {
            _fileService = fileService;
            _excelOptions = excelOptionsAccessor.Value;
            _enviroment = enviroment;
        }

        public async Task<string> Export(CustomerBalanceMetaModel metaModel)
        {
            CreateTempFolder();
            var _header = GetHeaderString();

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
                        var index = ((metaModel.PageIndex - 1) * metaModel.PageSize) + 1;

                        while (reader.Read())
                        {
                            rowBuilder.Append(index);
                            rowBuilder.Append(_excelOptions.CSVSeparateCharacter);
                            await WriteMainDataItemAsync(reader, stw, rowBuilder).ConfigureAwait(false);
                            index++;
                        }

                        if (index > 1 && reader.NextResult() && reader.Read())
                        {
                            _footer = GetFooterString(reader);
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
                ViewResources.Withdrawal,
                ViewResources.Bonus,
                ViewResources.Adjustment,
                ViewResources.Turnover,
                ViewResources.DrawGame,
                ViewResources.NetTurnover,
                ViewResources.PendingFunds,
                ViewResources.CustomerW_L,
                $"{ViewResources.CustomerW_L} {ViewResources.Comm}",
                $"{ViewResources.CustomerW_L} {ViewResources.Total}",
                ViewResources.ClosingBalance,
                ViewResources.AffiliateTransfer,
                $"{ViewResources.BA} {ViewResources.Turnover}",
                ViewResources.BACommission,
                ViewResources.AmtDueToBA
            };

            return string.Join(_excelOptions.CSVSeparateCharacter, headers);
        }

        private string GetFooterString(IDataReader reader)
        {
            var footer = new[]
            {
                string.Empty,
                string.Empty,
                ViewResources.Total,
                reader["sumopeningbalance"],
                reader["sumdeposit"],
                reader["sumwithdrawal"],
                reader["sumbonus"],
                reader["sumadjustment"],
                reader["sumturnover"],
                reader["sumdrawgame"],
                Convert.ToDecimal(reader["sumturnover"]) - Convert.ToDecimal(reader["sumdrawgame"]),
                reader["sumoutstanding"],
                reader["sumwinloss"],
                reader["sumplayercomm"],
                Convert.ToDecimal(reader["sumwinloss"]) + Convert.ToDecimal(reader["sumplayercomm"]),
                reader["sumclosingbalance"],
                reader["sumaffiliatetransfer"],
                reader["sumturnoverba"],
                reader["sumcommba"],
                reader["sumamountduetoba"]
            };

            return string.Join(_excelOptions.CSVSeparateCharacter, footer);
        }

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