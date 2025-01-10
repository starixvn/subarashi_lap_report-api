namespace Fanex.Athena.Report.BySite.Services
{
    using Fanex.Athena.Report.BySite.DTOs;
    using Fanex.Athena.Report.BySite.MetaModels;
    using Fanex.Athena.Report.BySite.Repositories.Interfaces;
    using Fanex.Athena.Report.BySite.Services.Interfaces;
    using Fanex.Athena.Report.Common.Model.ViewModel;
    using Fanex.Athena.Report.Resources.Constants;
    using System.Threading.Tasks;

    public class GodOfFortuneWinLossService : IGodOfFortuneWinLossService
    {
        private readonly IGodOfFortuneWinLossRepository _repository;

        public GodOfFortuneWinLossService(IGodOfFortuneWinLossRepository repository)
        {
            _repository = repository;
        }

        public async Task<SortableReportResultModel<GodOfFortuneWinLoss>> GetDataAsync(GodOfFortuneWinLossMetaModel metaModel)
        {
            SortableReportResultModel<GodOfFortuneWinLoss> result;

            if (metaModel.DrillDownLevel == (int)ReportLevel.Main)
            {
                result = await _repository.GetSummaryWinloss(metaModel).ConfigureAwait(false);
            }
            else if (metaModel.DrillDownLevel == (int)ReportLevel.Customer)
            {
                result = await _repository.GetCustomerWinlossDetail(metaModel).ConfigureAwait(false);
            }
            else
            {
                result = await _repository.GetAgentWinlossDetail(metaModel).ConfigureAwait(false);
            }

            return result;
        }
    }
}