using System.Collections.Generic;
using System.Threading.Tasks;
using Fanex.Athena.Report.BySite.MetaModels;
using Fanex.Athena.Report.BySite.Models;

namespace Fanex.Athena.Report.BySite.Services.Interfaces
{
    public interface ISettledMatchCountService
    {
        Task<IList<SettledMatchCountResultModel>> GetDataList(SettledMatchCountMetaModel metaModel);
    }
}
