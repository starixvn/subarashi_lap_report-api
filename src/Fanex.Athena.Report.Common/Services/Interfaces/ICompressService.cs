namespace Fanex.Athena.Report.Common.Services.Interfaces
{
    using System.Collections.Generic;
    using Model.MetaModels;

    public interface ICompressService
    {
        string EnsureFolderPath();

        byte[] CompressDataInByteFormat(IEnumerable<ZipEntry> entries, bool isDeleteOriginalAfterCompress);

        string CompressDataToFile(IEnumerable<ZipEntry> entries, bool isDeleteOriginalAfterCompress);
    }
}