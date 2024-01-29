using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fanex.Athena.Report.Common.Services.Interfaces;
using Fanex.Athena.Report.Core.Constants;
using Fanex.Athena.Report.Core.Models.DTOs;
using Fanex.Athena.Report.Core.Repositories.Abstraction;
using Fanex.Athena.Report.Core.Services.Interfaces;
using Fanex.Athena.Report.Resources;
using Fanex.Athena.Report.Resources.Constants;

namespace Fanex.Athena.Report.Core.Services
{
    public class SportService : ISportService
    {
        private readonly ISportRepository _sportRepository;

        private readonly ICacheRefAdapter _cacheRefAdapter;

        public SportService(ISportRepository sportRepository, ICacheRefAdapter cacheRefAdapter)
        {
            _sportRepository = sportRepository;
            _cacheRefAdapter = cacheRefAdapter;
        }

        public Task<IEnumerable<SportProductItem>> GetSportsAndProducts(int licenseeId, string languageCode, string productCodes = "") 
            => _sportRepository.GetSportsAndProducts(licenseeId, languageCode, productCodes);

        public async Task<IEnumerable<SportProductItem>> GetSportAndProductAndSabaSport(int licenseeId, string languageCode, string productCodes = "",
                                                                                  bool isHideSaba = false)
        {
            var result = new List<SportProductItem>();
            result.AddRange(await GetSportsAndProducts(licenseeId, languageCode, productCodes));

            result.AddRange(GetSabaSports(isHideSaba).Select(t => new SportProductItem
            {
                Checked = true,
                Id = t.Id,
                Name = t.Name,
                ParentId = ProductTypes.Sportsbook
            }));

            return result;
        }

        public IEnumerable<SabaSport> GetSabaSports(bool isHideSaba = false)
            => SportType.GetSabaVirtualSports().Select(t => new SabaSport
            {
                Id = t.Item1,
                Name = GetSabaName(t.Item2, t.Item3, isHideSaba),
                SportTypeId = t.Item2
            });

        private string GetSabaName(int sportId, string viewResource = "", bool isHideSaba = false)
        {
            var prefixText = isHideSaba ? ViewResources.Virtual : ViewResources.Saba;

            var sabaSportName = sportId != 0
                ? _cacheRefAdapter.GetSportName(sportId)
                : ViewResources.ResourceManager.GetString(viewResource);

            return $"{prefixText}{sabaSportName}";
        }
    }
}
