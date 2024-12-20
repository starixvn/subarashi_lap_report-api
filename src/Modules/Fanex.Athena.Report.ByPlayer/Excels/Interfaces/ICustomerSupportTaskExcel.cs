namespace Fanex.Athena.Report.ByPlayer.Excels.Interfaces
{
    using Fanex.Athena.Report.ByPlayer.MetaModels;
    using Fanex.Athena.Report.Common.Excel.Interfaces;

    public interface ICustomerSupportTaskExcel : IExportZipFile<CustomerSupportTaskMetaModel>
    {
        bool Should(CustomerSupportTaskMetaModel metaModel);
    }
}