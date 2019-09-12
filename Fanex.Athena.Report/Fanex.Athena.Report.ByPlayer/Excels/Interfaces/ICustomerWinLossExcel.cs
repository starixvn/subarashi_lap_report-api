using Fanex.Athena.Report.ByPlayer.DTO;
using Fanex.Athena.Report.ByPlayer.MetaModels;
using Fanex.Athena.Report.Common.Model.ViewModel;
using NPOI.SS.UserModel;

namespace Fanex.Athena.Report.ByPlayer.Excels.Interfaces
{
    public interface ICustomerWinLossExcel
    {
        IWorkbook Export(string template, ResultModel<CustomerWinLoss> data, CustomerWinLossMetaModel metaModel);
    }
}