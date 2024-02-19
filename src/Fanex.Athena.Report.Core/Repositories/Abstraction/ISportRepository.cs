using System.Collections.Generic;
using System.Threading.Tasks;
using Fanex.Athena.Report.Core.Models.DTOs;

namespace Fanex.Athena.Report.Core.Repositories.Abstraction
{
    public interface ISportRepository
    {
        Task<IEnumerable<SportProductItem>> GetSportsAndProducts(int licenseeId, string languageCode, string productCodes = "");

        Task<IEnumerable<SportForTreeViewItem>> GetSportsForTreeView(int licenseeId, string productCodes);
    }
}
