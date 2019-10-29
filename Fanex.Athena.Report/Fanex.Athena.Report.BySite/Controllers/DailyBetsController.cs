namespace Fanex.Athena.Report.BySite.Controllers
{
    using System.Threading.Tasks;

    using Fanex.Athena.Report.BySite.MetaModels;
    using Fanex.Athena.Report.BySite.Repositories.Interfaces;

    using Microsoft.AspNetCore.Mvc;

    [ApiController]
    [Route("api/[controller]/[action]")]
    public class DailyBetsController : ControllerBase
    {
        private readonly IDailyBetsRepository _repository;

        public DailyBetsController(IDailyBetsRepository repository)
        {
            _repository = repository;
        }

        [HttpPost]
        public async Task<dynamic> Index(DailyBetsMetaModel metaModel)
        {
            var result = await _repository.GetDataAsync(metaModel).ConfigureAwait(false);

            return new
            {
                result.DataList,
                Total = result.TotalData,
                result.TotalRows
            };
        }
    }
}