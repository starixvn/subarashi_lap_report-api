using Fanex.Athena.Report.Base.Controllers;
using Fanex.Athena.Report.Common.Services.Interfaces;
using Fanex.Athena.Report.Core.Services.Interfaces;
using Fanex.Athena.Report.Core.Utility.Interfaces;
using Fanex.Athena.Report.UnitTests.Common;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Threading.Tasks;
using Xunit;

namespace Fanex.Athena.Report.Base.UnitTests.Controllers
{
    public class UserGuideControllerTest
    {
        private readonly Mock<IContextService> _contextService;

        private readonly Mock<IFileSystem> _fileSystem;

        private readonly Mock<IWebHostEnvironment> _hostEnvironment;

        private readonly Mock<IProductService> _productService;

        private readonly Mock<IBalanceUpService> _balanceUpService;

        public UserGuideControllerTest()
        {
            _fileSystem = new Mock<IFileSystem>();
            _productService = new Mock<IProductService>();
            _hostEnvironment = new Mock<IWebHostEnvironment>();
            _contextService = new Mock<IContextService>();
            _balanceUpService = new Mock<IBalanceUpService>();
        }

        [Fact]
        public async Task Index_FileNotExist_ExecutedDefaultFilePath()
        {
            // Arrange
            const string ViLanguage = "vi-VN";
            const string ResourceName = "SettledMatchCount";

            _contextService.Setup(x => x.Language).Returns(ViLanguage);
            _fileSystem.Setup(x => x.Exists(It.IsAny<string>())).Returns(false);

            var controller = BuildController();

            // Action
            await controller.Index(ResourceName);

            // Assert
            _fileSystem.Verify(x => x.ReadFile(It.Is<string>(_ => _.Contains(ResourceName) && _.Contains("en-US.htm"))));
        }

        [Fact]
        public async Task Index_FileExist_ReturnFilePathWithLanguageSession()
        {
            // Arrange
            const string ViLanguage = "vi-VN";
            const string ResourceName = "SettledMatchCount";

            _contextService.Setup(x => x.Language).Returns(ViLanguage);
            _fileSystem.Setup(x => x.Exists(It.IsAny<string>())).Returns(true);

            var controller = BuildController();

            // Action
            await controller.Index(ResourceName);

            // Assert
            _fileSystem.Verify(x => x.ReadFile(It.Is<string>(_ => _.Contains(ResourceName) && _.Contains($"{ViLanguage}.htm"))));
        }

        [Fact]
        public async Task Index_NotEnableSingleWallet_ReturnOpenBalanceSection()
        {
            // Arrange
            const string ResourceName = "CustomerBalance";
            const string Content = "This is Content";
            const string Section = "{open-balance-section}" + Content + "{/open-balance-section}";

            _contextService.Setup(x => x.Language).Returns(FakeHelper.Create<string>());
            _contextService.Setup(x => x.IsEnableSingleWallet).Returns(false);

            _fileSystem.Setup(x => x.ReadFile(It.IsAny<string>())).Returns(Section);

            var controller = BuildController();

            // Action
            var response = await controller.Index(ResourceName) as OkObjectResult;
            var result = response?.Value as string;

            // Assert
            Assert.Equal(Content, result);
        }

        [Fact]
        public async Task Index_EnableSingleWallet_RemoveOpenBalanceSection()
        {
            // Arrange
            const string ResourceName = "CustomerBalance";
            const string Content = "This is Content";
            const string Section = "{open-balance-section}" + Content + "{/open-balance-section}";

            _contextService.Setup(x => x.Language).Returns(FakeHelper.Create<string>());
            _contextService.Setup(x => x.IsEnableSingleWallet).Returns(true);

            _fileSystem.Setup(x => x.ReadFile(It.IsAny<string>())).Returns(Section);

            var controller = BuildController();

            // Action
            var response = await controller.Index(ResourceName) as OkObjectResult;
            var result = response?.Value as string;

            // Assert
            Assert.True(string.IsNullOrEmpty(result));
        }

        private UserGuideController BuildController() =>
            new(_contextService.Object, _balanceUpService.Object, _fileSystem.Object, _hostEnvironment.Object,
                _productService.Object);
    }
}
