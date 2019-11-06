namespace Fanex.Athena.Report.Core.DTO
{
    public class ExcelOptions
    {
        public int MaximumRowPerFile { get; set; }

        public string Path { get; set; }

        public string CSVSeparateCharacter { get; set; }

        public string CSVTextDelimiter { get; set; }
    }
}
