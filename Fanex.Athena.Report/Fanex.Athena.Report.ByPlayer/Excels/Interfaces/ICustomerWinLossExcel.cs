namespace Fanex.Athena.Report.ByPlayer.Excels.Interfaces
{
    using Fanex.Athena.Report.ByPlayer.DTO;
    using Fanex.Athena.Report.ByPlayer.MetaModels;
    using Fanex.Athena.Report.Common.Model.ViewModel;
    using System.Collections.Generic;

    public interface ICustomerWinLossExcel
    {
        byte[] Export(string template, ResultModel<CustomerWinLoss> data, CustomerWinLossMetaModel metaModel);

        byte[] ExportFile(string template, IList<ResultModel<CustomerWinLoss>> data, CustomerWinLossMetaModel metaModel);
    }
}