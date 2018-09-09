#region USING_DIRECTIVES
using NUnit.Framework;

using System;
using System.Net;
using System.Threading.Tasks;

using TheGodfather.Modules.Search.Common;
using TheGodfather.Modules.Search.Services;
#endregion

namespace TheGodfatherTests.Modules.Search.Services
{
    [TestFixture]
    public class IpGeolocationServiceTests
    {
        [Test]
        public async Task GetInfoForIpAsyncTest()
        {
            IpInfo info;

            info = await IpGeolocationService.GetInfoForIpAsync("208.80.152.201");
            Assert.IsNotNull(info);
            Assert.IsTrue(info.Success);
            Assert.AreEqual("US", info.CountryCode);
            Assert.That(info.City.StartsWith("San Francisco"));
            Assert.AreEqual("94105", info.ZipCode);

            info = await IpGeolocationService.GetInfoForIpAsync(IPAddress.Parse("208.80.152.201"));
            Assert.IsNotNull(info);
            Assert.IsTrue(info.Success);
            Assert.AreEqual("US", info.CountryCode);
            Assert.That(info.City.StartsWith("San Francisco"));
            Assert.AreEqual("94105", info.ZipCode);

            Assert.ThrowsAsync(typeof(ArgumentException), () => IpGeolocationService.GetInfoForIpAsync((string)null));
            Assert.ThrowsAsync(typeof(ArgumentNullException), () => IpGeolocationService.GetInfoForIpAsync((IPAddress)null));
            Assert.ThrowsAsync(typeof(ArgumentException), () => IpGeolocationService.GetInfoForIpAsync(""));
            Assert.ThrowsAsync(typeof(ArgumentException), () => IpGeolocationService.GetInfoForIpAsync(" "));
            Assert.ThrowsAsync(typeof(ArgumentException), () => IpGeolocationService.GetInfoForIpAsync("\n"));
            Assert.ThrowsAsync(typeof(ArgumentException), () => IpGeolocationService.GetInfoForIpAsync("asd"));
            Assert.ThrowsAsync(typeof(ArgumentException), () => IpGeolocationService.GetInfoForIpAsync("asd"));
            Assert.ThrowsAsync(typeof(ArgumentException), () => IpGeolocationService.GetInfoForIpAsync("555.123.123.123"));
        }
    }
}
