using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using System.Collections.Generic;
using System.Linq;

namespace Fanex.Athena.Report.Core.ModelBinding
{
    public static class ModelBinderProviderExtensions
    {
        public static void AddBodyOrDefaultBinding(this IList<IModelBinderProvider> providers)
        {
            var bodyProvider = providers.Single(provider => provider.GetType() == typeof(BodyModelBinderProvider)) as BodyModelBinderProvider;
            var complexDataProvider = providers.OfType<ComplexObjectModelBinderProvider>().Single();
            var bodyOrDefault = new BodyOrDefaultModelBinderProvider(bodyProvider, complexDataProvider);
            providers.Insert(0, bodyOrDefault);
        }
    }
}