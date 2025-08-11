namespace Fanex.Athena.Report.ByPlayer.Excels.Interfaces
{
    using Common.Excel.Interfaces;
    using MetaModels;

    public interface ICustomerSupportTaskExcel : IExportZipFile<CustomerSupportTaskMetaModel>
    {
        bool Should(CustomerSupportTaskMetaModel metaModel);
    }
}