namespace Fanex.Athena.Report.BySite.Services
{
    using System.Threading.Tasks;
    using Fanex.Athena.Report.BySite.DTOs;
    using Fanex.Athena.Report.BySite.MetaModels;
    using Fanex.Athena.Report.BySite.Repositories.Interfaces;
    using Fanex.Athena.Report.BySite.Services.Interfaces;
    using Fanex.Athena.Report.Common.Model.ViewModel;
    using Fanex.Athena.Report.Resources.Constants;

    public class ExtraPayoutService : IExtraPayoutService
    {
        private readonly IExtraPayoutRepository _repository;

        public ExtraPayoutService(IExtraPayoutRepository repository)
        {
            _repository = repository;
        }

        public async Task<SortableReportResultModel<ExtraPayout>> GetDataAsync(ExtraPayoutMetaModel metaModel)
        {
            if (metaModel.DrillDownLevel == (int)ReportLevel.Main)
            {
                return await _repository.GetExtraPayoutMainData(metaModel).ConfigureAwait(false);
            }

            return await _repository.GetExtraPayoutMemberData(metaModel).ConfigureAwait(false);
        }

        public async Task<SortableReportResultModel<ExtraPayoutDetail>> GetDetailDataAsync(ExtraPayoutDetailMetaModel metaModel)
        {
            return await _repository.GetExtraPayoutDetailData(metaModel).ConfigureAwait(false);
        }
    }
}