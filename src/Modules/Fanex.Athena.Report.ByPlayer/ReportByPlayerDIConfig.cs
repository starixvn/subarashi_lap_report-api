namespace Fanex.Athena.Report.ByPlayer
{
    using Excels;
    using Excels.Interfaces;
    using Microsoft.Extensions.DependencyInjection;
    using Repository;
    using Repository.Interfaces;
    using Service;
    using Service.Interfaces;

    public static class ReportByPlayerDIConfig
    {
        public static IServiceCollection AddByPlayerModule(this IServiceCollection services)
        {
            services.AddTransient<ICustomerWinLossService, CustomerWinLossService>();
            services.AddTransient<ICustomerWinLossExcel, CustomerWinLossExcel>();
            services.AddTransient<ICustomerBalanceService, CustomerBalanceService>();
            services.AddTransient<ICustomerBalanceExcel, CustomerBalanceExcel>();
            services.AddTransient<ICustomerSupportTaskExcel, CustomerClosingBalanceExcel>();
            services.AddTransient<ICustomerSupportTaskExcel, CustomerRunningTicketExcel>();
            services.AddTransient<ISearchCustomerService, SearchCustomerService>();
            services.AddTransient<ICustomerTurnoverExcel, CustomerTurnoverExcel>();
            services.AddTransient<ICustomerTurnoverService, CustomerTurnoverService>();
            services.AddTransient<ICustomerTurnoverRepository, CustomerTurnoverRepository>();
            return services;
        }
    }
}