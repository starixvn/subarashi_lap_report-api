namespace Fanex.Athena.Report.BySite.Repositories.Interfaces
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using Fanex.Athena.Report.BySite.MetaModels;
    using Fanex.Athena.Report.BySite.Models;

    public interface IPromotionPrizeRepository
    {
        Task<IDictionary<string, PromotionPrizeResultModel>> GetData(PromotionPrizeMetaModel metaModel);
    }
}
