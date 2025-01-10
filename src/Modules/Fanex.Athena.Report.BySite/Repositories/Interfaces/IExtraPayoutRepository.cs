namespace Fanex.Athena.Report.BySite.Repositories.Interfaces
{
    using Fanex.Athena.Report.BySite.DTOs;
    using Fanex.Athena.Report.BySite.MetaModels;
    using Fanex.Athena.Report.Common.Model.ViewModel;
    using System.Threading.Tasks;

    public interface IExtraPayoutRepository
    {
        Task<SortableReportResultModel<ExtraPayout>> GetExtraPayoutMainData(
            ExtraPayoutMetaModel metaModel);

        Task<SortableReportResultModel<ExtraPayout>> GetExtraPayoutMemberData(
            ExtraPayoutMetaModel metaModel);

        Task<SortableReportResultModel<ExtraPayoutDetail>> GetExtraPayoutDetailData(
            ExtraPayoutDetailMetaModel metaModel);
    }
}