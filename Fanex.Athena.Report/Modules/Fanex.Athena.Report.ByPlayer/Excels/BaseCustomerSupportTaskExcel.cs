using Fanex.Athena.Report.ByPlayer.DTO;
using Fanex.Athena.Report.ByPlayer.MetaModels;
using Fanex.Athena.Report.Common.Excel;
using Fanex.Athena.Report.Common.Extensions;
using Fanex.Athena.Report.Common.Model.MetaModels;
using Fanex.Athena.Report.Common.Services.Interfaces;
using Fanex.Athena.Report.Resources;
using Fanex.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Fanex.Athena.Report.ByPlayer.Excels
{
    public abstract class BaseCustomerSupportTaskExcel<TData> : BaseExcel, IExportZipFile<CustomerSupportTaskMetaModel>
        where TData : CustomerSupportTask, new()
    {
        private const int TemplateSheetIndex = 0;
        private const int TemplateTitleRowIndex = 1;

        private const string SheetName = "Data";

        private readonly IFileProvider _fileProvider;

        protected BaseCustomerSupportTaskExcel(
            IFileProvider fileProvider,
            IFileService fileService,
            IOptions<ExcelOptions> excelOptionsAccessor,
            IWebHostEnvironment enviroment) : base(enviroment, fileService, excelOptionsAccessor)
        {
            _fileProvider = fileProvider;
        }

        protected abstract int ColumnTemplateCount { get; }
        protected abstract int TemplateNoInfoRowIndex { get; }
        protected abstract int TemplateDataRowIndex { get; }
        protected abstract string TemplatePath { get; }
        protected abstract string UspName { get; }

        protected abstract string GetFileName(CustomerSupportTaskMetaModel metaModel);
        protected abstract Task AddParameters(IObjectDb db, CustomerSupportTaskMetaModel metaModel);
        protected abstract void AssignHeader(ExcelWorksheet sheet);
        protected abstract void AssignBody(ExcelWorksheet sheet, IEnumerable<TData> dataList);
        protected abstract string BuildTitle(CustomerSupportTaskMetaModel metaModel);
        protected abstract void BuildDataRecord(IDataReader reader, TData record);

        private ExcelPackage _package;
        protected ExcelPackage Package
        {
            get 
            {
                if(_package == null)
                {
                    var physicalPath = _fileProvider.GetFileInfo(TemplatePath).PhysicalPath;
                    var fileInfo = new FileInfo(physicalPath);
                    _package = new ExcelPackage(fileInfo, false);
                }

                return _package;
            }

            set
            {
                _package = value;
            }
        }

        protected ExcelWorksheet TemplateSheet => Package.Workbook.Worksheets[TemplateSheetIndex];

        public async Task<string> CreateZipFile(CustomerSupportTaskMetaModel metaModel)
        {
            var sourcePath = await CreateFile(metaModel);
            return ZipExcelFile(new List<ZipEntry> { new ZipEntry { Path = sourcePath, FileName = GetFileName(metaModel) } }, EnsureFolderPath());
        }

        private void BatchExport(int batchIndex, IEnumerable<TData> batchResult, CustomerSupportTaskMetaModel metaModel)
        {
            var currentSheet = Package.Workbook.Worksheets.Add($"{SheetName}{(batchIndex)}", TemplateSheet);

            AssignTitle(currentSheet, metaModel);
            AssignHeader(currentSheet);

            if(batchResult.Any())
            {
                currentSheet.DeleteRow(TemplateNoInfoRowIndex, 1);
                AssignBody(currentSheet, batchResult);
            }
            else
            {
                AssignNoInfoRow(currentSheet);
                currentSheet.DeleteRow(TemplateDataRowIndex, 1, true);
            }
        }

        private async Task<string> CreateFile(CustomerSupportTaskMetaModel metaModel)
        {
            await BuildWorkSheets(metaModel);
            var fileName = SaveAsWorksheetsToFile();
            return fileName;
        }

        private string SaveAsWorksheetsToFile()
        {
            Package.Workbook.Worksheets.Delete(TemplateSheetIndex);
            var fileName = Path.Combine(EnsureFolderPath(), $"{Guid.NewGuid()}.eplus");
            using (var stream = new FileStream(fileName, FileMode.Create, FileAccess.ReadWrite))
            {
                Package.SaveAs(stream);
                Package.Dispose();
                Package = null;
            }

            return fileName;
        }

        private async Task BuildWorkSheets(CustomerSupportTaskMetaModel metaModel)
        {
            using (var db = ObjectDbFactory.CreateInstance(UspName))
            {
                await AddParameters(db, metaModel);
                using (var reader = await db.ExecuteReaderAsync())
                {
                    var hasData = false;
                    var index = 1;
                    var batchIndex = 1;
                    var batchResult = new LinkedList<TData>();
                    while (reader.Read())
                    {
                        var record = new TData
                        {
                            Index = index++,
                            ExternalId = reader.GetVal<string>("externalid"),
                            SystemId = reader.GetVal<string>("systemid"),
                            Currency = reader.GetVal<string>("currencyname")
                        };

                        BuildDataRecord(reader, record);

                        batchResult.AddLast(record);

                        if (batchResult.Count == ExcelOptions.MaximumRowPerFile)
                        {
                            BatchExport(batchIndex, batchResult, metaModel);
                            batchResult.Clear();
                            batchIndex++;
                        }

                        hasData = true;
                    }

                    if (hasData && !batchResult.Any())
                    {
                        return;
                    }

                    BatchExport(batchIndex, batchResult, metaModel);
                    batchResult.Clear();
                }
            }
        }


        private void AssignNoInfoRow(ExcelWorksheet sheet)
        {
            sheet.SetValue(TemplateNoInfoRowIndex, 1, ViewResources.NoInfo);
            var noInfoRow = sheet.Cells[TemplateNoInfoRowIndex, 1, TemplateNoInfoRowIndex, ColumnTemplateCount];
            noInfoRow.Merge = true;
        }

        private void AssignTitle(ExcelWorksheet sheet, CustomerSupportTaskMetaModel metaModel)
        {
            var destinationCell = sheet.Cells[TemplateTitleRowIndex, 1, TemplateTitleRowIndex, ColumnTemplateCount];
            destinationCell.Merge = true;
            sheet.SetValue(TemplateTitleRowIndex, 1, BuildTitle(metaModel));
        }
    }
}
