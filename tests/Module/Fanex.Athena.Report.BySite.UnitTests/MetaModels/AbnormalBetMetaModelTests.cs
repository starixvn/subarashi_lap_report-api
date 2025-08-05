using Fanex.Athena.Report.BySite.MetaModels;
using Xunit;

namespace Fanex.Athena.Report.BySite.UnitTests.MetaModels
{
    public class AbnormalBetMetaModelTests
    {
        [Fact]
        public void UserName_ShouldTrimWhitespace()
        {
            var metaModel = new AbnormalBetMetaModel { UserName = "  testUser  " };
            Assert.Equal("testUser", metaModel.UserName);
        }

        [Fact]
        public void UserName_ShouldReturnEmpty_WhenNullOrEmpty()
        {
            var metaModel = new AbnormalBetMetaModel { UserName = null };
            Assert.Equal(string.Empty, metaModel.UserName);

            metaModel.UserName = "";
            Assert.Equal(string.Empty, metaModel.UserName);
        }

        [Fact]
        public void LeagueName_ShouldTrimWhitespace()
        {
            var metaModel = new AbnormalBetMetaModel { LeagueName = "  testLeague  " };
            Assert.Equal("testLeague", metaModel.LeagueName);
        }

        [Fact]
        public void LeagueName_ShouldReturnEmpty_WhenNullOrEmpty()
        {
            var metaModel = new AbnormalBetMetaModel { LeagueName = null };
            Assert.Equal(string.Empty, metaModel.LeagueName);

            metaModel.LeagueName = "";
            Assert.Equal(string.Empty, metaModel.LeagueName);
        }

        [Fact]
        public void LicenseeName_ShouldTrimWhitespace()
        {
            var metaModel = new AbnormalBetMetaModel { LicenseeName = "  testLicensee  " };
            Assert.Equal("testLicensee", metaModel.LicenseeName);
        }

        [Fact]
        public void LicenseeName_ShouldReturnEmpty_WhenNullOrEmpty()
        {
            var metaModel = new AbnormalBetMetaModel { LicenseeName = null };
            Assert.Equal(string.Empty, metaModel.LicenseeName);

            metaModel.LicenseeName = "";
            Assert.Equal(string.Empty, metaModel.LicenseeName);
        }

        [Fact]
        public void Ip_ShouldTrimWhitespace()
        {
            var metaModel = new AbnormalBetMetaModel { Ip = "  127.0.0.1  " };
            Assert.Equal("127.0.0.1", metaModel.Ip);
        }

        [Fact]
        public void Ip_ShouldReturnEmpty_WhenNullOrEmpty()
        {
            var metaModel = new AbnormalBetMetaModel { Ip = null };
            Assert.Equal(string.Empty, metaModel.Ip);

            metaModel.Ip = "";
            Assert.Equal(string.Empty, metaModel.Ip);
        }

        [Theory]
        [InlineData(2, true)]
        [InlineData(3, true)]
        [InlineData(1, false)]
        [InlineData(4, false)]
        public void IsHideSaba_ShouldReturnCorrectValue(int specialPlatform, bool expected)
        {
            var metaModel = new AbnormalBetMetaModel { SpecialPlatform = specialPlatform };
            Assert.Equal(expected, metaModel.IsHideSaba);
        }
    }
}
