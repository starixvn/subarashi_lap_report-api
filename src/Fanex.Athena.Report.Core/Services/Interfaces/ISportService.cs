using System.Collections.Generic;
using System.Threading.Tasks;
using Fanex.Athena.Report.Core.Models.DTOs;

namespace Fanex.Athena.Report.Core.Services.Interfaces
{
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
