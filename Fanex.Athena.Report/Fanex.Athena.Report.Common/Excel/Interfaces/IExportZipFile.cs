using System.Threading.Tasks;

namespace Fanex.Athena.Report.Common.Excel
{
    public interface IExportZipFile<TMetaModel>
    {
        Task<string> CreateZipFile(TMetaModel metaModel);
    }
}
