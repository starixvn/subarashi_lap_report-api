namespace Fanex.Athena.Report.BySite.Repositories.Interfaces
{
    using System.Threading.Tasks;
    using Fanex.Athena.Report.BySite.DTOs;
    using Fanex.Athena.Report.BySite.MetaModels;
    using Fanex.Athena.Report.BySite.Models;

    public interface IPlatfromWinLossRepository
    {
        Task<ResultModel<PlatformWinLossMember>> GetMemberData(PlatformWinLossMetaModel metaModel);

        Task<ResultModel<PlatformWinLossSummary>> GetSummaryData(PlatformWinLossMetaModel metaModel);
    }
}
