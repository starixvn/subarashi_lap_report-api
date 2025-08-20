namespace Fanex.Athena.Report.BySite.Services
{
    using System.Threading.Tasks;
    using Common.Model.ViewModel;
    using DTOs;
    using Interfaces;
    using MetaModels;
    using Repositories.Interfaces;
    using Resources.Constants;

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