namespace Fanex.Athena.Report.BySite.Services
{
    using Fanex.Athena.Report.BySite.DTOs;
    using Fanex.Athena.Report.BySite.MetaModels;
    using Fanex.Athena.Report.BySite.Repositories.Interfaces;
    using Fanex.Athena.Report.BySite.Services.Interfaces;
    using Fanex.Athena.Report.Common.Model.ViewModel;
    using Fanex.Athena.Report.Resources.Constants;
    using System.Threading.Tasks;

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