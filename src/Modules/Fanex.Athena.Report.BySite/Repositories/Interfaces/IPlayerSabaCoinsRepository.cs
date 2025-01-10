namespace Fanex.Athena.Report.BySite.Repositories.Interfaces
{
    using Fanex.Athena.Report.BySite.DTOs;
    using Fanex.Athena.Report.BySite.MetaModels;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public interface IPlayerSabaCoinsRepository
    {
        Task<IEnumerable<PlayerSabaCoins>> GetMainData(
            PlayerSabaCoinsMetaModel metaModel);

        Task<IEnumerable<PlayerSabaCoins>> GetExtraInfoData(
            PlayerSabaCoinsMetaModel metaModel);
    }
}