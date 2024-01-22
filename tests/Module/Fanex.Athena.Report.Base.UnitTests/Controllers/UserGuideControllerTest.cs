using System.Linq;
using Fanex.Athena.Report.Base.Controllers;
using Fanex.Athena.Report.Common.Services.Interfaces;
using Fanex.Athena.Report.Core.Constants;
using Fanex.Athena.Report.Core.Models.DTOs;
using Fanex.Athena.Report.Core.Services.Interfaces;
using Fanex.Athena.Report.Core.Utility.Interfaces;
using Fanex.Athena.Report.UnitTests.Common;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace Fanex.Athena.Report.Base.UnitTests.Controllers
{
    public class UserGuideControllerTest
    {
        private readonly Mock<ICacheRefAdapter> _cacheRefAdapter;

        private readonly Mock<IContextService> _contextService;

        private readonly Mock<IFileSystem> _fileSystem;

        private readonly Mock<IWebHostEnvironment> _hostEnvironment;

        private readonly Mock<IProductService> _productService;

        public UserGuideControllerTest()
        {
            _cacheRefAdapter = new Mock<ICacheRefAdapter>();
            _fileSystem = new Mock<IFileSystem>();
            _productService = new Mock<IProductService>();
            _hostEnvironment = new Mock<IWebHostEnvironment>();
            _contextService = new Mock<IContextService>();
        }

        [Fact]
        public void Index_FileNotExist_ExecutedDefaultFilePath()
        {
            // Arrange
            const string ViLanguage = "vi-VN";
            const string ResourceName = "SettledMatchCount";

            _contextService.Setup(_ => _.Language).Returns(ViLanguage);
            _fileSystem.Setup(_ => _.Exists(It.IsAny<string>())).Returns(false);

            var controller = BuildController();

            // Action
            controller.Index(ResourceName);

            // Assert
            _fileSystem.Verify(x =>
                x.ReadFile(It.Is<string>(_ => _.Contains(ResourceName) && _.Contains("en-US.htm"))));
        }

        [Fact]
        public void Index_FileExist_ReturnFilePathWithLanguageSession()
        {
            // Arrange
            const string ViLanguage = "vi-VN";
            const string ResourceName = "SettledMatchCount";

            _contextService.Setup(_ => _.Language).Returns(ViLanguage);
            _fileSystem.Setup(_ => _.Exists(It.IsAny<string>())).Returns(true);

            var controller = BuildController();

            // Action
            controller.Index(ResourceName);

            // Assert
            _fileSystem.Verify(x =>
                x.ReadFile(It.Is<string>(_ => _.Contains(ResourceName) && _.Contains($"{ViLanguage}.htm"))));
        }

        [Fact]
        public void Index_NotEnableSingleWallet_ReturnOpenBalanceSection()
        {
            // Arrange
            const string ResourceName = "CustomerBalance";
            const string Content = "This is Content";
            const string Section = "{open-balance-section}" + Content + "{/open-balance-section}";

            _contextService.Setup(_ => _.Language).Returns(FakeHelper.Create<string>());
            _contextService.Setup(_ => _.IsEnableSingleWallet).Returns(false);

            _fileSystem.Setup(_ => _.ReadFile(It.IsAny<string>())).Returns(Section);

            var controller = BuildController();

            // Action
            var response = controller.Index(ResourceName) as OkObjectResult;
            var result = response?.Value as string;

            // Assert
            Assert.Equal(Content, result);
        }

        [Fact]
        public void Index_EnableSingleWallet_RemoveOpenBalanceSection()
        {
            // Arrange
            const string ResourceName = "CustomerBalance";
            const string Content = "This is Content";
            const string Section = "{open-balance-section}" + Content + "{/open-balance-section}";

            _contextService.Setup(_ => _.Language).Returns(FakeHelper.Create<string>());
            _contextService.Setup(_ => _.IsEnableSingleWallet).Returns(true);

            _fileSystem.Setup(_ => _.ReadFile(It.IsAny<string>())).Returns(Section);

            var controller = BuildController();

            // Action
            var response = controller.Index(ResourceName) as OkObjectResult;
            var result = response?.Value as string;

            // Assert
            Assert.True(string.IsNullOrEmpty(result));
        }

        [Fact]
        public void Index_VS2Sport_ReturnCorrectContent()
        {
            // Arrange
            const string ResourceName = "CustomerBalance";
            const string Section = "{VS2-sportList}";

            _contextService.Setup(_ => _.Language).Returns(FakeHelper.Create<string>());
            _contextService.Setup(_ => _.IsEnableSingleWallet).Returns(true);

            _fileSystem.Setup(_ => _.ReadFile(It.IsAny<string>())).Returns(Section);

            var controller = BuildController();

            // Action
            var response = controller.Index(ResourceName) as OkObjectResult;
            var result = response?.Value as string;
            result = result.Replace(",", "");

            // Assert
            Assert.True(string.IsNullOrWhiteSpace(result));
        }

        [Fact]
        public void Index_AddSpecialProducts_RemoveProductVS2Section()
        {
            // Arrange
            const string ResourceName = "CustomerBalance";
            const string Content = "This is Content";
            const string Section = "{VS2-productlist-section}" + Content + "{/VS2-productlist-section}";

            _contextService.Setup(_ => _.Language).Returns(FakeHelper.Create<string>());
            _contextService.Setup(_ => _.IsEnableSingleWallet).Returns(true);
            _productService.Setup(_ => _.GetProductList(It.IsAny<int>(), It.IsAny<string>()))
                           .Returns(FakeHelper.CreateMany<ProductInfo>().ToList());

            _fileSystem.Setup(_ => _.ReadFile(It.IsAny<string>())).Returns(Section);

            var controller = BuildController();

            // Action
            var response = controller.Index(ResourceName) as OkObjectResult;
            var result = response?.Value as string;

            // Assert
            Assert.True(string.IsNullOrEmpty(result));
        }

        [Fact]
        public void Index_AddSpecialVs2Products_ReturnProductVS2Section()
        {
            // Arrange
            const string ResourceName = "CustomerBalance";
            const string Content = "This is Content";
            const string Section = "{VS2-productlist-section}" + Content + "{/VS2-productlist-section}";
            var products = FakeHelper.CreateMany<ProductInfo>().ToList();
            products[0].ProductID = ProductTypes.VirtualSports2;

            _contextService.Setup(_ => _.Language).Returns(FakeHelper.Create<string>());
            _contextService.Setup(_ => _.IsEnableSingleWallet).Returns(true);
            _productService.Setup(_ => _.GetProductList(It.IsAny<int>(), It.IsAny<string>()))
                           .Returns(products);

            _fileSystem.Setup(_ => _.ReadFile(It.IsAny<string>())).Returns(Section);

            var controller = BuildController();

            // Action
            var response = controller.Index(ResourceName) as OkObjectResult;
            var result = response?.Value as string;

            // Assert
            Assert.Equal(Content, result);
        }

        private UserGuideController BuildController() =>
            new(_contextService.Object, _fileSystem.Object, _hostEnvironment.Object,
                _productService.Object, _cacheRefAdapter.Object);
    }
}
