using Fanex.Athena.Report.Common.Model.DTO;
using Fanex.Athena.Report.Common.Model.MetaModels;
using Fanex.Athena.Report.Core.Extensions;
using Fanex.Athena.Report.Core.Models;
using Fanex.Athena.Report.Core.Services;
using Fanex.Core.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Fanex.Athena.Report.Core.Attributes
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false)]
    public class AutoBindingMetaModelAttribute : TypeFilterAttribute
    {
        public AutoBindingMetaModelAttribute(
            bool bindAgent = false,
            bool bindPlayerCom = false,
            bool bindProductOptions = false
            )
            : base(typeof(AutoBindingMetaModelImplAttribute))
        {
            Arguments = new object[] { bindAgent, bindPlayerCom, bindProductOptions };
        }
    }

    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false)]
    public class AutoBindingMetaModelImplAttribute : ActionFilterAttribute
    {
        private readonly IMetaModelBindingService _bindingService;
        private readonly IContextService _contextService;
        private readonly ISubsidiaryService _subsidiaryService;

        private readonly bool _bindAgent;
        private readonly bool _bindProductOptions;
        private readonly bool _bindPlayerCom;

        public AutoBindingMetaModelImplAttribute(
            IMetaModelBindingService bindingService,
            IContextService contextService,
            ISubsidiaryService subsidiaryService,
            bool bindAgent,
            bool bindPlayerCom,
            bool bindProductOptions
         )
        {
            _bindingService = bindingService;
            _contextService = contextService;
            _subsidiaryService = subsidiaryService;

            _bindAgent = bindAgent;
            _bindProductOptions = bindProductOptions;
            _bindPlayerCom = bindPlayerCom;
        }

        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            if (_bindAgent)
            {
                await BindAgent(context);
            }

            if (_bindPlayerCom && MetaModelHelper.IsMetaModel(context.ActionArguments, out IPlayerComMetaModel playerComMetaModel))
            {
                playerComMetaModel.HasPlayerCom = _contextService.IsAllowedCustomerComm;
            }

            if (_bindProductOptions)
            {
                BindProductOptions(context.ActionArguments, _contextService.OfferedSubProductList);
            }

            await next();
        }

        private async Task BindAgent(ActionExecutingContext context)
        {
            var agentList = _contextService.AgentList as IDictionary<int, int[]>;
            if (MetaModelHelper.IsMetaModel(context.ActionArguments, out ISubsidiarySearchMetaModel subsidiaryMetaModel)
                && !_subsidiaryService.IsIgnoredSubsidiarySearch(subsidiaryMetaModel.SubsidiarySearch, subsidiaryMetaModel.SubsidiaryList))
            {
                agentList = await _subsidiaryService.GetAgentList(subsidiaryMetaModel.SubsidiaryList);
            }
            _bindingService.BindAgentXml(context.ActionArguments, agentList);
        }

        public static void BindProductOptions(IDictionary<string, object> actionArguments, IEnumerable<string> offeredSubProducts)
        {
            if (MetaModelHelper.IsMetaModel(actionArguments, out IProductOptionsMetaModel metaModelProductOptions)
                && metaModelProductOptions.ProductOptions.IsPresent())
            {
                BindProductOptions(metaModelProductOptions, offeredSubProducts);
            }
        }

        private static void BindProductOptions(IProductOptionsMetaModel metaModel, IEnumerable<string> offeredSubProducts)
        {
            metaModel.OriginalProductOptions = metaModel.ProductOptions;
            metaModel.ProductOptions = GetAllowedSubProductOptions(offeredSubProducts, metaModel.ProductOptions);
        }

        private static IEnumerable<ProductOption> GetAllowedSubProductOptions(IEnumerable<string> offeredSubProductList, IEnumerable<ProductOption> productOptions)
        {
            const string ProductSB = "SB";

            if (!offeredSubProductList.IsPresent() || !productOptions.Any(_ => _.Code == ProductSB && _.Status))
            {
                return productOptions;
            }

            var newProductOptions = new List<ProductOption>(productOptions);
            newProductOptions.AddRange(offeredSubProductList.Select(subProduct => new ProductOption
            {
                Code = subProduct,
                Status = true
            }));

            return newProductOptions;
        }
    }
}
