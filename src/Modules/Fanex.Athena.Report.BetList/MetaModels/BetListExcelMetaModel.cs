using Fanex.Athena.Report.Core.Models.DTOs;

namespace Fanex.Athena.Report.BetList.MetaModels
{
    public class BetListExcelMetaModel : BetListMetaModel
    {
        public ShowingOption ShowingOption { get; set; }

        public int[] HiddenColumns { get; set; }

        public int StartIndex { get; set; } = 1;

        public string ProductCode { get; set; }
    }
}
