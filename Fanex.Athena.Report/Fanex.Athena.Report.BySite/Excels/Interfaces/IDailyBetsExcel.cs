namespace Fanex.Athena.Report.BySite.Excels.Interfaces
{
    using System.Threading.Tasks;
    using Fanex.Athena.Report.BySite.MetaModels;

    public interface IDailyBetsExcel
    {
        Task<string> Export(DailyBetsMetaModel metaModel);
    }
}
