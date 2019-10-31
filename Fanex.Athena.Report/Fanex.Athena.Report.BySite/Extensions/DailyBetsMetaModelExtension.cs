using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Fanex.Athena.Report.BySite.MetaModels;

namespace Fanex.Athena.Report.BySite.Extensions
{
    public static class DailyBetsMetaModelExtension
    {
        public static string CurrencyListToXml(this DailyBetsMetaModel item)
        {
            return $"<ROOT>{string.Join("", item.CurrencyList.Select(i => $"<CR Id=\"{i.ID}\" Code=\"{i.Name}\" />"))}</ROOT>";

        }

    }
}
