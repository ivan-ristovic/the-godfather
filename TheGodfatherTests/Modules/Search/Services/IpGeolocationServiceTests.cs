#region USING_DIRECTIVES
using Microsoft.VisualStudio.TestTools.UnitTesting;

using System;
using System.Threading.Tasks;

using TheGodfather.Modules.Search.Common;
using TheGodfather.Modules.Search.Services;
#endregion

namespace TheGodfatherTests.Modules.Search.Services
{
    [TestClass]
    public class IpGeolocationServiceTests
    {
        [TestMethod]
        public async Task GetInfoForIpAsyncTest()
        {
            IpInfo info;

            info = await IpGeolocationService.GetInfoForIpAsync("208.80.152.201");
            Assert.IsNotNull(info);
            Assert.IsTrue(info.Success);
            Assert.AreEqual("US", info.CountryCode);
            Assert.AreEqual("San Francisco", info.City);
            Assert.AreEqual("94105", info.ZipCode);
            
            info = await IpGeolocationService.GetInfoForIpAsync("208.80.152.999");
            Assert.IsNotNull(info);
            Assert.IsFalse(info.Success);

            await Assert.ThrowsExceptionAsync<ArgumentException>(
                () => IpGeolocationService.GetInfoForIpAsync(null)
            );
            await Assert.ThrowsExceptionAsync<ArgumentException>(
                () => IpGeolocationService.GetInfoForIpAsync("")
            );
            await Assert.ThrowsExceptionAsync<ArgumentException>(
                () => IpGeolocationService.GetInfoForIpAsync(" ")
            );
            await Assert.ThrowsExceptionAsync<ArgumentException>(
                () => IpGeolocationService.GetInfoForIpAsync("\n")
            );

            // TODO
            /*
            await Assert.ThrowsExceptionAsync<ArgumentException>(
                () => IpGeolocationService.GetInfoForIpAsync("asdsada")
            );
            */
        }
    }
}
