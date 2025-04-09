using AutoFixture;
using Fanex.Athena.Report.BySite.Controllers;
using Fanex.Athena.Report.BySite.MetaModels;
using Fanex.Athena.Report.BySite.Repositories.Interfaces;
using Fanex.Athena.Report.Common.Model.DTO;
using Fanex.Athena.Report.Core.Attributes;
using Fanex.Athena.Report.Core.Services.Interfaces;
using Fanex.Core.Utils;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Routing;
using Moq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace Fanex.Athena.Report.Core.UnitTests.Attributes
{
    public class AutoBindingMetaModelAttributeTest
    {
        private readonly Mock<IMetaModelBindingService> _bindingService = new();

        private readonly Mock<IContextService> _contextService = new();

        private readonly Mock<ISubsidiaryService> _subsidiaryService = new();

        private readonly IFixture fixture = new Fixture();

        [Fact]
        public async Task BindAgent_ValidatedMetaModel_BindAgentXml()
        {
            // Arrange
            var metaModel = fixture.Create<PlatformWinLossMetaModel>();
            var actionContext = CreateActionContext();
            var context = CreateContext(metaModel, actionContext);
            var next = CreateActionExecutionDelegate(actionContext);

            var agentList = new Dictionary<int, int[]> { { 1, new[] { 1, 2, 3, } }, };
            _subsidiaryService
               .Setup(_ => _.IsIgnoredSubsidiarySearch(metaModel.SubsidiarySearch, metaModel.SubsidiaryList))
               .Returns(false);
            _subsidiaryService.Setup(_ => _.GetAgentList(metaModel.SubsidiaryList)).ReturnsAsync(agentList);

            var filter = CreateAttribute(true);

            // Action
            await filter.OnActionExecutionAsync(context, next);

            // Assert
            _bindingService.Verify(b => b.BindAgentXml(context.ActionArguments, agentList), Times.Once);
        }

        [Fact]
        public async Task BindPlayerCom_ValidatedMetaModel_HasPlayerCom()
        {
            // Arrange
            var metaModel = fixture.Build<PlatformWinLossMetaModel>().With(_ => _.HasPlayerCom, false).Create();
            var actionContext = CreateActionContext();
            var context = CreateContext(metaModel, actionContext);
            var next = CreateActionExecutionDelegate(actionContext);
            _contextService.Setup(_ => _.IsAllowedCustomerComm).Returns(true);

            var filter = CreateAttribute(bindPlayerCom: true);

            // Action
            await filter.OnActionExecutionAsync(context, next);

            // Assert
            Assert.True(metaModel.HasPlayerCom);
        }

        [Fact]
        public async Task BindProductOptions_validatedMetaModel_BindProductOptions()
        {
            // Arrange
            var metaModel = fixture.Build<PlatformWinLossMetaModel>().With(_ => _.ProductOptions,
                new List<ProductOption> { new() { Code = "SB", Status = true, }, }).Create();
            var actionContext = CreateActionContext();
            var context = CreateContext(metaModel, actionContext);
            var next = CreateActionExecutionDelegate(actionContext);

            _contextService.Setup(_ => _.OfferedSubProductList).Returns(new List<string> { "SLT", });

            var filter = CreateAttribute(bindProductOptions: true);

            // Action
            await filter.OnActionExecutionAsync(context, next);

            // Assert
            Assert.Contains(metaModel.ProductOptions, _ => _.Code == "SLT");
        }

        [Fact]
        public async Task BindProductCodeList_validatedMetaModel_BindProductCodeList()
        {
            // Arrange
            var metaModel = fixture.Build<LeagueWinLossMetaModel>().With(_ => _.ProductCodeList, "SB").Create();
            var actionContext = CreateActionContext();
            var context = new ActionExecutingContext(
                actionContext,
                new List<IFilterMetadata>(),
                new Dictionary<string, object> { { "Parameter", metaModel }, },
                new Mock<Controller>().Object);
            var next = CreateActionExecutionDelegate(actionContext);

            _contextService.Setup(_ => _.OfferedSubProductList).Returns(new List<string> { "SLT", });

            var filter = CreateAttribute(bindProductCodeList: true);

            // Action
            await filter.OnActionExecutionAsync(context, next);

            // Assert
            Assert.Equal("SB,SLT", metaModel.ProductCodeList);
        }

        private AutoBindingMetaModelImplAttribute CreateAttribute(bool bindAgent = false,
                                                                  bool bindPlayerCom = false,
                                                                  bool bindProductOptions = false,
                                                                  bool bindProductCodeList = false,
                                                                  bool bindUserId = false) =>
            new(_bindingService.Object,
                _contextService.Object,
                _subsidiaryService.Object,
                bindAgent,
                bindPlayerCom,
                bindProductOptions,
                bindProductCodeList,
                bindUserId);

        private static ActionContext CreateActionContext()
        {
            var modelState = new ModelStateDictionary();
            var httpContext = new DefaultHttpContext();
            return new ActionContext(
                httpContext,
                new RouteData(),
                new ActionDescriptor(),
                modelState
            );
        }

        private static ActionExecutingContext CreateContext(PlatformWinLossMetaModel metaModel,
                                                            ActionContext actionContext) =>
            new(
                actionContext,
                new List<IFilterMetadata>(),
                new Dictionary<string, object> { { "Parameter", metaModel }, },
                new Mock<Controller>().Object);

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
