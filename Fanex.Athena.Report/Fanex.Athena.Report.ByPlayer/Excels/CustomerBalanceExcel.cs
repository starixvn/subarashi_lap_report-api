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
            const string UspName = "Lrf_Rpt_CustomerBalance";
            var fileName = Path.Combine(EnsureFolderPath(), $"{Guid.NewGuid()}.dat");

            using (var db = ObjectDbFactory.CreateInstance(UspName))
            {
                metaModel.InjectToDbParameter(db);
                using (var reader = await db.ExecuteReaderAsync().ConfigureAwait(false))
                {
                    using (StreamWriter stream = _fileService.CreateStreamWriter(fileName, false, Encoding.UTF8))
                    {
                        WriteHeaderToCSV(metaModel, stream);

                        var index = 1;
                        while (reader.Read())
                        {
                            WriteDataRowToCSV(stream, reader, metaModel, index);
                            index++;
                        }

                        WriteFooterToCSV(reader, metaModel, stream);
                    }
                }
            }

            return fileName;
        }

        private void WriteHeaderToCSV(CustomerBalanceMetaModel metaModel, StreamWriter stream)
        {
            var rowBuilder = new StringBuilder();
            rowBuilder.Append("#").Append(_excelOptions.CSVSeparateCharacter);
            rowBuilder.Append(ViewResources.ExternalId).Append(_excelOptions.CSVSeparateCharacter);
            rowBuilder.Append(ViewResources.SystemId).Append(_excelOptions.CSVSeparateCharacter);
            rowBuilder.Append(ViewResources.OpeningBalance).Append(_excelOptions.CSVSeparateCharacter);
            rowBuilder.Append(ViewResources.Deposit).Append(_excelOptions.CSVSeparateCharacter);
            rowBuilder.Append(ViewResources.Withdrawal).Append(_excelOptions.CSVSeparateCharacter);
            rowBuilder.Append(ViewResources.Bonus).Append(_excelOptions.CSVSeparateCharacter);
            rowBuilder.Append(ViewResources.Adjustment).Append(_excelOptions.CSVSeparateCharacter);
            rowBuilder.Append(ViewResources.Turnover).Append(_excelOptions.CSVSeparateCharacter);
            rowBuilder.Append(ViewResources.DrawGame).Append(_excelOptions.CSVSeparateCharacter);
            rowBuilder.Append(ViewResources.NetTurnover).Append(_excelOptions.CSVSeparateCharacter);
            rowBuilder.Append(ViewResources.PendingFunds).Append(_excelOptions.CSVSeparateCharacter);
            rowBuilder.Append(ViewResources.CustomerW_L).Append(_excelOptions.CSVSeparateCharacter);

            if (metaModel.HasComm)
            {
                rowBuilder.Append($"{ViewResources.CustomerW_L} {ViewResources.Comm}").Append(_excelOptions.CSVSeparateCharacter);
                rowBuilder.Append($"{ViewResources.CustomerW_L} {ViewResources.Total}").Append(_excelOptions.CSVSeparateCharacter);
            }

            rowBuilder.Append(ViewResources.ClosingBalance).Append(_excelOptions.CSVSeparateCharacter);
            rowBuilder.Append(ViewResources.AffiliateTransfer).Append(_excelOptions.CSVSeparateCharacter);

            if (metaModel.IsAllowedBA)
            {
                rowBuilder.Append($"{ViewResources.BA} {ViewResources.Turnover}").Append(_excelOptions.CSVSeparateCharacter);
                rowBuilder.Append(ViewResources.BACommission).Append(_excelOptions.CSVSeparateCharacter);
                rowBuilder.Append(ViewResources.AmtDueToBA).Append(_excelOptions.CSVSeparateCharacter);
            }

            stream.WriteLine(rowBuilder);
            rowBuilder.Clear();
        }

        private void WriteDataRowToCSV(
            StreamWriter stream,
            IDataReader reader,
            CustomerBalanceMetaModel metaModel,
            int index)
        {
            var rowBuilder = new StringBuilder();
            rowBuilder.Append(index).Append(_excelOptions.CSVSeparateCharacter);
            rowBuilder.Append(reader["nickname"]).Append(_excelOptions.CSVSeparateCharacter);
            rowBuilder.Append(reader["username"]).Append(_excelOptions.CSVSeparateCharacter);
            rowBuilder.Append(reader["openingbalance"]).Append(_excelOptions.CSVSeparateCharacter);
            rowBuilder.Append(reader["deposit"]).Append(_excelOptions.CSVSeparateCharacter);
            rowBuilder.Append(reader["withdrawal"]).Append(_excelOptions.CSVSeparateCharacter);
            rowBuilder.Append(reader["bonus"]).Append(_excelOptions.CSVSeparateCharacter);
            rowBuilder.Append(reader["adjustment"]).Append(_excelOptions.CSVSeparateCharacter);
            rowBuilder.Append(reader["turnover"]).Append(_excelOptions.CSVSeparateCharacter);
            rowBuilder.Append(reader["drawgame"]).Append(_excelOptions.CSVSeparateCharacter);
            rowBuilder.Append(Convert.ToDecimal(reader["turnover"]) - Convert.ToDecimal(reader["drawgame"])).Append(_excelOptions.CSVSeparateCharacter);

            rowBuilder.Append(reader["outstanding"]).Append(_excelOptions.CSVSeparateCharacter);

            rowBuilder.Append(reader["winloss"]).Append(_excelOptions.CSVSeparateCharacter);

            if (metaModel.HasComm)
            {
                rowBuilder.Append(reader["playercomm"]).Append(_excelOptions.CSVSeparateCharacter);
                rowBuilder.Append(Convert.ToDecimal(reader["winloss"]) + Convert.ToDecimal(reader["playercomm"])).Append(_excelOptions.CSVSeparateCharacter);
            }

            rowBuilder.Append(reader["closingbalance"]).Append(_excelOptions.CSVSeparateCharacter);

            rowBuilder.Append(reader["affiliatetransfer"]).Append(_excelOptions.CSVSeparateCharacter);

            if (metaModel.IsAllowedBA)
            {
                rowBuilder.Append(reader["TurnOverBA"]).Append(_excelOptions.CSVSeparateCharacter);

                rowBuilder.Append(reader["CommBA"]).Append(_excelOptions.CSVSeparateCharacter);

                rowBuilder.Append(reader["AmountDueToBA"]).Append(_excelOptions.CSVSeparateCharacter);
            }

            stream.WriteLine(rowBuilder);
            rowBuilder.Clear();
        }

        private void WriteFooterToCSV(
            IDataReader reader,
            CustomerBalanceMetaModel metaModel,
            StreamWriter stream)
        {
            if (reader.NextResult() && reader.Read())
            {
                if (Convert.ToInt32(reader["TotalRows"]) == 0)
                {
                    stream.WriteLine("No information is available");
                }
                else
                {
                    var rowBuilder = new StringBuilder();
                    rowBuilder.Append(string.Empty).Append(_excelOptions.CSVSeparateCharacter);
                    rowBuilder.Append(string.Empty).Append(_excelOptions.CSVSeparateCharacter);
                    rowBuilder.Append(ViewResources.Total).Append(_excelOptions.CSVSeparateCharacter);
                    rowBuilder.Append(reader["sumopeningbalance"]).Append(_excelOptions.CSVSeparateCharacter);
                    rowBuilder.Append(reader["sumdeposit"]).Append(_excelOptions.CSVSeparateCharacter);
                    rowBuilder.Append(reader["sumwithdrawal"]).Append(_excelOptions.CSVSeparateCharacter);
                    rowBuilder.Append(reader["sumbonus"]).Append(_excelOptions.CSVSeparateCharacter);
                    rowBuilder.Append(reader["sumadjustment"]).Append(_excelOptions.CSVSeparateCharacter);
                    rowBuilder.Append(reader["sumturnover"]).Append(_excelOptions.CSVSeparateCharacter);
                    rowBuilder.Append(reader["sumdrawgame"]).Append(_excelOptions.CSVSeparateCharacter);
                    rowBuilder.Append(Convert.ToDecimal(reader["sumturnover"]) - Convert.ToDecimal(reader["sumdrawgame"])).Append(_excelOptions.CSVSeparateCharacter);
                    rowBuilder.Append(reader["sumoutstanding"]).Append(_excelOptions.CSVSeparateCharacter);
                    rowBuilder.Append(reader["sumwinloss"]).Append(_excelOptions.CSVSeparateCharacter);

                    if (metaModel.HasComm)
                    {
                        rowBuilder.Append(reader["sumplayercomm"]).Append(_excelOptions.CSVSeparateCharacter);
                        rowBuilder.Append(Convert.ToDecimal(reader["sumwinloss"]) - Convert.ToDecimal(reader["sumplayercomm"])).Append(_excelOptions.CSVSeparateCharacter);
                    }

                    rowBuilder.Append(reader["sumclosingbalance"]).Append(_excelOptions.CSVSeparateCharacter);
                    rowBuilder.Append(reader["sumaffiliatetransfer"]).Append(_excelOptions.CSVSeparateCharacter);

                    if (metaModel.IsAllowedBA)
                    {
                        rowBuilder.Append(reader["sumturnoverba"]).Append(_excelOptions.CSVSeparateCharacter);
                        rowBuilder.Append(reader["sumcommba"]).Append(_excelOptions.CSVSeparateCharacter);
                        rowBuilder.Append(reader["sumamountduetoba"]).Append(_excelOptions.CSVSeparateCharacter);
                    }

                    stream.WriteLine(rowBuilder);
                    rowBuilder.Clear();
                }
            }
        }

        private string EnsureFolderPath()
        {
            var path = GetTempFolderPath();
            _fileService.CreateFolder(path);
            return path;
        }

        private string GetTempFolderPath()
        {
            var root = _fileService.GetParentDirectory(_enviroment.ContentRootPath);

            return Path.Combine(root.FullName, _excelOptions.Path);
        }
    }
}