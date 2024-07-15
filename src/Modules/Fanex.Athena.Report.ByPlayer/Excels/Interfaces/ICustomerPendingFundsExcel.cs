namespace Fanex.Athena.Report.ByPlayer.Excels.Interfaces
{
    using Common.Model.ViewModel;
    using DTO;
    using Options;

    public interface ICustomerPendingFundsExcel
    {
        byte[] Export(string template, ReportResultModel<CustPendingFunds> data, CustomerPendingFundsExcelOptions excelOption);
    }
}