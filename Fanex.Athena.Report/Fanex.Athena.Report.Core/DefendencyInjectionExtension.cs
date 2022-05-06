using Fanex.Athena.Report.Core.Services;
using Fanex.Core.Utils;
using Microsoft.Extensions.DependencyInjection;

namespace Fanex.Athena.Report.Core
{
    public static class DefendencyInjectionExtension
    {
        public static IServiceCollection AddCore(this IServiceCollection services)
        {
            services.AddSingleton<IMetaModelBindingService, BaseMetaModelBindingService>();
            services.AddSingleton<ILicenseeService, LicenseeService>();
            services.AddScoped<IContextService, ContextService>();
            services.AddSingleton<ICurrencyValidation, CurrencyValidation>();
            services.AddTransient<ISubsidiaryService, SubsidiaryService>();
            return services;
        }
    }
}
