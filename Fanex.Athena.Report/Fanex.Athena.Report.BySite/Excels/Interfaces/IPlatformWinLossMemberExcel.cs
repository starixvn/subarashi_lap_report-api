using Fanex.Athena.Report.BySite.DTOs;
using Fanex.Athena.Report.BySite.MetaModels;
using Fanex.Athena.Report.Common.Model.ViewModel;

using NPOI.SS.UserModel;

namespace Fanex.Athena.Report.BySite.Excels.Interfaces
{
    public interface IPlatformWinLossMemberExcel
    {
        IWorkbook Export(string template, SortableResultModel<PlatformWinLossMember> data,
            PlatformWinLossMetaModel metaModel);
    }
}
