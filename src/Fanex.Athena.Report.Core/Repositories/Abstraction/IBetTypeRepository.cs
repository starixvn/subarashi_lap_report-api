using System.Collections.Generic;
using System.Threading.Tasks;
using Fanex.Athena.Report.Core.Models.DTOs;

namespace Fanex.Athena.Report.Core.Repositories.Abstraction
{
    public interface IBetTypeRepository
    {
        Task<IList<CheckedItem>> GetBetTypesByProduct(int licenseeId, string productCode, string languageCode,
                                                      bool isHideSaba);

        Task<IEnumerable<(string ProductCode, int BetType)>> GetBetTypeForProductsAsync(
            int licenseeId, string productCodes);
    }
}
