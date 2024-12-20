using Fanex.Athena.Report.Core.Services.Interfaces;
using Fanex.Core.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Fanex.Athena.Report.Core.Attributes
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false)]
    public class RequestValidationAttribute : TypeFilterAttribute
    {
        public RequestValidationAttribute(
            bool validateCurrency = false,
            bool validateCurrencyList = false)
            : base(typeof(DataValidationImplAttribute))
        {
            Arguments = new object[] { validateCurrency, validateCurrencyList };
        }
    }

    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false)]
    public class DataValidationImplAttribute : ActionFilterAttribute
    {
        private const string ErrorMsg = "Invalid Params";

        private readonly IContextService _contextService;
        private readonly ICurrencyValidation _currencyValidation;

        private readonly bool _validateCurrency;
        private readonly bool _validateCurrencyList;

        public DataValidationImplAttribute(IContextService contextService, ICurrencyValidation currencyValidation,
            bool validateCurrency,
            bool validateCurrencyList
            )
        {
            _contextService = contextService;
            _currencyValidation = currencyValidation;

            _validateCurrency = validateCurrency;
            _validateCurrencyList = validateCurrencyList;
        }

        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            if (context.ActionDescriptor.EndpointMetadata.OfType<SkipRequestValidationAttribute>().Any())
            {
                await next();
                return;
            }

            var isValid = IsValid(context.ActionArguments);
            if (!isValid)
            {
                context.Result = new JsonResult(new { Msg = ErrorMsg, IsSuccess = false })
                {
                    StatusCode = (int)HttpStatusCode.BadRequest
                };
            }
            else
            {
                await next();
            }
        }

        private bool IsValid(IDictionary<string, object> actionArguments)
        {
            var result = !MetaModelHelper.IsMetaModel(actionArguments, out IValidationMetaModel validationMetaModel) || validationMetaModel.IsValid;

            result &= !_validateCurrency || (MetaModelHelper.IsMetaModel(actionArguments, out ICurrencyIdMetaModel currencyIdMetaModel)
               && _currencyValidation.IsValidCurrency(currencyIdMetaModel.CurrencyId, _contextService.CurrencyList as IDictionary<int, string>));

            result &= !_validateCurrencyList || (MetaModelHelper.IsMetaModel(actionArguments, out ICurrencyIdListMetaModel currencyIdListMetaModel)
                && _currencyValidation.IsValidCurrencyList(currencyIdListMetaModel.CurrencyIdList, _contextService.CurrencyList as IDictionary<int, string>));

            return result;
        }
    }
}