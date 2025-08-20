namespace Fanex.Athena.Report.BySite.Services
{
    using System.Threading.Tasks;
    using Common.Model.ViewModel;
    using DTOs;
    using Interfaces;
    using MetaModels;
    using Repositories.Interfaces;
    using Resources.Constants;

    public class NewCashOutWinLossService : INewCashOutWinLossService
    {
        private readonly INewCashOutWinLossRepository _repository;

        public NewCashOutWinLossService(INewCashOutWinLossRepository repository)
        {
            _repository = repository;
        }

        public async Task<SortableReportResultModel<CashOutWinLoss>> GetDataAsync(NewCashOutWinLossMetaModel metaModel)
        {
            if (metaModel.DrillDownLevel == (int)ReportLevel.Main)
            {
                return await _repository.GetNewCashOutWinLossMainData(metaModel).ConfigureAwait(false);
            }

            return await _repository.GetNewCashOutWinLossMemberData(metaModel).ConfigureAwait(false);
        }
    }
}