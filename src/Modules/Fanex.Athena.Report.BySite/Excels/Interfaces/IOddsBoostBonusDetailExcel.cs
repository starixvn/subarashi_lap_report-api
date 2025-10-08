namespace Fanex.Athena.Report.BySite.Excels.Interfaces
{
    using Common.Model.ViewModel;
    using DTOs;
    using MetaModels;

    public interface IOddsBoostBonusDetailExcel
    {
        byte[] Export(string template, SortableReportResultModel<OddsBoostBonusDetail> data, ExtraPayoutMetaModel metaModel);
    }
}
