using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading.Tasks;
using TheGodfather.Services;

namespace TheGodfatherTests.Services
{
    [TestClass]
    public class XkcdServiceTests
    {
        [TestMethod]
        public async Task GetComicAsyncTest()
        {
            Assert.IsNotNull(await XkcdService.GetComicAsync());
            Assert.IsNotNull(await XkcdService.GetComicAsync(1));
            Assert.IsNotNull(await XkcdService.GetComicAsync(2));
            Assert.IsNotNull(await XkcdService.GetComicAsync(3));
            Assert.IsNotNull(await XkcdService.GetComicAsync(4));
            Assert.IsNotNull(await XkcdService.GetComicAsync(5));
            Assert.IsNotNull(await XkcdService.GetComicAsync(1000));
            Assert.IsNotNull(await XkcdService.GetComicAsync(2000));

            await Assert.ThrowsExceptionAsync<ArgumentException>(() => XkcdService.GetComicAsync(-1));
            await Assert.ThrowsExceptionAsync<ArgumentException>(() => XkcdService.GetComicAsync(0));
            await Assert.ThrowsExceptionAsync<ArgumentException>(() => XkcdService.GetComicAsync(100000));
        }

        [TestMethod]
        public async Task GetRandomComicAsyncTest()
        {
            for (int i = 0; i < 10; i++)
                Assert.IsNotNull(await XkcdService.GetRandomComicAsync());
        }
    }
}
