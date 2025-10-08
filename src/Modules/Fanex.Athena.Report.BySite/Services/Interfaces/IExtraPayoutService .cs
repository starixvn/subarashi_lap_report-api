namespace Fanex.Athena.Report.BySite.Services.Interfaces
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Common.Model.ViewModel;
    using Core.Models.DTOs;
    using DTOs;
    using MetaModels;

    public interface IExtraPayoutService
    {
        Task<SortableReportResultModel<ExtraPayout>> GetDataAsync(ExtraPayoutMetaModel metaModel);

        Task<SortableReportResultModel<ExtraPayoutDetail>> GetDetailDataAsync(ExtraPayoutDetailMetaModel metaModel);

        Task<SortableReportResultModel<OddsBoostBonusDetail>> GetOddsBoostBonusDetailDataAsync(ExtraPayoutMetaModel metaModel);

        IEnumerable<CheckedItem> BuildPromotionTypeList(string productCodeList);
    }
}