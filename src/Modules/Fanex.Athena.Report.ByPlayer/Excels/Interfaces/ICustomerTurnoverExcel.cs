namespace Fanex.Athena.Report.ByPlayer.Excels.Interfaces
{
    using System.Collections.Generic;
    using Common.Model.MetaModels;
    using Common.Model.ViewModel;
    using DTO;
    using MetaModels;
    using Options;

    public interface ICustomerTurnoverExcel
    {
        public byte[] ExportFile(string template, ReportResultModel<CustTurnover> data, CustomerTurnoverMetaModel metaModel, CustomerTurnoverExcelOptions excelOption);

        public List<ZipEntry> ExportZip(string template, IEnumerable<ReportResultModel<CustTurnover>> dataList, CustomerTurnoverMetaModel metaModel, CustomerTurnoverExcelOptions excelOption);
    }
}
