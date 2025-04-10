namespace Fanex.Athena.Report.BySite.Excels.Interfaces
{
    using Common.Model.ViewModel;
    using DTOs;
    using MetaModels;

    public interface IWinLossByTimeZoneExcel
    {
        string Export(string template, ReportResultModel<WinLossByTimeZone> data, WinLossByTimeZoneMetaModel metaModel);
    }
}