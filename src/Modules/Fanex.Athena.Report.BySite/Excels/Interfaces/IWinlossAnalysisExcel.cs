namespace Fanex.Athena.Report.BySite.Excels.Interfaces
{
    using Models;
    using Options;

    public interface IWinlossAnalysisExcel
    {
        byte[] Export(string templatePath, WinlossAnalysisResultModel data, WinlossAnalysisExcelOption option);
    }
}