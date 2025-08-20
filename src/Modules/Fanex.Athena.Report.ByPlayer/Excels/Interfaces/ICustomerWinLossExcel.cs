namespace Fanex.Athena.Report.ByPlayer.Excels.Interfaces
{
    using Common.Model.ViewModel;
    using DTO;
    using MetaModels;

    public interface ICustomerWinLossExcel
    {
        string Export(string template, ResultModel<CustomerWinLoss> data, CustomerWinLossMetaModel metaModel);
    }
}