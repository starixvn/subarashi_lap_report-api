using System.Collections.Generic;
using Fanex.Athena.Report.Common.Model.MetaModels;

namespace Fanex.Athena.Report.Common.Services.Interfaces
{
    public interface ICompressService
    {
        string EnsureFolderPath();

        byte[] CompressCompressDataInByteFormssData(IEnumerable<ZipEntry> entries, bool isDeleteOriginalAfterCompress);

        string CompressDataToFile(IEnumerable<ZipEntry> entries, bool isDeleteOriginalAfterCompress);
    }
}