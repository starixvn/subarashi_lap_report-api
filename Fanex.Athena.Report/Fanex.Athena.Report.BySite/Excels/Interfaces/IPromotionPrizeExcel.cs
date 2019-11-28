namespace Fanex.Athena.Report.BySite.Excels.Interfaces
{
    using System.Collections.Generic;
    using Fanex.Athena.Report.BySite.MetaModels;
    using Fanex.Athena.Report.BySite.Models;
    using NPOI.SS.UserModel;

    public interface IPromotionPrizeExcel
    {
        IWorkbook Export(string template, IDictionary<string, PromotionPrizeResultModel> data, PromotionPrizeMetaModel metaModel);
    }
}
