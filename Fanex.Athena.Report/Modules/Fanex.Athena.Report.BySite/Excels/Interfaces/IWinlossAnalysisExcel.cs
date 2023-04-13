
using Fanex.Athena.Report.BySite.DTOs.WinlossAnalysis;
using Fanex.Athena.Report.BySite.MetaModels;

namespace Fanex.Athena.Report.BySite.Excels.Interfaces
{
    public interface IWinlossAnalysisExcel
    {
        byte[] Export(string templatePath, WinlossAnalysisAggregateDTO data, WinlossAnalysisMetaModel metaModel);
    }
}
