namespace Fanex.Athena.Report.Core.UnitTests.Attributes
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using AutoFixture;
    using BySite.Controllers;
    using BySite.MetaModels;
    using BySite.Repositories.Interfaces;
    using Common.Model.DTO;
    using Core.Attributes;
    using Core.Services.Interfaces;
    using Fanex.Core.Utils;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Abstractions;
    using Microsoft.AspNetCore.Mvc.Filters;
    using Microsoft.AspNetCore.Mvc.ModelBinding;
    using Microsoft.AspNetCore.Routing;
    using Moq;
    using Xunit;

    public class AutoBindingMetaModelAttributeTest
    {
        private readonly IFixture fixture = new Fixture();

        private readonly Mock<IMetaModelBindingService> _bindingService = new Mock<IMetaModelBindingService>();
        private readonly Mock<IContextService> _contextService = new Mock<IContextService>();
        private readonly Mock<ISubsidiaryService> _subsidiaryService = new Mock<ISubsidiaryService>();

        [Fact]
        public void BindAgent_ValidatedMetaModel_BindAgentXml()
        {
            // Arrange
            var metaModel = fixture.Create<PlatformWinLossMetaModel>();
            var actionContext = CreateActionContext();
            var context = CreateContext(metaModel, actionContext);
            var next = CreateActionExecutionDelegate(actionContext);

            var agentList = new Dictionary<int, int[]> { { 1, new[] { 1, 2, 3 } } };
            _subsidiaryService.Setup(_ => _.IsIgnoredSubsidiarySearch(metaModel.SubsidiarySearch, metaModel.SubsidiaryList)).Returns(false);
            _subsidiaryService.Setup(_ => _.GetAgentList(metaModel.SubsidiaryList)).ReturnsAsync(agentList);

            var filter = new AutoBindingMetaModelImplAttribute(_bindingService.Object, _contextService.Object, _subsidiaryService.Object, true, false, false, false);

            // Action
            filter.OnActionExecutionAsync(context, next);

            // Assert
            _bindingService.Verify(b => b.BindAgentXml(context.ActionArguments, agentList), Times.Once);
        }

        [Fact]
        public void BindPlayerCom_ValidatedMetaModel_HasPlayerCom()
        {
            // Arrange
            var metaModel = fixture.Build<PlatformWinLossMetaModel>().With(_ => _.HasPlayerCom, false).Create();
            var actionContext = CreateActionContext();
            var context = CreateContext(metaModel, actionContext);
            var next = CreateActionExecutionDelegate(actionContext);
            _contextService.Setup(_ => _.IsAllowedCustomerComm).Returns(true);

            var filter = new AutoBindingMetaModelImplAttribute(_bindingService.Object, _contextService.Object, _subsidiaryService.Object, false, true, false, false);

            // Action
            filter.OnActionExecutionAsync(context, next);

            // Assert
            Assert.True(metaModel.HasPlayerCom);
        }

        [Fact]
        public void BindProductOptions_validatedMetaModel_BindProductOptions()
        {
            // Arrange
            var metaModel = fixture.Build<PlatformWinLossMetaModel>().With(_ => _.ProductOptions,
                new List<ProductOption> { new ProductOption { Code = "SB", Status = true } }).Create();
            var actionContext = CreateActionContext();
            var context = CreateContext(metaModel, actionContext);
            var next = CreateActionExecutionDelegate(actionContext);

            _contextService.Setup(_ => _.OfferedSubProductList).Returns(new List<string> { "SLT" });

            var filter = new AutoBindingMetaModelImplAttribute(_bindingService.Object, _contextService.Object, _subsidiaryService.Object, false, false, true, false);

            // Action
            filter.OnActionExecutionAsync(context, next);

            // Assert
            Assert.Contains(metaModel.ProductOptions, _ => _.Code == "SLT");
        }

        [Fact]
        public void BindProductCodeList_validatedMetaModel_BindProductCodeList()
        {
            // Arrange
            var metaModel = fixture.Build<LeagueWinLossMetaModel>().With(_ => _.ProductCodeList, "SB").Create();
            var actionContext = CreateActionContext();
            var context = new ActionExecutingContext(
                actionContext,
                new List<IFilterMetadata>(),
                new Dictionary<string, object> { { "Parameter", metaModel } },
                new Mock<Controller>().Object);
            var next = CreateActionExecutionDelegate(actionContext);

            _contextService.Setup(_ => _.OfferedSubProductList).Returns(new List<string> { "SLT" });

            var filter = new AutoBindingMetaModelImplAttribute(_bindingService.Object, _contextService.Object, _subsidiaryService.Object, false, false, false, true);

            // Action
            filter.OnActionExecutionAsync(context, next);

            // Assert
            Assert.Equal("SB,SLT", metaModel.ProductCodeList);
        }

        private static ActionContext CreateActionContext()
        {
            var modelState = new ModelStateDictionary();
            var httpContext = new DefaultHttpContext();
            return new ActionContext(
                    httpContext: httpContext,
                    routeData: new RouteData(),
                    actionDescriptor: new ActionDescriptor(),
                    modelState: modelState
             );
        }

        private static ActionExecutingContext CreateContext(PlatformWinLossMetaModel metaModel, ActionContext actionContext)
        {
            return new ActionExecutingContext(
                actionContext,
                new List<IFilterMetadata>(),
                new Dictionary<string, object> { { "Parameter", metaModel } },
                new Mock<Controller>().Object);
        }

        private static ActionExecutionDelegate CreateActionExecutionDelegate(ActionContext actionContext)
        {
            var repository = new Mock<IPlatformWinLossRepository>();
            var controller = new PlatformWinLossController(repository.Object);
            var metadata = new List<IFilterMetadata>();
            ActionExecutionDelegate next = () =>
            {
                var ctx = new ActionExecutedContext(actionContext, metadata, controller);
                return Task.FromResult(ctx);
            };

            return next;
        }
    }
}