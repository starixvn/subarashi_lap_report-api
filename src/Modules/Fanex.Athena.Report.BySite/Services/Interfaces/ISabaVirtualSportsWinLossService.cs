using System.Collections.Generic;
using System.Threading.Tasks;
using Fanex.Athena.Report.BySite.DTOs;
using Fanex.Athena.Report.BySite.MetaModels;
using Fanex.Athena.Report.Common.Extensions;
using Fanex.Athena.Report.Common.Model.ViewModel;

namespace Fanex.Athena.Report.BySite.Services.Interfaces
{
    public interface ISabaVirtualSportsWinLossService
    {
        Task<SortableReportResultModel<SabaVirtualSportsWinLossDetail>> GetDataWithDrillDownAsync(SabaVirtualSportsWinLossMetaModel metaModel, IReadOnlyDictionary<int, string> currencyList);

        void SortData(SortableList<SabaVirtualSportsWinLossDetail> dataList, string sortBy, string sortOrder = "asc");

        IEnumerable<(int Id, int SportId, string Name, IEnumerable<int> SabaSportIds)> GetSportList();
    }
}
