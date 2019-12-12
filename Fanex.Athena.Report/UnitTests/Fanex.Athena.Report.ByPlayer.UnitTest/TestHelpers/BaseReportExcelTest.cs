using System;
using System.IO;

namespace Fanex.Athena.Report.ByPlayer.UnitTests.TestHelpers
{
    public class BaseReportExcelTest
    {
        protected BaseReportExcelTest()
        {
        }

        protected string CreateFile()
        {
            var fileName = $"{Guid.NewGuid()}.dat";
            File.Create(fileName).Dispose();

            return fileName;
        }

        protected byte[] GetStreamContent(Stream fileStream)
        {
            using (var memoryStream = new MemoryStream())
            {
                fileStream.CopyTo(memoryStream);
                return memoryStream.ToArray();
            }
        }
    }
}