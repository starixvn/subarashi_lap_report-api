namespace Fanex.Athena.Report.Core.Services.Interfaces
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Models.DTOs;

    public interface ISportService
    {
        Task<IEnumerable<SportProductItem>> GetSportsAndProducts(int licenseeId, string languageCode,
                                                                 string productCodes = "");

        Task<IEnumerable<SportProductItem>> GetSportAndProductAndSabaSport(
            int licenseeId, string languageCode, bool isHideSaba, string productCodes = "");

        Task<IEnumerable<CheckedItem>> GetSportsForTreeView(int licenseeId, string languageCode,
                                                            string productCodes = "", bool isHideSaba = false);
    }
}
