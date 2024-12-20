using Fanex.Athena.Report.BySite.MetaModels;
using Fanex.Athena.Report.BySite.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Fanex.Athena.Report.BySite.Services.Interfaces
{
    public interface ISettledMatchCountService
    {
        Task<IList<SettledMatchCountResultModel>> GetDataList(SettledMatchCountMetaModel metaModel);
    }
}
