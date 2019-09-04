using System;
using System.Collections.Generic;
using System.Text;
using Fanex.Athena.Report.BySite.Excels;
using Fanex.Athena.Report.BySite.Excels.Interfaces;
using Fanex.Athena.Report.BySite.Repositories;
using Fanex.Athena.Report.BySite.Repositories.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace Fanex.Athena.Report.BySite
{
    public static class ReportBySiteDIConfig
    {
        public static void Setup(IServiceCollection services)
        {
            services.AddScoped<IPlatformWinLossRepository, PlatformWinLossRepository>();
            services.AddScoped<IPlatformWinLossSummaryExcel, PlatformWinLossSummaryExcel>();
            services.AddScoped<IPlatformWinLossMemberExcel, PlatformWinLossMemberExcel>();

        }
    }
}
