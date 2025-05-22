namespace Fanex.Athena.Report.BySite.Excels;

using System;
using System.Drawing;
using System.IO;
using System.Linq;
using Fanex.Athena.Report.BySite.DTOs;
using Fanex.Athena.Report.BySite.Excels.Interfaces;
using Fanex.Athena.Report.BySite.MetaModels;
using Fanex.Athena.Report.Common.Excel;
using Fanex.Athena.Report.Common.Services.Interfaces;
using Fanex.Athena.Report.Resources;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Options;
using OfficeOpenXml;

public class MatchMonitoringCenterExcel(
    IFileService fileService,
    IOptions<ExcelOptions> excelOptionsAccessor,
    IWebHostEnvironment environment
) : BaseExcel(environment, fileService, excelOptionsAccessor), IMatchMonitoringCenterExcel
{
    // private readonly string[] _headerTextList = [
    //     ViewResources.KickOffTime,
    //     ViewResources.MatchStatus,
    //     ViewResources.TriggeredAlert,
    //     ViewResources.AlertRules,
    //     ViewResources.Event,
    //     $"{ViewResources.Stake} %",
    //     ViewResources.TotalHome,
    //     ViewResources.TotalAway,
    //     ViewResources.ForecastHome,
    //     ViewResources.ForecastAway
    // ];

    private MatchMonitoringCenter _data;

    private MatchMonitoringCenterGetMetaModel _metaModel;

    // #region Constants
    // private const string SheetName = "MatchMonitoringCenter";
    // private const int TemplateSheetIndex = 0;
    // private const int TemplateTitleRowIndex = 1;
    // private const int TemplateHeaderRowIndex = 2;
    // private const int TemplateDataRowIndex = 3;
    // private const int ColumnIndex = 1;

    // #endregion

    public string Export(string template, MatchMonitoringCenter data, MatchMonitoringCenterGetMetaModel metaModel)
    {
        // _metaModel = metaModel;
        // _data = data;

        // return CreateFileStream(template);
        return string.Empty;
    }

    // private string CreateFileStream(string template)
    // {
    //     var fileInfo = new FileInfo(template);
    //     using var package = new ExcelPackage(fileInfo, false);

    //     var templateSheet = package.Workbook.Worksheets[TemplateSheetIndex];
    //     var currentSheet = package.Workbook.Worksheets.Add(SheetName, templateSheet);

    //     AssignTitle(currentSheet);
    //     AssignHeader(currentSheet);
    //     AssignSheetData(currentSheet);

    //     package.Workbook.Worksheets.Delete(TemplateSheetIndex);
    //     var fileName = Path.Combine(EnsureFolderPath(), $"{Guid.NewGuid()}.eplus");

    //     using var stream = new FileStream(fileName, FileMode.Create, FileAccess.ReadWrite);
    //     package.SaveAs(stream);

    //     return fileName;
    // }

    // private void AssignTitle(ExcelWorksheet sheet)
    // {
    //     var destinationCell = sheet.Cells[TemplateTitleRowIndex, ColumnIndex, TemplateTitleRowIndex, _headerTextList.Length];
    //     destinationCell.Merge = true;

    //     string title = string.Format(
    //         "{0}, {1}: {2}, {3}: {4}",
    //         ViewResources.MatchMonitoringCenter,
    //         ViewResources.Currency,
    //         _metaModel.CurrencyName,
    //         ViewResources.BaseCurrency,
    //         _metaModel.BaseCurrencyName);

    //     sheet.SetValue(TemplateTitleRowIndex, ColumnIndex, title);
    // }

    // private void AssignHeader(ExcelWorksheet sheet)
    // {
    //     for (var i = 1; i <= _headerTextList.Length; i++)
    //     {
    //         sheet.SetValue(TemplateHeaderRowIndex, i, _headerTextList[i - 1]);
    //     }

    //     sheet.View.FreezePanes(TemplateDataRowIndex, ColumnIndex);
    // }

    // private void AssignSheetData(ExcelWorksheet sheet)
    // {
    //     var rowIndex = 0;
    //     AssignRunningData(sheet, ref rowIndex);
    //     AssignSettledData(sheet, ref rowIndex);
    // }

    // private void AssignRunningData(ExcelWorksheet sheet, ref int rowIndex)
    // {
    //     if (_data.Running.Count() > 1)
    //     {
    //         sheet.InsertRow(TemplateDataRowIndex + 1, _data.Running.Count() - 1, TemplateDataRowIndex);
    //     }
    //     foreach (var rowData in _data.Running)
    //     {
    //         var realIndex = rowIndex + TemplateDataRowIndex;
    //         sheet.SetValue(realIndex, ColumnIndex, rowIndex + 1);

    //         AssignCommonData(sheet, rowData, realIndex);

    //         rowIndex++;
    //     }
    // }

    // private void AssignSettledData(ExcelWorksheet sheet, ref int rowIndex)
    // {
    //     if (_data.Settled.Count() > 1)
    //     {
    //         sheet.InsertRow(TemplateDataRowIndex + 1, _data.Settled.Count() - 1, TemplateDataRowIndex);
    //     }

    //     foreach (var rowData in _data.Settled)
    //     {
    //         var realIndex = rowIndex + TemplateDataRowIndex;
    //         sheet.SetValue(realIndex, ColumnIndex, rowIndex + 1);

    //         AssignCommonData(sheet, rowData, realIndex);

    //         rowIndex++;
    //     }
    // }

    // private void AssignCommonData(ExcelWorksheet sheet, MatchMonitoringItem rowData, int rowIndex)
    // {
    //     sheet.SetValue(rowIndex, ColumnReportBy,
    //         rowData.ReportBy.ToString(_metaModel.ReportBy == ReportDaily ? "MM/dd/yyyy" : "MM/yyyy"));
    //     sheet.SetValue(rowIndex, ColumnGroupBy, rowData.GroupBy);
    //     sheet.SetValue(rowIndex, ColumnBetCount, rowData.BetCount);
    //     sheet.SetValue(rowIndex, ColumnTurnOver, rowData.Turnover);
    //     sheet.SetValue(rowIndex, ColumnCustomerComm, rowData.CustomerComm);
    //     sheet.SetValue(rowIndex, ColumnLicenseeWl, rowData.LicenseeWinLoss);
    //     sheet.SetValue(rowIndex, ColumnLicenseeComm, rowData.LicenseeComm);
    //     sheet.SetValue(rowIndex, ColumnLicenseeNetWl, rowData.LicenseeNetWinLoss);
    //     sheet.SetValue(rowIndex, ColumnMargin, rowData.Margin / Percentage);

    //     if (rowData.ForecastAway > 0)
    //     {
    //         sheet.Cells[rowIndex, ColumnLicenseeWl].Style.Font.Color.SetColor(Color.Red);
    //     }

    //     if (rowData.ForecastHome > 0)
    //     {
    //         sheet.Cells[rowIndex, ColumnLicenseeNetWl].Style.Font.Color.SetColor(Color.Red);
    //     }
    // }
}

