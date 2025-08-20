namespace Fanex.Athena.Report.BySite.Excels.Interfaces
{
    using Common.Model.ViewModel;
    using DTOs;
    using MetaModels;

    public interface IPlatformWinLossMemberExcel
    {
        byte[] Export(
            string template,
            SortableResultModel<PlatformWinLossMember> data,
            PlatformWinLossMetaModel metaModel);
    }
}