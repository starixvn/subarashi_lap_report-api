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
    using Fanex.Athena.Report.Common.Excels;
    using Fanex.Athena.Report.Common.Extensions;
    using Fanex.Athena.Report.Common.Services;
    using Fanex.Athena.Report.Resources;
    using Fanex.Data;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Options;

    public class CustomerBalanceExcel : BaseExcel, ICustomerBalanceExcel
    {
        private const int DecimalDigit = 2;

        private static readonly string[] _commonColumns = {
            "openingbalance",
            "deposit",
            "withdrawal",
            "bonus",
            "adjustment",
            "promotion",
            "turnover",
            "drawgame",
            "netturnover",
            "outstanding",
            "winloss",
            "winlosscommtotal",
            "closingbalance",
            "affiliatetransfer",
            "ba"
        };

        private readonly string[] _commonHeaderText = {
                "#",
                ViewResources.ExternalId,
                ViewResources.SystemId,
                ViewResources.OpeningBalance,
                ViewResources.Deposit,
                ViewResources.Withdrawal,
                ViewResources.Bonus,
                ViewResources.Adjustment,
                ViewResources.Promotion,
                ViewResources.Turnover,
                ViewResources.DrawGame,
                ViewResources.NetTurnover,
                ViewResources.PendingFunds,
                ViewResources.CustomerW_L
            };

        public CustomerBalanceExcel(
            IFileService fileService,
            IOptions<ExcelOptions> excelOptionsAccessor,
            IHostingEnvironment environment) : base(environment, fileService, excelOptionsAccessor)
        {
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
                    using (StreamWriter stream = FileService.CreateStreamWriter(fileName, false, Encoding.UTF8))
                    {
                        var rowBuilder = new StringBuilder();
                        await WriteHeaderToCSV(metaModel, stream, rowBuilder).ConfigureAwait(false);

                        var index = 1;
                        while (reader.Read())
                        {
                            await WriteContentRowToCSV(stream, reader, rowBuilder, metaModel, index).ConfigureAwait(false);
                            index++;
                        }

                        await WriteFooterToCSV(reader, metaModel, stream, rowBuilder).ConfigureAwait(false);
                    }
                }
            }

            return fileName;
        }

        private async Task WriteHeaderToCSV(CustomerBalanceMetaModel metaModel, StreamWriter stream, StringBuilder rowBuilder)
        {
            rowBuilder.Append(string.Join(ExcelOptions.CSVSeparateCharacter, _commonHeaderText)).Append(ExcelOptions.CSVSeparateCharacter);

            if (metaModel.HasComm)
            {
                rowBuilder.Append($"{ViewResources.CustomerW_L} {ViewResources.Comm}").Append(ExcelOptions.CSVSeparateCharacter)
                    .Append($"{ViewResources.CustomerW_L} {ViewResources.Total}").Append(ExcelOptions.CSVSeparateCharacter);
            }

            rowBuilder.Append(ViewResources.ClosingBalance).Append(ExcelOptions.CSVSeparateCharacter)
                .Append(ViewResources.AffiliateTransfer);

            if (metaModel.IsAllowedBA)
            {
                rowBuilder.Append(ExcelOptions.CSVSeparateCharacter)
                    .Append($"{ViewResources.BA} {ViewResources.Turnover}").Append(ExcelOptions.CSVSeparateCharacter)
                    .Append(ViewResources.BACommission).Append(ExcelOptions.CSVSeparateCharacter)
                    .Append(ViewResources.AmtDueToBA);
            }

            await stream.WriteLineAsync(rowBuilder.ToString()).ConfigureAwait(false);
            rowBuilder.Clear();
        }

        private async Task WriteContentRowToCSV(
            StreamWriter stream,
            IDataReader reader,
            StringBuilder rowBuilder,
            CustomerBalanceMetaModel metaModel,
            int index)
        {
            rowBuilder.Append(index).Append(ExcelOptions.CSVSeparateCharacter);
            BuildItemAsText(rowBuilder, reader.GetVal<string>("nickname"));
            rowBuilder.Append(reader["username"]).Append(ExcelOptions.CSVSeparateCharacter);

            BuildCommonColumn(reader, metaModel, string.Empty, rowBuilder);

            await stream.WriteLineAsync(rowBuilder.ToString()).ConfigureAwait(false);
            rowBuilder.Clear();
        }

        private void BuildCommonColumn(
            IDataReader reader,
            CustomerBalanceMetaModel metaModel,
            string prefixName,
            StringBuilder rowBuilder)
        {
            foreach (var item in _commonColumns)
            {
                switch (item)
                {
                    case "netturnover":
                        {
                            var netTurnover = reader.GetVal<decimal>(prefixName + "turnover") - reader.GetVal<decimal>(prefixName + "drawgame");
                            BuildItemWithTextDelimiter(rowBuilder, decimal.Round(netTurnover, DecimalDigit));
                            break;
                        }

                    case "winlosscommtotal":
                        {
                            if (metaModel.HasComm)
                            {
                                BuildItemWithTextDelimiter(rowBuilder, decimal.Round(reader.GetVal<decimal>(prefixName + "playercomm"), DecimalDigit));

                                var totalWinLoss = reader.GetVal<decimal>(prefixName + "winloss") + reader.GetVal<decimal>(prefixName + "playercomm");
                                BuildItemWithTextDelimiter(rowBuilder, decimal.Round(totalWinLoss, DecimalDigit));
                            }

                            break;
                        }

                    case "ba":
                        {
                            if (metaModel.IsAllowedBA)
                            {
                                BuildBAColumn(reader, prefixName == "sum", rowBuilder);
                            }

                            break;
                        }

                    default:
                        {
                            BuildItemWithTextDelimiter(rowBuilder, decimal.Round(reader.GetVal<decimal>(prefixName + item), DecimalDigit));
                            break;
                        }
                }
            }
        }

        private async Task WriteFooterToCSV(
            IDataReader reader,
            CustomerBalanceMetaModel metaModel,
            StreamWriter stream, StringBuilder rowBuilder)
        {
            if (reader.NextResult() && reader.Read())
            {
                if (reader.GetVal<int>("TotalRows") == 0)
                {
                    await stream.WriteLineAsync(ViewResources.NoInfo).ConfigureAwait(false);
                }
                else
                {
                    rowBuilder.Append(string.Empty).Append(ExcelOptions.CSVSeparateCharacter)
                        .Append(string.Empty).Append(ExcelOptions.CSVSeparateCharacter)
                        .Append(ViewResources.Total).Append(ExcelOptions.CSVSeparateCharacter);
                    BuildCommonColumn(reader, metaModel, "sum", rowBuilder);
                    await stream.WriteLineAsync(rowBuilder.ToString()).ConfigureAwait(false);
                    rowBuilder.Clear();
                }
            }
        }

        private void BuildBAColumn(
            IDataReader reader,
            bool isSum,
            StringBuilder rowBuilder)
        {
            var columnNameList = new[] {
                isSum ? "sumturnoverba" : "TurnOverBA",
                isSum ? "sumcommba" : "CommBA",
                isSum ? "sumamountduetoba" : "AmountDueToBA"
            };

            foreach (var columnName in columnNameList)
            {
                BuildItemWithTextDelimiter(rowBuilder, decimal.Round(reader.GetVal<decimal>(columnName), DecimalDigit));
            }
        }
    }
}