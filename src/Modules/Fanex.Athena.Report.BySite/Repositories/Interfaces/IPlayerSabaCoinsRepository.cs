namespace Fanex.Athena.Report.BySite.Repositories.Interfaces
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using DTOs;
    using MetaModels;

    public interface IPlayerSabaCoinsRepository
    {
        Task<IEnumerable<PlayerSabaCoinsMain>> GetMainData(
            PlayerSabaCoinsMetaModel metaModel);

        Task<IEnumerable<PlayerSabaCoinsExtraInfo>> GetExtraInfoData(
            PlayerSabaCoinsMetaModel metaModel);
    }
}