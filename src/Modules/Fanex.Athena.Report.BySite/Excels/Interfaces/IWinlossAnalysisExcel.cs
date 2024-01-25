using Fanex.Athena.Report.BySite.Excels.Options;
using Fanex.Athena.Report.BySite.Models;

namespace Fanex.Athena.Report.BySite.Excels.Interfaces
{
    public interface IWinlossAnalysisExcel
    {
        byte[] Export(string templatePath, WinlossAnalysisResultModel data, WinlossAnalysisExcelOption option);
    }
}