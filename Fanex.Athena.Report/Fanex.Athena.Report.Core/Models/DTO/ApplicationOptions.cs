namespace Fanex.Athena.Report.Core.DTO
{
    using System.Collections.Generic;

    public class ApplicationOptions
    {
        public IList<string> WhiteList { get; set; }

        public string AuthKey { get; set; }
    }
}
