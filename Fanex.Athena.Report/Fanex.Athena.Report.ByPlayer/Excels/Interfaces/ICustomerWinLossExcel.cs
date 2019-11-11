namespace Fanex.Athena.Report.ByPlayer.Excels.Interfaces
{
    using Fanex.Athena.Report.ByPlayer.DTO;
    using Fanex.Athena.Report.ByPlayer.MetaModels;
    using Fanex.Athena.Report.Common.Model.ViewModel;

    public interface ICustomerWinLossExcel
    {
        byte[] Export(string template, ResultModel<CustomerWinLoss> data, CustomerWinLossMetaModel metaModel);
    }
}