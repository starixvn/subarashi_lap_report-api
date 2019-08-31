namespace Fanex.Athena.Report.BySite.Controllers
{
    using System.Threading.Tasks;
    using Fanex.Athena.Report.BySite.DTOs;
    using Fanex.Athena.Report.BySite.MetaModels;
    using Fanex.Athena.Report.BySite.Models;
    using Fanex.Athena.Report.BySite.Repositories.Interfaces;
    using Microsoft.AspNetCore.Mvc;

    [ApiController]
    [Route("api/[controller]/[action]")]
    public class PlatformWinLossController : ControllerBase
    {
        private readonly IPlatformWinLossRepository _repository;

        public PlatformWinLossController(IPlatformWinLossRepository repository)
        {
            _repository = repository;
        }

        [HttpPost]
        public async Task<ResultModel<PlatformWinLossSummary>> GetSummaryData(PlatformWinLossMetaModel metaModel)
        {
            var data = await _repository.GetSummaryDataAsync(metaModel).ConfigureAwait(false);

            return data;
        }

        [HttpPost]
        public async Task<ResultModel<PlatformWinLossMember>> GetMemberData(PlatformWinLossMetaModel metaModel)
        {
            var data = await _repository.GetMemberDataAsync(metaModel).ConfigureAwait(false);

            return data;
        }
    }
}