namespace Fanex.Athena.Report.BySite.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Core.Attributes;
    using Core.Constants;
    using DTOs;
    using Excels.Interfaces;
    using Excels.Options;
    using MetaModels;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.FileProviders;
    using Models;
    using Services.Interfaces;

    [ApiController, Route("api/[controller]/[action]"),
     BearerAuthorize(Roles.PortalSuperAdmin, Roles.WeeklyLiveMatches),]
    public class WeeklyLiveMatchController : Controller
    {
        [HttpPost]
        public async Task<IActionResult> Index(
            [FromServices] IWeeklyLiveMatchService service,
            WeeklyLiveMatchMetaModel metaModel
        )
        {
            var result = await service.GetData(metaModel);

            if (!result.DataList.Any())
            {
                return Ok(new WeeklyLiveMatchResponseModel
                {
                    WeekHeaders = new List<string>(),
                    NumberOfWeeks = 0,
                    Data = Array.Empty<CellModel>(),
                    WeeksTotalData = Array.Empty<int>()
                });
            }

            var weekHeaders = service.GetWeekHeaders(metaModel.FromDate, metaModel.ToDate, result.NumberOfWeeks);
            return Ok(new WeeklyLiveMatchResponseModel
            {
                WeekHeaders = weekHeaders,
                NumberOfWeeks = result.NumberOfWeeks,
                Data = result.DataList.Select(MapToGridView).ToArray(),
                WeeksTotalData = result.WeeksTotalData
            });
        }

        private static CellModel MapToGridView(WeeklyLiveMatch item)
        {
            var data = new List<object> { item.Index, item.SportName, };
            data.AddRange(item.NumbersOfMatches.Cast<object>());
            return new CellModel
            {
                Cell = data
            };
        }

        [HttpPost]
        public async Task<FileContentResult> ExportExcel(
            [FromServices] IFileProvider file,
            [FromServices] IWeeklyLiveMatchExcel excel,
            [FromServices] IWeeklyLiveMatchService service,
            WeeklyLiveMatchMetaModel metaModel)
        {
            var template = file.GetFileInfo("/ExcelTemplate/WeeklyLiveMatch.xls").PhysicalPath;

            var result = await service.GetData(metaModel).ConfigureAwait(false);
            var weekHeaders = service.GetWeekHeaders(metaModel.FromDate, metaModel.ToDate, result.NumberOfWeeks)
                                     .ToList();
            var options = new WeeklyLiveMatchOptions
            {
                MetaModel = metaModel,
                WeekHeaders = weekHeaders
            };

            var fileContent = excel.Export(template, result, options);

            return new FileContentResult(fileContent, "application/octet-stream");
        }
    }
}
