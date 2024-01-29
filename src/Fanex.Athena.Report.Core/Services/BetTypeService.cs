using System.Collections.Generic;
using System.Threading.Tasks;
using Fanex.Athena.Report.Core.Models.DTOs;
using Fanex.Athena.Report.Core.Repositories.Abstraction;
using Fanex.Athena.Report.Core.Services.Interfaces;

namespace Fanex.Athena.Report.Core.Services
{
    public class BetTypeService : IBetTypeService
    {
        private readonly IBetTypeRepository _betTypeRepository;

        public BetTypeService(IBetTypeRepository betTypeRepository)
        {
            _betTypeRepository = betTypeRepository;
        }

        public async Task<IList<CheckedItem>> GetBetTypesByProduct(int licenseeId, string productCode, string languageCode, bool isHideSaba = false)
        {
            return await _betTypeRepository.GetBetTypesByProduct(licenseeId, productCode, languageCode, isHideSaba).ConfigureAwait(false);
        }

    }
}
