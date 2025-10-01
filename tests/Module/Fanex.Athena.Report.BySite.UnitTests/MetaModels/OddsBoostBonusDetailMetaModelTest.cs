namespace Fanex.Athena.Report.BySite.UnitTests.MetaModels
{
    using AutoFixture;
    using BySite.MetaModels;
    using Xunit;

    public class ExtraPayoutMetaModelTest
    {
        private readonly IFixture _fixture = new Fixture();

        [Fact]
        public void ExtraPayoutMetaModel_InheritsFromExtraPayoutMetaModel()
        {
            // Arrange & Act
            var metaModel = new ExtraPayoutMetaModel();

            // Assert
            Assert.IsAssignableFrom<ExtraPayoutMetaModel>(metaModel);
        }

        [Fact]
        public void ExtraPayoutMetaModel_Properties_CanBeSetAndRetrieved()
        {
            // Arrange
            var expectedUsername = _fixture.Create<string>();

            // Act
            var metaModel = new ExtraPayoutMetaModel
            {
                Username = expectedUsername
            };

            // Assert
            Assert.Equal(expectedUsername, metaModel.Username);
        }

        [Fact]
        public void ExtraPayoutMetaModel_InheritsBaseProperties()
        {
            // Arrange
            var expectedProductCodeList = _fixture.Create<string>();
            var expectedBaseCurrency = _fixture.Create<string>();

            // Act
            var metaModel = new ExtraPayoutMetaModel
            {
                ProductCodeList = expectedProductCodeList,
                BaseCurrency = expectedBaseCurrency
            };

            // Assert
            Assert.Equal(expectedProductCodeList, metaModel.ProductCodeList);
            Assert.Equal(expectedBaseCurrency, metaModel.BaseCurrency);
        }

        [Theory]
        [InlineData("")]
        [InlineData("test123")]
        [InlineData("external_id_with_underscores")]
        [InlineData("ExternalId123!@#")]
        public void ExtraPayoutMetaModel_ExternalId_AcceptsVariousStringValues(string username)
        {
            // Arrange & Act
            var metaModel = new ExtraPayoutMetaModel { Username = username };

            // Assert
            Assert.Equal(username, metaModel.Username);
        }
    }
}