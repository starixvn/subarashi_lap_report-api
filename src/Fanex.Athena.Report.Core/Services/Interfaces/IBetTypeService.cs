using System.Collections.Generic;
using System.Threading.Tasks;
using Fanex.Athena.Report.Core.Models.DTOs;

namespace Fanex.Athena.Report.Core.Services.Interfaces
{
    public interface IBetTypeService
    {
        Task<IList<CheckedItem>> GetBetTypesByProduct(int licenseeId, string productCode, string languageCode, bool isHideSaba = false);
    }
}
