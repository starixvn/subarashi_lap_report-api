using Fanex.Athena.Report.ByPlayer.Constants;
using Fanex.Athena.Report.ByPlayer.DTO;
using Fanex.Athena.Report.ByPlayer.Excels.Interfaces;
using Fanex.Athena.Report.ByPlayer.MetaModels;
using Fanex.Athena.Report.Common.Excel;
using Fanex.Athena.Report.Common.Extensions;
using Fanex.Athena.Report.Common.Services;
using Fanex.Athena.Report.Resources;
using Fanex.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace Fanex.Athena.Report.ByPlayer.Excels
{
    public class CustomerRunningTicketExcel : BaseCustomerSupportTaskExcel<CustomerRunningTicket>, ICustomerSupportTaskExcel
    {
        private const int StartTemplateHeaderRowIndex = 2;

        private const int ColumnIndex = 1;
        private const int ColumnExternalId = 2;
        private const int ColumnSystemId = 3;
        private const int ColumnCurrency = 4;
        private const int ColumnTransId = 5;
        private const int ColumnBetType = 6;
        private const int ColumnStatus = 7;
        private const int ColumnTransTime = 8;
        private const int ColumnWinLostDate = 9;
        private const int ColumnStake = 10;
        private const int ColumnActualStake = 11;

        public CustomerRunningTicketExcel(
             IFileProvider fileProvider,
             IFileService fileService,
             IOptions<ExcelOptions> excelOptionsAccessor,
             IHostingEnvironment enviroment) : base(fileProvider, fileService, excelOptionsAccessor, enviroment)
        {
        }

        public bool Should(CustomerSupportTaskMetaModel metaModel)
         => metaModel.SupportTask == CustomerSupportTaskConstant.CustomerSupportTaskReportType.RunnningTicket;

        protected override string TemplatePath => "/ExcelTemplate/CustomerReports/CustomerRunningTicket.xlsx";
        protected override string UspName => "Lrf_Rpt_CustomerRunningTicket";

        protected override int ColumnTemplateCount => 11;
        protected override int TemplateNoInfoRowIndex => 5;
        protected override int TemplateDataRowIndex => 4;

        protected override string GetFileName(CustomerSupportTaskMetaModel metaModel)
            => "RunningTicketList.xlsx";

        protected override Task AddParameters(IObjectDb db, CustomerSupportTaskMetaModel metaModel)
        {
            db.SetParameterValue("AgentList", metaModel.AgentListXml);
            return Task.CompletedTask;
        }

        protected override void AssignHeader(ExcelWorksheet sheet)
        {
            sheet.SetValue(StartTemplateHeaderRowIndex, ColumnIndex, "#");
            sheet.SetValue(StartTemplateHeaderRowIndex, ColumnExternalId, ViewResources.ExternalId);
            sheet.SetValue(StartTemplateHeaderRowIndex, ColumnSystemId, ViewResources.SystemId);
            sheet.SetValue(StartTemplateHeaderRowIndex, ColumnCurrency, ViewResources.CurrencyName);
            sheet.SetValue(StartTemplateHeaderRowIndex, ColumnTransId, ViewResources.TransId);
            sheet.SetValue(StartTemplateHeaderRowIndex, ColumnBetType, ViewResources.BetType);
            sheet.SetValue(StartTemplateHeaderRowIndex, ColumnStatus, ViewResources.Status);
            sheet.SetValue(StartTemplateHeaderRowIndex, ColumnTransTime, ViewResources.TransTime);
            sheet.SetValue(StartTemplateHeaderRowIndex, ColumnWinLostDate, ViewResources.WinLostDate);
            sheet.SetValue(StartTemplateHeaderRowIndex, ColumnStake, ViewResources.Stake);
            sheet.SetValue(StartTemplateHeaderRowIndex, ColumnActualStake, ViewResources.ActualStake);

            sheet.View.FreezePanes(TemplateDataRowIndex, ColumnIndex);
        }

        protected override void AssignBody(ExcelWorksheet sheet, IEnumerable<CustomerRunningTicket> dataList)
        {
            sheet.InsertRow(TemplateDataRowIndex + 1, dataList.Count() - 1, TemplateDataRowIndex);

            var rowIndexPerSheet = 0;
            foreach (var rowValue in dataList)
            {
                var currentRowIndex = rowIndexPerSheet + TemplateDataRowIndex;
                sheet.SetValue(currentRowIndex, ColumnIndex, rowValue.Index);
                sheet.SetValue(currentRowIndex, ColumnExternalId, rowValue.ExternalId);
                sheet.SetValue(currentRowIndex, ColumnSystemId, rowValue.SystemId);
                sheet.SetValue(currentRowIndex, ColumnCurrency, rowValue.Currency);
                sheet.SetValue(currentRowIndex, ColumnTransId, rowValue.TransId);
                sheet.SetValue(currentRowIndex, ColumnBetType, rowValue.BetType);
                sheet.SetValue(currentRowIndex, ColumnStatus, rowValue.Status);
                sheet.SetValue(currentRowIndex, ColumnTransTime, rowValue.TransTime);
                sheet.SetValue(currentRowIndex, ColumnWinLostDate, rowValue.WinlostDate);
                sheet.SetValue(currentRowIndex, ColumnStake, rowValue.Stake);
                sheet.SetValue(currentRowIndex, ColumnActualStake, rowValue.ActualStake);

                rowIndexPerSheet++;
            }
        }

        protected override void BuildDataRecord(IDataReader reader, CustomerRunningTicket record)
        {
            record.TransId = reader.GetVal<string>("TransId");
            record.BetType = reader.GetVal<string>("BetType");
            record.Status = reader.GetVal<string>("Status");
            record.TransTime = reader.GetVal<DateTime>("TransDate");
            record.WinlostDate = reader.GetVal<DateTime>("WinlostDate");
            record.Stake = reader.GetVal<decimal>("Stake");
            record.ActualStake = reader.GetVal<decimal>("Actual_stake");
        }

        protected override string BuildTitle(CustomerSupportTaskMetaModel metaModel)
            => ViewResources.RunningTicketList;
    }
}
