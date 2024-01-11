using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;

namespace Fanex.Athena.Report.Core.ModelBinding
{
    [AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class FromBodyOrDefaultAttribute : Attribute, IBindingSourceMetadata
    {
        public BindingSource BindingSource => BodyOrDefaultBindingSource.BodyOrDefault;
    }

    public class BodyOrDefaultModelBinder : IModelBinder
    {
        private readonly IModelBinder _bodyBinder;
        private readonly IModelBinder _complexBinder;

        public BodyOrDefaultModelBinder(IModelBinder bodyBinder, IModelBinder complexBinder)
        {
            _bodyBinder = bodyBinder;
            _complexBinder = complexBinder;
        }

        public async Task BindModelAsync(ModelBindingContext bindingContext)
        {
            await _bodyBinder.BindModelAsync(bindingContext);

            if (bindingContext.Result.IsModelSet)
            {
                return;
            }
            bindingContext.ModelState.Clear();
            await _complexBinder.BindModelAsync(bindingContext);
        }
    }

    public class BodyOrDefaultBindingSource : BindingSource
    {
        public static readonly BindingSource BodyOrDefault = new BodyOrDefaultBindingSource(
            "BodyOrDefault",
            "BodyOrDefault",
            true,
            true
            );

        public BodyOrDefaultBindingSource(string id, string displayName, bool isGreedy, bool isFromRequest) : base(id, displayName, isGreedy, isFromRequest)
        {
        }

        public override bool CanAcceptDataFrom(BindingSource bindingSource)
            => bindingSource == Body || bindingSource == this;
    }

    public class BodyOrDefaultModelBinderProvider : IModelBinderProvider
    {
        private readonly IModelBinderProvider _bodyModelBinderProvider;
        private readonly IModelBinderProvider _complexDataModelBinderProvider;

        public BodyOrDefaultModelBinderProvider(BodyModelBinderProvider bodyModelBinderProvider, ComplexObjectModelBinderProvider complexDataModelBinderProvider)
        {
            _bodyModelBinderProvider = bodyModelBinderProvider;
            _complexDataModelBinderProvider = complexDataModelBinderProvider;
        }

        public IModelBinder GetBinder(ModelBinderProviderContext context)
        {
            if (context.BindingInfo.BindingSource != null && context.BindingInfo.BindingSource.CanAcceptDataFrom(BodyOrDefaultBindingSource.BodyOrDefault))
            {
                var bodyBinder = _bodyModelBinderProvider.GetBinder(context);
                var complexBinder = _complexDataModelBinderProvider.GetBinder(context);
                return new BodyOrDefaultModelBinder(bodyBinder, complexBinder);
            }
            return null;
        }
    }
}