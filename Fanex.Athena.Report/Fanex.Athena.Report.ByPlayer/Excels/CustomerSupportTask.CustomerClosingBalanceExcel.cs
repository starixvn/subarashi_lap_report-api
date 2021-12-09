using Fanex.Athena.Report.ByPlayer.Constants;
using Fanex.Athena.Report.ByPlayer.DTO;
using Fanex.Athena.Report.ByPlayer.Excels.Interfaces;
using Fanex.Athena.Report.ByPlayer.MetaModels;
using Fanex.Athena.Report.Common.Excel;
using Fanex.Athena.Report.Common.Extensions;
using Fanex.Athena.Report.Common.Services;
using Fanex.Athena.Report.Common.Services.Interfaces;
using Fanex.Athena.Report.Resources;
using Fanex.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;
using OfficeOpenXml;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace Fanex.Athena.Report.ByPlayer.Excels
{
    public class CustomerClosingBalanceExcel : BaseCustomerSupportTaskExcel<CustomerClosingBalance>, ICustomerSupportTaskExcel
    {
        private const int StartTemplateHeaderRowIndex = 2;
        private const int ColumnIndex = 1;
        private const int ColumnExternalId = 2;
        private const int ColumnSystemId = 3;
        private const int ColumnCurrency = 4;
        private const int ColumnClosingBalance = 5;

        private readonly IBalanceUpService _balanceUpService;

        public CustomerClosingBalanceExcel(
             IBalanceUpService balanceUpService,
             IFileProvider fileProvider,
             IFileService fileService,
             IOptions<ExcelOptions> excelOptionsAccessor,
             IHostingEnvironment enviroment) : base(fileProvider, fileService, excelOptionsAccessor, enviroment)
        {
            _balanceUpService = balanceUpService;
        }

        public bool Should(CustomerSupportTaskMetaModel metaModel)
           => metaModel.SupportTask == CustomerSupportTaskConstant.CustomerSupportTaskReportType.CustomerClosingBalance
            || metaModel.SupportTask == CustomerSupportTaskConstant.CustomerSupportTaskReportType.RunnningTicketClosingBalance;

        protected override string TemplatePath => "/ExcelTemplate/CustomerReports/CustomerClosingBalance.xlsx";

        protected override string UspName => "Lrf_Rpt_CustomerClosingBalance";

        protected override int ColumnTemplateCount => 5;
        protected override int TemplateNoInfoRowIndex => 5;
        protected override int TemplateDataRowIndex => 4;

        protected override string GetFileName(CustomerSupportTaskMetaModel metaModel)
        {
            var fileName = metaModel.SupportTask == CustomerSupportTaskConstant.CustomerSupportTaskReportType.CustomerClosingBalance
                ? nameof(ViewResources.CustomerClosingBalance) : nameof(ViewResources.RunningTicketClosingBalance);
            return $"{fileName}.xlsx";
        }

        protected override async Task AddParameters(IObjectDb db, CustomerSupportTaskMetaModel metaModel)
        {
            var balanceUpInfo = await _balanceUpService.GetBalanceUpAsync();
            db.SetParameterValue("AgentList", metaModel.AgentListXml);
            db.SetParameterValue("ReportType", metaModel.SupportTask);
            db.SetParameterValue("MoveBetsDay", balanceUpInfo.MoveBetsDay);
        }

        protected override void AssignHeader(ExcelWorksheet sheet)
        {
            sheet.SetValue(StartTemplateHeaderRowIndex, ColumnIndex, "#");
            sheet.SetValue(StartTemplateHeaderRowIndex, ColumnExternalId, ViewResources.ExternalId);
            sheet.SetValue(StartTemplateHeaderRowIndex, ColumnSystemId, ViewResources.SystemId);
            sheet.SetValue(StartTemplateHeaderRowIndex, ColumnCurrency, ViewResources.CurrencyName);
            sheet.SetValue(StartTemplateHeaderRowIndex, ColumnClosingBalance, ViewResources.Balance);
            sheet.View.FreezePanes(TemplateDataRowIndex, ColumnIndex);
        }

        protected override void AssignBody(ExcelWorksheet sheet, IEnumerable<CustomerClosingBalance> dataList)
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
                sheet.SetValue(currentRowIndex, ColumnClosingBalance, rowValue.ClosingBalance);

                rowIndexPerSheet++;
            }
        }

        protected override void BuildDataRecord(IDataReader reader, CustomerClosingBalance record)
        {
            record.ClosingBalance = reader.GetVal<decimal>("ClosingBalance");
        }

        protected override string BuildTitle(CustomerSupportTaskMetaModel metaModel)
         => metaModel.SupportTask == CustomerSupportTaskConstant.CustomerSupportTaskReportType.CustomerClosingBalance
            ? ViewResources.CustomerClosingBalance : ViewResources.RunningTicketClosingBalance;
    }
}
