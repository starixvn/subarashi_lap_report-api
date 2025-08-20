namespace Fanex.Athena.Report.Core.Repositories.Abstraction
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Models.DTOs;

    public interface IBetTypeRepository
    {
        Task<IList<(string ProductPrefix, int BetTypeId)>> GetBettypes(string xmlSportList);

        Task<IList<CheckedItem>> GetBetTypesByProduct(int licenseeId, string productCode, string languageCode,
                                                      bool isHideSaba);

        Task<IEnumerable<(string ProductCode, int BetType)>> GetBetTypeForProductsAsync(
            int licenseeId, string productCodes);
    }
}
