namespace Fanex.Athena.Report.BySite.Services.Interfaces
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using MetaModels;
    using Models;

    public interface ISettledMatchCountService
    {
        Task<IList<SettledMatchCountResultModel>> GetDataList(SettledMatchCountMetaModel metaModel);
    }
}
