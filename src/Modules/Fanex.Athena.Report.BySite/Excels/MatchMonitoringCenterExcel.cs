namespace Fanex.Athena.Report.BySite.Excels;

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using Fanex.Athena.Report.BySite.DTOs;
using Fanex.Athena.Report.BySite.Excels.Interfaces;
using Fanex.Athena.Report.BySite.MetaModels;
using Fanex.Athena.Report.Common.Excel;
using Fanex.Athena.Report.Common.Services.Interfaces;
using Fanex.Athena.Report.Core.Extensions;
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
    private string[] HeaderTextList => [
        ViewResources.KickOffTime,
        ViewResources.MatchStatus,
        ViewResources.TriggeredAlert,
        ViewResources.AlertRules,
        ViewResources.Event,
        $"{ViewResources.Stake} %",
        ViewResources.TotalHome,
        ViewResources.TotalAway,
        ViewResources.ForecastHome,
        ViewResources.ForecastAway
    ];

    private MatchMonitoringCenter _data;
    private MatchMonitoringCenterGetMetaModel _metaModel;

    #region Constants

    private const string SheetName = "MatchMonitoringCenter";
    private const int TemplateSheetIndex = 0;
    private const int TemplateTitleRowIndex = 1;
    private const int TemplateHeaderRowIndex = 2;
    private const int TemplateRunningLeagueRowIndex = 3;
    private const int TemplateRunningDataRowIndex = 4;
    private const int TemplateSettledLeagueRowIndex = 5;
    private const int TemplateSettledDataRowIndex = 6;
    private const int TemplateDataRowIndex = 7;

    private const int ColumnKickOffTime = 1;
    private const int ColumnMatchStatus = 2;
    private const int ColumnTriggeredAlert = 3;
    private const int ColumnAlertRules = 4;
    private const int ColumnEvent = 5;
    private const int ColumnStake = 6;
    private const int ColumnTotalHome = 7;
    private const int ColumnTotalAway = 8;
    private const int ColumnForecastHome = 9;
    private const int ColumnForecastAway = 10;

    #endregion Constants

    public string Export(string template, MatchMonitoringCenter data, MatchMonitoringCenterGetMetaModel metaModel)
    {
        _metaModel = metaModel;
        _data = data;

        return CreateFileStream(template);
    }

    private string CreateFileStream(string template)
    {
        var fileInfo = new FileInfo(template);
        using var package = new ExcelPackage(fileInfo, false);

        var templateSheet = package.Workbook.Worksheets[TemplateSheetIndex];
        var currentSheet = package.Workbook.Worksheets.Add(SheetName, templateSheet);

        AssignTitle(currentSheet);
        AssignHeader(currentSheet);
        AssignSheetData(currentSheet);

        package.Workbook.Worksheets.Delete(TemplateSheetIndex);
        var fileName = Path.Combine(EnsureFolderPath(), $"{Guid.NewGuid()}.eplus");

        using var stream = new FileStream(fileName, FileMode.Create, FileAccess.ReadWrite);
        package.SaveAs(stream);

        return fileName;
    }

    private void AssignTitle(ExcelWorksheet sheet)
    {
        var destinationCell = sheet.Cells[TemplateTitleRowIndex, ColumnKickOffTime, TemplateTitleRowIndex, HeaderTextList.Length];
        destinationCell.Merge = true;

        string title = string.Format(
            "{0} - {1}: {2}, {3}: {4}",
            ViewResources.MatchMonitoringCenter,
            ViewResources.Currency,
            _metaModel.CurrencyName,
            ViewResources.BaseCurrency,
            _metaModel.BaseCurrencyName);

        sheet.SetValue(TemplateTitleRowIndex, ColumnKickOffTime, title);
    }

    private void AssignHeader(ExcelWorksheet sheet)
    {
        for (var i = 1; i <= HeaderTextList.Length; i++)
        {
            sheet.SetValue(TemplateHeaderRowIndex, i, HeaderTextList[i - 1]);
        }

        sheet.View.FreezePanes(TemplateRunningLeagueRowIndex, ColumnKickOffTime);
    }

    private void AssignSheetData(ExcelWorksheet sheet)
    {
        int currentRow = TemplateDataRowIndex;
        AssignRunningData(sheet, ref currentRow);
        AssignSettledData(sheet, ref currentRow);

        sheet.DeleteRow(TemplateRunningLeagueRowIndex, TemplateDataRowIndex - TemplateRunningLeagueRowIndex);
    }

    private void AssignRunningData(ExcelWorksheet sheet, ref int currentRow)
    {
        if (!_data.Running.IsPresent())
        {
            return;
        }

        var runningDataByLeague = _data.Running.GroupBy(item => item.LeagueId)
            .Select(group => new
            {
                LeagueId = group.Key,
                LeagueName = group.First().LeagueName,
                Items = group.ToList()
            })
            .ToList();

        foreach (var league in runningDataByLeague)
        {
            sheet.InsertRow(currentRow, 1, TemplateRunningLeagueRowIndex);
            sheet.SetValue(currentRow, ColumnKickOffTime, league.LeagueName);

            var leagueCell = sheet.Cells[currentRow, ColumnKickOffTime, currentRow, ColumnForecastAway];
            leagueCell.Merge = true;
            leagueCell.Style.Font.Bold = true;

            currentRow++;

            foreach (var rowData in league.Items)
            {
                sheet.InsertRow(currentRow, 1, TemplateRunningDataRowIndex);
                AssignCommonData(sheet, rowData, currentRow, false);

                currentRow++;
            }
        }
    }

    private void AssignSettledData(ExcelWorksheet sheet, ref int currentRow)
    {
        if (!_data.Settled.IsPresent())
        {
            return;
        }

        var settledDataByLeague = _data.Settled.GroupBy(item => item.LeagueId)
            .Select(group => new
            {
                LeagueId = group.Key,
                LeagueName = group.First().LeagueName,
                Items = group.ToList()
            })
            .ToList();

        foreach (var league in settledDataByLeague)
        {
            sheet.InsertRow(currentRow, 1, TemplateSettledLeagueRowIndex);
            sheet.SetValue(currentRow, ColumnKickOffTime, league.LeagueName);

            var leagueCell = sheet.Cells[currentRow, ColumnKickOffTime, currentRow, ColumnForecastAway];
            leagueCell.Merge = true;
            leagueCell.Style.Font.Bold = true;

            currentRow++;

            foreach (var rowData in league.Items)
            {
                sheet.InsertRow(currentRow, 1, TemplateSettledDataRowIndex);
                AssignCommonData(sheet, rowData, currentRow, true);

                currentRow++;
            }
        }
    }

    private void AssignCommonData(ExcelWorksheet sheet, MatchMonitoringItem rowData, int rowIndex, bool isSettled)
    {
        sheet.SetValue(rowIndex, ColumnKickOffTime, rowData.KickOffTimeString);
        AssignMatchStatus(sheet, rowData, rowIndex);
        sheet.SetValue(rowIndex, ColumnTriggeredAlert, rowData.IsAlerted ? ViewResources.Yes : ViewResources.No);
        if (rowData.IsAlerted)
        {
            sheet.Cells[rowIndex, ColumnTriggeredAlert].Style.Font.Color.SetColor(Color.Red);
        }
        sheet.SetValue(rowIndex, ColumnAlertRules, FormatAlertRules(rowData.AlertRules));
        sheet.SetValue(rowIndex, ColumnEvent, rowData.MatchName);

        if (isSettled)
        {
            sheet.SetValue(rowIndex, ColumnStake, "-");
            sheet.SetValue(rowIndex, ColumnTotalHome, "-");
            sheet.SetValue(rowIndex, ColumnTotalAway, "-");
            sheet.SetValue(rowIndex, ColumnForecastHome, "-");
            sheet.SetValue(rowIndex, ColumnForecastAway, "-");
        }
        else
        {
            AssignStake(sheet, rowData, rowIndex);
            sheet.SetValue(rowIndex, ColumnTotalHome, rowData.TotalHome);
            sheet.SetValue(rowIndex, ColumnTotalAway, rowData.TotalAway);
            sheet.SetValue(rowIndex, ColumnForecastHome, rowData.ForecastHome);
            sheet.SetValue(rowIndex, ColumnForecastAway, rowData.ForecastAway);

            if (rowData.ForecastHome > 0)
            {
                sheet.Cells[rowIndex, ColumnForecastHome].Style.Font.Color.SetColor(Color.Red);
            }

            if (rowData.ForecastAway > 0)
            {
                sheet.Cells[rowIndex, ColumnForecastAway].Style.Font.Color.SetColor(Color.Red);
            }
        }
    }

    private void AssignMatchStatus(ExcelWorksheet sheet, MatchMonitoringItem rowData, int rowIndex)
    {
        if (rowData.LiveIndicator)
        {
            string liveText = ViewResources.Live;
            string scoreText = $"{rowData.LiveHomeScore} - {rowData.LiveAwayScore}";

            var cell = sheet.Cells[rowIndex, ColumnMatchStatus];
            cell.Style.WrapText = true;

            var richText = cell.RichText;
            richText.Clear();

            richText.Add(liveText);
            var score = richText.Add("\n" + scoreText);
            score.Color = Color.Red;
        }
        else
        {
            sheet.SetValue(rowIndex, ColumnMatchStatus, ViewResources.NoneLive2);
        }
    }

    private void AssignStake(ExcelWorksheet sheet, MatchMonitoringItem rowData, int rowIndex)
    {
        decimal totalStake = rowData.HomeStake + rowData.AwayStake;
        decimal homeStakePercent = totalStake > 0 ? (rowData.HomeStake / totalStake) * 100 : 0;
        decimal awayStakePercent = 100 - homeStakePercent;

        var cell = sheet.Cells[rowIndex, ColumnStake];
        cell.Style.WrapText = true;

        var richText = cell.RichText;
        richText.Clear();

        string homeText = $"Home: {rowData.HomeStake:F2} ({homeStakePercent:F2}%)";
        string awayText = $"Away: {rowData.AwayStake:F2} ({awayStakePercent:F2}%)";

        var homeLine = richText.Add(homeText);
        homeLine.Color = Color.Blue;
        homeLine.Bold = true;

        var awayLine = richText.Add("\n" + awayText);
        awayLine.Color = Color.Red;
        awayLine.Bold = true;
    }

    private string FormatAlertRules(List<string> alertRules)
    {
        if (alertRules == null || alertRules.Count == 0)
        {
            return "-";
        }

        return string.Join(Environment.NewLine, alertRules.Select(rule => "- " + rule));
    }
}