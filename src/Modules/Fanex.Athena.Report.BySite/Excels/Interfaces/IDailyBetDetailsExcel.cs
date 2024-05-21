namespace Fanex.Athena.Report.BySite.Excels.Interfaces
{
    using Fanex.Athena.Report.BySite.DTOs;
    using Fanex.Athena.Report.BySite.MetaModels;
    using Fanex.Athena.Report.Common.Model.ViewModel;
    using System.Threading.Tasks;

    public interface IDailyBetDetailsExcel
    {
        byte[] Export(string template, ResultModel<DailyBetDetails> data, DailyBetDetailsMetaModel metaModel);
    }
}