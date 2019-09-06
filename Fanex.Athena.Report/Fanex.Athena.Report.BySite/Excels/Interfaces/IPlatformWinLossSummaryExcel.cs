namespace Fanex.Athena.Report.BySite.Excels.Interfaces
{
    using Fanex.Athena.Report.BySite.DTOs;
    using Fanex.Athena.Report.BySite.MetaModels;
    using Fanex.Athena.Report.Common.Model.ViewModel;
    using NPOI.SS.UserModel;

    public interface IPlatformWinLossSummaryExcel
    {
        IWorkbook Export(string template, ResultModel<PlatformWinLossSummary> data, PlatformWinLossMetaModel metaModel);
    }
}
