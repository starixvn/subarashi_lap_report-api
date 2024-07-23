using System.Collections.Generic;
using Fanex.Athena.Report.BySite.DTOs;
using Fanex.Athena.Report.BySite.MetaModels;
using Fanex.Athena.Report.Common.Excel.Interfaces;
using Fanex.Athena.Report.Common.Extensions;
using Fanex.Athena.Report.Core.Models.DTOs;

namespace Fanex.Athena.Report.BySite.Excels.Options
{
    public class SabaVirtualSportsWinLossExcelOption : IConvertingOption
    {
        public string BaseCurrency { get; set; }

        public string CurrencyName { get; set; }

        public bool IsConvert { get; set; }

        public bool SbVisible { get; set; }

        public bool BaVisible { get; set; }

        public bool CoVisible { get; set; }

        public bool CustomerCommVisible { get; set; }
    }
}
