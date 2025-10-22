namespace Fanex.Athena.Report.Common.UnitTests.Excel
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using Common.Excel;
    using Moq;
    using NPOI.HSSF.UserModel;
    using NPOI.SS.UserModel;
    using NPOI.SS.Util;
    using Xunit;

    public class ExcelBuilderTest
    {
        [Fact]
        public void LoadTemplate_WhenCall_ReceiveWorkbook()
        {
            // Arrange
            var path = CreateFile();

            try
            {
                var excelBuilder = new ExcelBuilder();

                // Action
                var workbook = excelBuilder.LoadTemplate(path);

                // Assert
                Assert.NotNull(workbook);
            }
            finally
            {
                File.Delete(path);
            }
        }

        private string CreateFile()
        {
            var rootPath = Directory.GetCurrentDirectory();
            var fileName = Path.Combine(rootPath, $"{Guid.NewGuid()}.xls");

            var workbook = new HSSFWorkbook();
            workbook.CreateSheet("Template");
            using (var fileData = new FileStream(fileName, FileMode.Create))
            {
                workbook.Write(fileData);
            }

            return fileName;
        }

        [Fact]
        public void CloneSheet_WhenCall_ReturnCorrectSheet()
        {
            // Arrange
            const string SourceSheetName = "Source";
            const int SourceSheetIndex = 0;
            const string NewSheetName = "NewSheetName";
            const int NewSheetIndex = 1;

            var workbook = new Mock<IWorkbook>();
            workbook.Setup(w => w.GetSheetIndex(SourceSheetName)).Returns(SourceSheetIndex);

            var newSheet = new Mock<ISheet>();
            workbook.Setup(w => w.CloneSheet(SourceSheetIndex)).Returns(newSheet.Object);
            workbook.Setup(w => w.GetSheetIndex(newSheet.Object)).Returns(NewSheetIndex);

            var excelBuilder = new ExcelBuilder();

            // Action
            var result = excelBuilder.CloneSheet(workbook.Object, SourceSheetName, NewSheetName);

            // Assert
            Assert.Equal(newSheet.Object, result);
            workbook.Verify(w => w.SetSheetName(NewSheetIndex, NewSheetName), Times.Once);
        }

        [Fact]
        public void RemoveSheet_WhenCall_CallRemoveMethod()
        {
            // Arrange
            const string SheetName = "SheetName";
            const int SheetIndex = 0;
            var sheet = new Mock<ISheet>();
            var workbook = new Mock<IWorkbook>();
            workbook.Setup(w => w.GetSheet(SheetName)).Returns(sheet.Object);
            workbook.Setup(w => w.GetSheetIndex(sheet.Object)).Returns(SheetIndex);

            var excelBuilder = new ExcelBuilder();

            // Action
            excelBuilder.RemoveSheet(workbook.Object, SheetName);

            // Assert
            workbook.Verify(w => w.RemoveSheetAt(SheetIndex), Times.Once);
        }

        [Fact]
        public void SetRowValue_WhenCall_CellIsSetValueCorrectly()
        {
            // Arrange
            var sheet = new Mock<ISheet>();
            var sourceSheet = new Mock<ISheet>();

            const int SourceRowIndex = 1;
            const int DestRowIndex = 2;
            const int ColumnCount = 3;
            const string Value = "Value";

            var oldRow = new Mock<IRow>();
            sourceSheet.Setup(s => s.GetRow(SourceRowIndex)).Returns(oldRow.Object);

            var newRow = new Mock<IRow>();
            sheet.Setup(s => s.GetRow(DestRowIndex)).Returns(newRow.Object);

            var cell = new Mock<ICell>();
            var cellList = new List<ICell> { cell.Object };
            newRow.Setup(r => r.Cells).Returns(cellList);

            var excelBuilder = new ExcelBuilder();

            // Action
            excelBuilder.SetRowValue(sheet.Object, sourceSheet.Object, SourceRowIndex, DestRowIndex, ColumnCount, Value);

            // Assert
            cell.Verify(c => c.SetCellValue(Value), Times.Once);
        }

        [Fact]
        public void ClearTemplate_WhenCall_DeleteAllRows()
        {
            // Arrange
            var sheet = new Mock<ISheet>();
            const int LastRowNum = 4;
            const int StartRowIndex = 0;
            const int ExpectedCallCount = 5;

            sheet.Setup(s => s.LastRowNum).Returns(LastRowNum);

            var row = new Mock<IRow>();
            sheet.Setup(s => s.GetRow(It.IsAny<int>())).Returns(row.Object);

            var excelBuilder = new ExcelBuilder();

            // Action
            excelBuilder.ClearTemplate(sheet.Object, StartRowIndex);

            // Assert
            sheet.Verify(s => s.RemoveRow(It.IsAny<IRow>()), Times.Exactly(ExpectedCallCount));
        }

        [Fact]
        public void MergeCells_WhenNoExistingMergedRegions_PassRangeValueCorrectly()
        {
            // Arrange
            var sheet = new Mock<ISheet>();
            const int FirstRow = 1;
            const int LastRow = 2;
            const int FirstCol = 5;
            const int LastCol = 7;
            const int ExpectedReturnValue = 0;

            sheet.Setup(s => s.NumMergedRegions).Returns(0);
            sheet.Setup(s => s.AddMergedRegion(It.IsAny<CellRangeAddress>())).Returns(ExpectedReturnValue);

            CellRangeAddress passedRange = null;
            sheet.Setup(s => s.AddMergedRegion(It.IsAny<CellRangeAddress>()))
                .Callback<CellRangeAddress>(x => passedRange = x)
                .Returns(ExpectedReturnValue);

            var excelBuilder = new ExcelBuilder();

            // Action
            var result = excelBuilder.MergeCells(sheet.Object, FirstRow, LastRow, FirstCol, LastCol);

            // Assert
            Assert.Equal(ExpectedReturnValue, result);
            Assert.Equal(FirstRow, passedRange.FirstRow);
            Assert.Equal(LastRow, passedRange.LastRow);
            Assert.Equal(FirstCol, passedRange.FirstColumn);
            Assert.Equal(LastCol, passedRange.LastColumn);
        }

        [Fact]
        public void MergeCells_WhenRegionIntersectsWithExisting_ReturnMinusOne()
        {
            // Arrange
            var sheet = new Mock<ISheet>();
            const int FirstRow = 1;
            const int LastRow = 2;
            const int FirstCol = 1;
            const int LastCol = 2;

            var existingRegion = new CellRangeAddress(1, 2, 1, 2);
            sheet.Setup(s => s.NumMergedRegions).Returns(1);
            sheet.Setup(s => s.GetMergedRegion(0)).Returns(existingRegion);

            var excelBuilder = new ExcelBuilder();

            // Action
            var result = excelBuilder.MergeCells(sheet.Object, FirstRow, LastRow, FirstCol, LastCol);

            // Assert
            Assert.Equal(-1, result);
            sheet.Verify(s => s.AddMergedRegion(It.IsAny<CellRangeAddress>()), Times.Never);
        }

        [Fact]
        public void MergeRows_WhenNoExistingMergedRegions_MergeSuccessfully()
        {
            // Arrange
            var sheet = new Mock<ISheet>();
            const int FirstRow = 1;
            const int LastRow = 3;
            const int ColIndex = 2;
            const int ExpectedReturnValue = 0;

            sheet.Setup(s => s.NumMergedRegions).Returns(0);
            sheet.Setup(s => s.AddMergedRegion(It.IsAny<CellRangeAddress>())).Returns(ExpectedReturnValue);

            var excelBuilder = new ExcelBuilder();

            // Action
            var result = excelBuilder.MergeRows(sheet.Object, FirstRow, LastRow, ColIndex);

            // Assert
            Assert.Equal(ExpectedReturnValue, result);
            sheet.Verify(s => s.AddMergedRegion(It.IsAny<CellRangeAddress>()), Times.Once);
        }

        [Fact]
        public void MergeRows_WhenRegionIntersectsWithExisting_ReturnMinusOne()
        {
            // Arrange
            var sheet = new Mock<ISheet>();
            const int FirstRow = 1;
            const int LastRow = 3;
            const int ColIndex = 2;

            var existingRegion = new CellRangeAddress(2, 4, 2, 2);
            sheet.Setup(s => s.NumMergedRegions).Returns(1);
            sheet.Setup(s => s.GetMergedRegion(0)).Returns(existingRegion);

            var excelBuilder = new ExcelBuilder();

            // Action
            var result = excelBuilder.MergeRows(sheet.Object, FirstRow, LastRow, ColIndex);

            // Assert
            Assert.Equal(-1, result);
            sheet.Verify(s => s.AddMergedRegion(It.IsAny<CellRangeAddress>()), Times.Never);
        }

        [Fact]
        public void MergeCols_WhenNoExistingMergedRegions_MergeSuccessfully()
        {
            // Arrange
            var sheet = new Mock<ISheet>();
            const int FirstCol = 1;
            const int LastCol = 3;
            const int RowIndex = 2;
            const int ExpectedReturnValue = 0;

            sheet.Setup(s => s.NumMergedRegions).Returns(0);
            sheet.Setup(s => s.AddMergedRegion(It.IsAny<CellRangeAddress>())).Returns(ExpectedReturnValue);

            var excelBuilder = new ExcelBuilder();

            // Action
            var result = excelBuilder.MergeCols(sheet.Object, FirstCol, LastCol, RowIndex);

            // Assert
            Assert.Equal(ExpectedReturnValue, result);
            sheet.Verify(s => s.AddMergedRegion(It.IsAny<CellRangeAddress>()), Times.Once);
        }

        [Fact]
        public void MergeCols_WhenRegionIntersectsWithExisting_ReturnMinusOne()
        {
            // Arrange
            var sheet = new Mock<ISheet>();
            const int FirstCol = 1;
            const int LastCol = 3;
            const int RowIndex = 2;

            var existingRegion = new CellRangeAddress(2, 2, 2, 4);
            sheet.Setup(s => s.NumMergedRegions).Returns(1);
            sheet.Setup(s => s.GetMergedRegion(0)).Returns(existingRegion);

            var excelBuilder = new ExcelBuilder();

            // Action
            var result = excelBuilder.MergeCols(sheet.Object, FirstCol, LastCol, RowIndex);

            // Assert
            Assert.Equal(-1, result);
            sheet.Verify(s => s.AddMergedRegion(It.IsAny<CellRangeAddress>()), Times.Never);
        }

        [Fact]
        public void HideRow_WhenCall_RowHeightIsZeroAndCellIsSetEmptyValue()
        {
            // Arrange
            var sheet = new Mock<ISheet>();
            const int RowIndex = 1;
            const int NumberOfColumn = 255;
            var row = new Mock<IRow>();
            var cell = new Mock<ICell>();
            row.Setup(r => r.GetCell(It.IsAny<int>())).Returns(cell.Object);
            sheet.Setup(s => s.GetRow(RowIndex)).Returns(row.Object);

            var excelBuilder = new ExcelBuilder();

            // Action
            excelBuilder.HideRow(sheet.Object, RowIndex);

            // Assert
            cell.Verify(c => c.SetCellValue(string.Empty), Times.Exactly(NumberOfColumn));
            row.VerifySet(r => r.ZeroHeight = true);
        }

        [Fact]
        public void HideColumn_WhenCall_SetColumnHiddenAndWidthToZero()
        {
            // Arrange
            var sheet = new Mock<ISheet>();
            const int NumberOfRow = 1;
            sheet.Setup(s => s.FirstRowNum).Returns(NumberOfRow);
            sheet.Setup(s => s.LastRowNum).Returns(NumberOfRow);

            const int RowIndex = 1;
            const int ColumnIndex = 1;
            var row = new Mock<IRow>();
            var cell = new Mock<ICell>();
            row.Setup(r => r.GetCell(ColumnIndex)).Returns(cell.Object);
            sheet.Setup(s => s.GetRow(RowIndex)).Returns(row.Object);

            var excelBuilder = new ExcelBuilder();

            // Action
            excelBuilder.HideColumn(sheet.Object, ColumnIndex);

            // Assert
            cell.Verify(c => c.SetCellValue(string.Empty), Times.Once);
            sheet.Verify(r => r.SetColumnWidth(ColumnIndex, 0), Times.Once);
            sheet.Verify(r => r.SetColumnHidden(ColumnIndex, true), Times.Once);
        }

        [Fact]
        public void MergeCells_WhenSingleCellRange_ReturnMinusOne()
        {
            // Arrange
            var sheet = new Mock<ISheet>();
            const int RowIndex = 1;
            const int ColIndex = 2;

            sheet.Setup(s => s.NumMergedRegions).Returns(0);

            var excelBuilder = new ExcelBuilder();

            // Action
            var result = excelBuilder.MergeCells(sheet.Object, RowIndex, RowIndex, ColIndex, ColIndex);

            // Assert
            Assert.Equal(-1, result);
            sheet.Verify(s => s.AddMergedRegion(It.IsAny<CellRangeAddress>()), Times.Never);
        }

        [Fact]
        public void MergeRows_WhenSingleCellRange_ReturnMinusOne()
        {
            // Arrange
            var sheet = new Mock<ISheet>();
            const int RowIndex = 1;
            const int ColIndex = 2;

            sheet.Setup(s => s.NumMergedRegions).Returns(0);

            var excelBuilder = new ExcelBuilder();

            // Action
            var result = excelBuilder.MergeRows(sheet.Object, RowIndex, RowIndex, ColIndex);

            // Assert
            Assert.Equal(-1, result);
            sheet.Verify(s => s.AddMergedRegion(It.IsAny<CellRangeAddress>()), Times.Never);
        }

        [Fact]
        public void MergeCols_WhenSingleCellRange_ReturnMinusOne()
        {
            // Arrange
            var sheet = new Mock<ISheet>();
            const int RowIndex = 1;
            const int ColIndex = 2;

            sheet.Setup(s => s.NumMergedRegions).Returns(0);

            var excelBuilder = new ExcelBuilder();

            // Action
            var result = excelBuilder.MergeCols(sheet.Object, ColIndex, ColIndex, RowIndex);

            // Assert
            Assert.Equal(-1, result);
            sheet.Verify(s => s.AddMergedRegion(It.IsAny<CellRangeAddress>()), Times.Never);
        }
    }
}