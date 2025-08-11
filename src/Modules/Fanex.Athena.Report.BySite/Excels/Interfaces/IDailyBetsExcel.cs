namespace Fanex.Athena.Report.BySite.Excels.Interfaces
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Common.Model.DTO;
    using MetaModels;

    public interface IDailyBetsExcel
    {
        Task<string> Export(string templatePath, DailyBetsMetaModel metaModel, IEnumerable<AuditCurrency> auditCurrencyList);
    }
}