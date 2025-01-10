using Fanex.Athena.Report.ByPlayer.DTO;
using Fanex.Athena.Report.ByPlayer.Excels.Options;
using Fanex.Athena.Report.ByPlayer.MetaModels;
using Fanex.Athena.Report.Common.Model.MetaModels;
using Fanex.Athena.Report.Common.Model.ViewModel;
using System.Collections.Generic;

namespace Fanex.Athena.Report.ByPlayer.Excels.Interfaces
{
    public interface ICustomerTurnoverExcel
    {
        public byte[] ExportFile(string template, ReportResultModel<CustTurnover> data, CustomerTurnoverMetaModel metaModel, CustomerTurnoverExcelOptions excelOption);

        public List<ZipEntry> ExportZip(string template, IEnumerable<ReportResultModel<CustTurnover>> dataList, CustomerTurnoverMetaModel metaModel, CustomerTurnoverExcelOptions excelOption);
    }
}
