namespace Fanex.Athena.Report.ByPlayer.Excels.Interfaces
{
    using System.IO;
    using Fanex.Athena.Report.ByPlayer.DTO;
    using Fanex.Athena.Report.ByPlayer.MetaModels;
    using Fanex.Athena.Report.Common.Model.ViewModel;

    public interface ICustomerWinLossExcel
    {
        string Export(string template, ResultModel<CustomerWinLoss> data, CustomerWinLossMetaModel metaModel);
    }
}