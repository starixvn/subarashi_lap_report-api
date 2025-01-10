namespace Fanex.Athena.Report.BySite.Services
{
    using Fanex.Athena.Report.BySite.MetaModels;
    using Fanex.Athena.Report.BySite.Repositories.Interfaces;
    using Fanex.Athena.Report.BySite.Services.Interfaces;
    using Fanex.Athena.Report.Resources.Constants;
    using System.Threading.Tasks;

    public class AbnormalBetService : IAbnormalBetService
    {
        private readonly IAbnormalBetRepository _repository;

        public AbnormalBetService(IAbnormalBetRepository repository)
        {
            _repository = repository;
        }

        public async Task<dynamic> GetDataAsync(AbnormalBetMetaModel metaModel)
        {
            dynamic result = metaModel.ViewIn switch
            {
                (int)AbnormalBetViewInLevel.Summary => await _repository.GetSummaryData(metaModel).ConfigureAwait(false),
                (int)AbnormalBetViewInLevel.IpSummary => await _repository.GetIpSummaryData(metaModel).ConfigureAwait(false),
                _ => await _repository.GetDetailData(metaModel).ConfigureAwait(false),
            };
            return result;
        }
    }
}