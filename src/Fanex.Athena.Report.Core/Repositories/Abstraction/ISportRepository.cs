namespace Fanex.Athena.Report.Core.Repositories.Abstraction
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Models.DTOs;

    public interface ISportRepository
    {
        Task<IEnumerable<SportProductItem>> GetSportsAndProducts(int licenseeId, string languageCode, string productCodes = "");

        Task<IEnumerable<SportForTreeViewItem>> GetSportsForTreeView(int licenseeId, string productCodes);
    }
}
