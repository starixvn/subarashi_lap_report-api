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

        private static readonly string[] _commonColumns = {
            "openingbalance",
            "deposit",
            "withdrawal",
            "bonus",
            "adjustment",
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

        private void WriteContentRowToCSV(
            StreamWriter stream,
            IDataReader reader,
            StringBuilder rowBuilder,
            CustomerBalanceMetaModel metaModel,
            int index)
        {
            rowBuilder.Append(index).Append(_excelOptions.CSVSeparateCharacter);
            rowBuilder.Append(reader["nickname"]).Append(_excelOptions.CSVSeparateCharacter);
            rowBuilder.Append(reader["username"]).Append(_excelOptions.CSVSeparateCharacter);
            BuildCommoncolumn(reader, metaModel, string.Empty, rowBuilder);
            stream.WriteLine(rowBuilder);
            rowBuilder.Clear();
        }

        private void BuildCommoncolumn(IDataReader reader, CustomerBalanceMetaModel metaModel, string suffixName,
            StringBuilder rowBuilder)
        {
            foreach (var item in _commonColumns)
            {
                switch (item)
                {
                    case "netturnover":
                        {
                            rowBuilder.Append(Convert.ToDecimal(reader[suffixName + "turnover"]) - Convert.ToDecimal(reader[suffixName + "drawgame"]))
                      .Append(_excelOptions.CSVSeparateCharacter);

                            break;
                        }

                    case "winlosscommtotal":
                        {
                            if (metaModel.HasComm)
                            {
                                rowBuilder.Append(Convert.ToDecimal(reader[suffixName + "playercomm"])).Append(_excelOptions.CSVSeparateCharacter);
                                rowBuilder.Append(Convert.ToDecimal(reader[suffixName + "winloss"])
                                    + Convert.ToDecimal(reader[suffixName + "playercomm"])).Append(_excelOptions.CSVSeparateCharacter);
                            }

                            break;
                        }

                    case "ba":
                        if (metaModel.IsAllowedBA)
                        {
                            rowBuilder.Append(reader[suffixName + "TurnOverBA"]).Append(_excelOptions.CSVSeparateCharacter);
                            rowBuilder.Append(reader[suffixName + "CommBA"]).Append(_excelOptions.CSVSeparateCharacter);
                            rowBuilder.Append(reader[suffixName + "AmountDueToBA"]).Append(_excelOptions.CSVSeparateCharacter);
                        }

                        break;

                    default:
                        {
                            rowBuilder.Append(reader[suffixName + item]).Append(_excelOptions.CSVSeparateCharacter);
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
                    stream.WriteLine("No information is available");
                }
                else
                {
                    rowBuilder.Append(string.Empty).Append(_excelOptions.CSVSeparateCharacter);
                    rowBuilder.Append(string.Empty).Append(_excelOptions.CSVSeparateCharacter);
                    rowBuilder.Append(ViewResources.Total).Append(_excelOptions.CSVSeparateCharacter);
                    BuildCommoncolumn(reader, metaModel, "sum", rowBuilder);
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