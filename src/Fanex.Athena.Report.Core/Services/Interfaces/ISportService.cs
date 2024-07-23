using Fanex.Athena.Report.Core.Models.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

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

        IEnumerable<SabaSport> GetSabaSports(bool isHideSaba = false);

        SabaSport GetSabaSport(int id, bool isHideSaba = false);
    }
}
