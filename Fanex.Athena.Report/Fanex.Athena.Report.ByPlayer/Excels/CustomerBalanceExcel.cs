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
    using Fanex.Athena.Report.Common.Services;
    using Fanex.Athena.Report.Resources;
    using Fanex.Data;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Options;

    public class CustomerBalanceExcel : BaseExcel, ICustomerBalanceExcel
    {
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

        private static readonly string[] _commonHeaderText = {
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
                        WriteHeaderToCSV(metaModel, stream, rowBuilder);

                        var index = 1;
                        while (reader.Read())
                        {
                            WriteContentRowToCSV(stream, reader, rowBuilder, metaModel, index);
                            index++;
                        }

                        WriteFooterToCSV(reader, metaModel, stream, rowBuilder);
                    }
                }
            }

            return fileName;
        }

        private void WriteHeaderToCSV(CustomerBalanceMetaModel metaModel, StreamWriter stream, StringBuilder rowBuilder)
        {
            rowBuilder.Append(string.Join(ExcelOptions.CSVSeparateCharacter, _commonHeaderText));
            rowBuilder.Append(ExcelOptions.CSVSeparateCharacter);

            if (metaModel.HasComm)
            {
                rowBuilder.Append($"{ViewResources.CustomerW_L} {ViewResources.Comm}").Append(ExcelOptions.CSVSeparateCharacter);
                rowBuilder.Append($"{ViewResources.CustomerW_L} {ViewResources.Total}").Append(ExcelOptions.CSVSeparateCharacter);
            }

            rowBuilder.Append(ViewResources.ClosingBalance).Append(ExcelOptions.CSVSeparateCharacter);
            rowBuilder.Append(ViewResources.AffiliateTransfer);

            if (metaModel.IsAllowedBA)
            {
                rowBuilder.Append(ExcelOptions.CSVSeparateCharacter);
                rowBuilder.Append($"{ViewResources.BA} {ViewResources.Turnover}").Append(ExcelOptions.CSVSeparateCharacter);
                rowBuilder.Append(ViewResources.BACommission).Append(ExcelOptions.CSVSeparateCharacter);
                rowBuilder.Append(ViewResources.AmtDueToBA);
            }

            stream.WriteLine(rowBuilder);
            rowBuilder.Clear();
        }

        private void WriteContentRowToCSV(
            StreamWriter stream,
            IDataReader reader,
            StringBuilder rowBuilder,
            CustomerBalanceMetaModel metaModel,
            int index)
        {
            rowBuilder.Append(index).Append(ExcelOptions.CSVSeparateCharacter);
            rowBuilder.Append(reader["nickname"]).Append(ExcelOptions.CSVSeparateCharacter);
            rowBuilder.Append(reader["username"]).Append(ExcelOptions.CSVSeparateCharacter);
            BuildCommonColumn(reader, metaModel, string.Empty, rowBuilder);

            stream.WriteLine(rowBuilder);
            rowBuilder.Clear();
        }

        private void BuildCommonColumn(
            IDataReader reader,
            CustomerBalanceMetaModel metaModel,
            string suffixName,
            StringBuilder rowBuilder)
        {
            foreach (var item in _commonColumns)
            {
                switch (item)
                {
                    case "netturnover":
                        {
                            rowBuilder.Append(Convert.ToDecimal(reader[suffixName + "turnover"]) - Convert.ToDecimal(reader[suffixName + "drawgame"])).Append(ExcelOptions.CSVSeparateCharacter);

                            break;
                        }

                    case "winlosscommtotal":
                        {
                            if (metaModel.HasComm)
                            {
                                rowBuilder.Append(Convert.ToDecimal(reader[suffixName + "playercomm"])).Append(ExcelOptions.CSVSeparateCharacter);
                                rowBuilder.Append(Convert.ToDecimal(reader[suffixName + "winloss"])
                                    + Convert.ToDecimal(reader[suffixName + "playercomm"])).Append(ExcelOptions.CSVSeparateCharacter);
                            }

                            break;
                        }

                    case "affiliatetransfer":
                        {
                            rowBuilder.Append(reader[suffixName + item]);
                            break;
                        }

                    case "ba":
                        {
                            if (metaModel.IsAllowedBA)
                            {
                                BuildBAColumn(reader, suffixName == "sum", rowBuilder);
                            }

                            break;
                        }

                    default:
                        {
                            rowBuilder.Append(reader[suffixName + item]).Append(ExcelOptions.CSVSeparateCharacter);
                            break;
                        }
                }
            }
        }

        private void WriteFooterToCSV(
        IDataReader reader,
        CustomerBalanceMetaModel metaModel,
        StreamWriter stream, StringBuilder rowBuilder)
        {
            if (reader.NextResult() && reader.Read())
            {
                if (Convert.ToInt32(reader["TotalRows"]) == 0)
                {
                    stream.WriteLine(ViewResources.NoInfo);
                }
                else
                {
                    rowBuilder.Append(string.Empty).Append(ExcelOptions.CSVSeparateCharacter);
                    rowBuilder.Append(string.Empty).Append(ExcelOptions.CSVSeparateCharacter);
                    rowBuilder.Append(ViewResources.Total).Append(ExcelOptions.CSVSeparateCharacter);
                    BuildCommonColumn(reader, metaModel, "sum", rowBuilder);
                    stream.WriteLine(rowBuilder);
                    rowBuilder.Clear();
                }
            }
        }

        private void BuildBAColumn(
            IDataReader reader,
            bool isSum,
            StringBuilder rowBuilder)
        {
            rowBuilder.Append(ExcelOptions.CSVSeparateCharacter);
            rowBuilder.Append(isSum ? Convert.ToDecimal(reader["sumturnoverba"]) : Convert.ToDecimal(reader["TurnOverBA"])).Append(ExcelOptions.CSVSeparateCharacter);
            rowBuilder.Append(isSum ? Convert.ToDecimal(reader["sumcommba"]) : Convert.ToDecimal(reader["CommBA"])).Append(ExcelOptions.CSVSeparateCharacter);
            rowBuilder.Append(isSum ? Convert.ToDecimal(reader["sumamountduetoba"]) : Convert.ToDecimal(reader["AmountDueToBA"]));
        }
    }
}