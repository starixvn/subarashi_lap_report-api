namespace Fanex.Athena.Report.Core.UnitTests.Services
{
    using System;
    using System.Collections.Generic;
    using System.Numerics;
    using Core.Models.DTOs;
    using Core.Repositories.Abstraction;
    using Core.Services;
    using Moq;
    using Xunit;

    public class IpServiceTest
    {
        private readonly Mock<IIpRepository> _ipRepository;
        private readonly IpService _ipService;

        public IpServiceTest()
        {
            _ipRepository = new Mock<IIpRepository>();
            _ipService = new IpService(_ipRepository.Object);
        }

        [Fact]
        public void IsAlwaysIpvn_AnyInput_ReturnsTrue()
        {
            // Arrange
            var ipAddress = "192.168.1.1";

            // Act
            var result = _ipService.IsAlwaysIpvn(ipAddress);

            // Assert
            Assert.True(result);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void IsVNIP_NullOrEmptyInput_ReturnsFalse(string ipAddress)
        {
            // Act
            var result = _ipService.IsVNIP(ipAddress);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void IsVNIP_WhitespaceInput_ReturnsFalse()
        {
            // Arrange
            var ipAddress = "   ";
            var vnipRanges = new Tuple<IList<IPV4RangeEntity>, IList<IPV6RangeEntity>>(new List<IPV4RangeEntity>(), null);
            _ipRepository.Setup(x => x.GetVNIPs()).Returns(vnipRanges);

            // Act
            var result = _ipService.IsVNIP(ipAddress);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void IsVNIP_ValidIPv4InRange_ReturnsTrue()
        {
            // Arrange
            var ipAddress = "192.168.1.1";
            var ipv4Ranges = new List<IPV4RangeEntity>
            {
                new() { IPFrom = 3232235776, IPTo = 3232236031 } // 192.168.1.0 to 192.168.1.255
            };
            var vnipRanges = new Tuple<IList<IPV4RangeEntity>, IList<IPV6RangeEntity>>(ipv4Ranges, null);


            _ipRepository.Setup(x => x.GetVNIPs()).Returns(vnipRanges);

            // Act
            var result = _ipService.IsVNIP(ipAddress);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void IsVNIP_ValidIPv4OutOfRange_ReturnsFalse()
        {
            // Arrange
            var ipAddress = "10.0.0.1";
            var ipv4Ranges = new List<IPV4RangeEntity>
            {
                new() { IPFrom = 3232235776, IPTo = 3232236031 } // 192.168.1.0 to 192.168.1.255
            };
            var vnipRanges = new Tuple<IList<IPV4RangeEntity>, IList<IPV6RangeEntity>>(ipv4Ranges, null);


            _ipRepository.Setup(x => x.GetVNIPs()).Returns(vnipRanges);

            // Act
            var result = _ipService.IsVNIP(ipAddress);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void IsVNIP_IPv4WithNullRanges_ReturnsFalse()
        {
            // Arrange
            var ipAddress = "192.168.1.1";
            var vnipRanges = new Tuple<IList<IPV4RangeEntity>, IList<IPV6RangeEntity>>(null, null);


            _ipRepository.Setup(x => x.GetVNIPs()).Returns(vnipRanges);

            // Act
            var result = _ipService.IsVNIP(ipAddress);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void IsVNIP_ValidIPv6InRange_ReturnsTrue()
        {
            // Arrange
            var ipAddress = "2001:db8::1";
            var ipv6Number = IpService.ConvertIpV6ToNumber(ipAddress);
            var fromHex = (ipv6Number - 100).ToString("X");
            var toHex = (ipv6Number + 100).ToString("X");
            var ipv6Ranges = new List<IPV6RangeEntity>
            {
                new() { IPFrom = fromHex, IPTo = toHex }
            };
            var vnipRanges = new Tuple<IList<IPV4RangeEntity>, IList<IPV6RangeEntity>>(null, ipv6Ranges);


            _ipRepository.Setup(x => x.GetVNIPs()).Returns(vnipRanges);

            // Act
            var result = _ipService.IsVNIP(ipAddress);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void IsVNIP_IPv6WithNullRanges_ReturnsFalse()
        {
            // Arrange
            var ipAddress = "2001:db8::1";
            var vnipRanges = new Tuple<IList<IPV4RangeEntity>, IList<IPV6RangeEntity>>(null, null);


            _ipRepository.Setup(x => x.GetVNIPs()).Returns(vnipRanges);

            // Act
            var result = _ipService.IsVNIP(ipAddress);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void GetIpLocation_ShowIpvnTrue_CallsRepository()
        {
            // Arrange
            var ipAddress = "192.168.1.1";
            var expectedLocation = new Location { CountryCode = "VN", CountryName = "Vietnam" };
            _ipRepository.Setup(x => x.GetIpLocation(ipAddress)).Returns(expectedLocation);

            // Act
            var result = _ipService.GetIpLocation(ipAddress, true);

            // Assert
            Assert.Equal(expectedLocation, result);
            _ipRepository.Verify(x => x.GetIpLocation(ipAddress), Times.Once);
        }

        [Fact]
        public void GetIpLocation_ShowIpvnFalseAndNotVNIP_CallsRepository()
        {
            // Arrange
            var ipAddress = "8.8.8.8";
            var expectedLocation = new Location { CountryCode = "US", CountryName = "United States" };
            var vnipRanges = new Tuple<IList<IPV4RangeEntity>, IList<IPV6RangeEntity>>(new List<IPV4RangeEntity>(), null);


            _ipRepository.Setup(x => x.GetVNIPs()).Returns(vnipRanges);
            _ipRepository.Setup(x => x.GetIpLocation(ipAddress)).Returns(expectedLocation);

            // Act
            var result = _ipService.GetIpLocation(ipAddress);

            // Assert
            Assert.Equal(expectedLocation, result);
            _ipRepository.Verify(x => x.GetIpLocation(ipAddress), Times.Once);
        }

        [Fact]
        public void GetIpLocation_ShowIpvnFalseAndIsVNIP_ReturnsEmptyLocation()
        {
            // Arrange
            var ipAddress = "192.168.1.1";
            var ipv4Ranges = new List<IPV4RangeEntity>
            {
                new() { IPFrom = 3232235776, IPTo = 3232236031 }
            };
            var vnipRanges = new Tuple<IList<IPV4RangeEntity>, IList<IPV6RangeEntity>>(ipv4Ranges, null);


            _ipRepository.Setup(x => x.GetVNIPs()).Returns(vnipRanges);

            // Act
            var result = _ipService.GetIpLocation(ipAddress);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("N/A", result.CountryCode);
            Assert.Equal("N/A", result.CountryName);
            _ipRepository.Verify(x => x.GetIpLocation(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public void GetIpAddress_ShowIpvnTrue_ReturnsOriginalIp()
        {
            // Arrange
            var ipAddress = "192.168.1.1";

            // Act
            var result = _ipService.GetIpAddress(ipAddress, true);

            // Assert
            Assert.Equal(ipAddress, result);
        }

        [Fact]
        public void GetIpAddress_ShowIpvnFalseAndNotVNIP_ReturnsOriginalIp()
        {
            // Arrange
            var ipAddress = "8.8.8.8";
            var vnipRanges = new Tuple<IList<IPV4RangeEntity>, IList<IPV6RangeEntity>>(new List<IPV4RangeEntity>(), null);


            _ipRepository.Setup(x => x.GetVNIPs()).Returns(vnipRanges);

            // Act
            var result = _ipService.GetIpAddress(ipAddress);

            // Assert
            Assert.Equal(ipAddress, result);
        }

        [Fact]
        public void GetIpAddress_ShowIpvnFalseAndIsVNIP_ReturnsEmptyString()
        {
            // Arrange
            var ipAddress = "192.168.1.1";
            var ipv4Ranges = new List<IPV4RangeEntity>
            {
                new() { IPFrom = 3232235776, IPTo = 3232236031 }
            };
            var vnipRanges = new Tuple<IList<IPV4RangeEntity>, IList<IPV6RangeEntity>>(ipv4Ranges, null);

            _ipRepository.Setup(x => x.GetVNIPs()).Returns(vnipRanges);

            // Act
            var result = _ipService.GetIpAddress(ipAddress);

            // Assert
            Assert.Equal(string.Empty, result);
        }

        [Theory]
        [InlineData("192.168.1.1", 3232235777)]
        [InlineData("10.0.0.1", 167772161)]
        [InlineData("127.0.0.1", 2130706433)]
        [InlineData("255.255.255.255", 4294967295)]
        [InlineData("0.0.0.0", 0)]
        public void ConvertIpV4ToNumber_ValidIpAddress_ReturnsCorrectNumber(string ipAddress, double expected)
        {
            // Act
            var result = IpService.ConvertIpV4ToNumber(ipAddress);

            // Assert
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData("")]
        [InlineData("invalid")]
        [InlineData("256.256.256.256")]
        [InlineData("192.168.1")]
        public void ConvertIpV4ToNumber_InvalidIpAddress_ReturnsZero(string ipAddress)
        {
            // Act
            var result = IpService.ConvertIpV4ToNumber(ipAddress);

            // Assert
            Assert.Equal(0, result);
        }

        [Fact]
        public void ConvertIpV4ToNumber_TooManyOctets_ProcessesFirstFour()
        {
            // Arrange - IP with 5 octets, should process first 4
            var ipAddress = "192.168.1.1.1";

            // Act
            var result = IpService.ConvertIpV4ToNumber(ipAddress);

            // Assert - Should process as 192.168.1.1
            Assert.Equal(3232235777, result);
        }

        [Theory]
        [InlineData("192.168.1.1", true)]
        [InlineData("10.0.0.1", true)]
        [InlineData("127.0.0.1", true)]
        [InlineData("255.255.255.255", true)]
        [InlineData("0.0.0.0", true)]
        [InlineData("", false)]
        [InlineData("invalid", false)]
        [InlineData("256.256.256.256", false)]
        [InlineData("192.168.1", false)]
        [InlineData(null, false)]
        public void IsValidIpV4_VariousInputs_ReturnsExpectedResult(string ipAddress, bool expected)
        {
            // Act
            var result = IpService.IsValidIpV4(ipAddress);

            // Assert
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData("2001:db8::1")]
        [InlineData("::1")]
        [InlineData("fe80::1")]
        public void ConvertIpV6ToNumber_ValidIpv6Address_ReturnsPositiveNumber(string ipv6Address)
        {
            // Act
            var result = IpService.ConvertIpV6ToNumber(ipv6Address);

            // Assert
            Assert.True(result > 0);
        }

        [Theory]
        [InlineData("")]
        [InlineData("invalid")]
        public void ConvertIpV6ToNumber_InvalidIpv6Address_ReturnsZero(string ipv6Address)
        {
            // Act
            var result = IpService.ConvertIpV6ToNumber(ipv6Address);

            // Assert
            Assert.Equal(BigInteger.Zero, result);
        }
    }
}