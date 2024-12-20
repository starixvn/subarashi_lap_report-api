namespace Fanex.Athena.Report.BySite.Excels.Interfaces
{
    using Fanex.Athena.Report.BySite.MetaModels;
    using Fanex.Athena.Report.Common.Model.DTO;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public interface IDailyBetsExcel
    {
        Task<string> Export(string templatePath, DailyBetsMetaModel metaModel, IEnumerable<AuditCurrency> auditCurrencyList);
    }
}