namespace Fanex.Athena.Report.Common.Excel.Interfaces
{
    using System.Threading.Tasks;

    public interface IExportZipFile<TMetaModel>
    {
        Task<string> CreateZipFile(TMetaModel metaModel);
    }
}