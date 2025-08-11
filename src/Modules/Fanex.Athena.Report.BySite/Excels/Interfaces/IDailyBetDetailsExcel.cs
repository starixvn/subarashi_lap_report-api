namespace Fanex.Athena.Report.BySite.Excels.Interfaces
{
    using Common.Model.ViewModel;
    using DTOs;
    using MetaModels;

    public interface IDailyBetDetailsExcel
    {
        byte[] Export(string template, ResultModel<DailyBetDetails> data, DailyBetDetailsMetaModel metaModel);
    }
}