#region USING_DIRECTIVES
using NUnit.Framework;

using System;
using System.Threading.Tasks;

using TheGodfather.Modules.Search.Services;
#endregion

namespace TheGodfatherTests.Modules.Search.Services
{
    [TestFixture]
    public class XkcdServiceTests
    {
        [Test]
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

            Assert.ThrowsAsync(typeof(ArgumentException), () => XkcdService.GetComicAsync(-1));
            Assert.ThrowsAsync(typeof(ArgumentException), () => XkcdService.GetComicAsync(0));
            Assert.ThrowsAsync(typeof(ArgumentException), () => XkcdService.GetComicAsync(100000));
            Assert.ThrowsAsync(typeof(ArgumentException), () => XkcdService.GetComicAsync(XkcdService.TotalComics + 1));
        }

        [Test]
        public async Task GetRandomComicAsyncTest()
        {
            Assert.IsNotNull(await XkcdService.GetRandomComicAsync());
        }
    }
}
