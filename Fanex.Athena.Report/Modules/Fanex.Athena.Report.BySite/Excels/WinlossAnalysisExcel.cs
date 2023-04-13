//using Fanex.Athena.Report.BySite.DTOs.WinlossAnalysis;
//using Fanex.Athena.Report.BySite.Excels.Interfaces;
//using Fanex.Athena.Report.BySite.MetaModels;
//using Fanex.Athena.Report.Common.Excel;
//using NPOI.HSSF.UserModel;
//using NPOI.HSSF.Util;
//using NPOI.SS.UserModel;
//using NPOI.SS.UserModel.Charts;
//using NPOI.XSSF.UserModel;
//using System;
//using System.Drawing;

//namespace Fanex.Athena.Report.BySite.Excels
//{
//    public class WinlossAnalysisExcel : IWinlossAnalysisExcel
//    {
//        private const int TEMPLATE_TITLE_ROW = 0;
//        private const int TEMPLATE_HEADER_ROW = 1;
//        private const int TEMPLATE_GROUP_ROW = 2;
//        private const int TEMPLATE_DETAIL_ROW = 3;
//        private const int TEMPLATE_TOTAL_ROW = 8;



//        private readonly IExcelBuilder _builder;

//        public WinlossAnalysisExcel(IExcelBuilder builder)
//        {
//            _builder = builder;
//        }

//        public byte[] Export(string templatePath, WinlossAnalysisAggregateDTO data, WinlossAnalysisMetaModel metaModel)
//        {
//            var templateWorkbook = _builder.LoadTemplate(templatePath);
//            var workbook = _builder.CreateWorkbook();
//            SetUpStyleForWorkbook(workbook);
//            var sheet = workbook.CreateSheet("Sheet0");
//        }

//        private void SetUpStyleForWorkbook(IWorkbook workbook)
//        {
//            var templateSheet = workbook.GetSheet("Template");
//            var row = templateSheet.GetRow(TEMPLATE_GROUP_ROW);
//            row.GetCell(0).Cel

//        }

//        private void SetTitleStyle(ICellStyle style)
//        {
//        }
//    }
//}
