namespace Fanex.Athena.Report.BySite.Repositories.Interfaces
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using Fanex.Athena.Report.BySite.DTOs;
    using Fanex.Athena.Report.BySite.MetaModels;

    public interface IPlayerSabaCoinsRepository
    {
        Task<IEnumerable<PlayerSabaCoins>> GetMainData(
            PlayerSabaCoinsMetaModel metaModel);

        Task<IEnumerable<PlayerSabaCoins>> GetExtraInfoData(
            PlayerSabaCoinsMetaModel metaModel);
    }
}