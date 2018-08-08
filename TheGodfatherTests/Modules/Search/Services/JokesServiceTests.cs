#region USING_DIRECTIVES
using Microsoft.VisualStudio.TestTools.UnitTesting;

using System;
using System.Threading.Tasks;

using TheGodfather.Modules.Search.Services;
#endregion

namespace TheGodfatherTests.Modules.Search.Services
{
    [TestClass]
    public class JokesServiceTests
    {
        [TestMethod]
        public async Task GetRandomJokeAsyncTest()
        {
            for (int i = 0; i < 10; i++)
                Assert.IsNotNull(await JokesService.GetRandomJokeAsync());
        }

        [TestMethod]
        public async Task GetRandomYoMommaJokeAsync()
        {
            for (int i = 0; i < 10; i++)
                Assert.IsNotNull(await JokesService.GetRandomYoMommaJokeAsync());
        }

        [TestMethod]
        public async Task SearchForJokesAsyncTest()
        {
            Assert.IsNotNull(await JokesService.SearchForJokesAsync("dad joke"));
            Assert.IsNull(await JokesService.SearchForJokesAsync("FOOOOOOOOOOOOO1231231313123"));

            await Assert.ThrowsExceptionAsync<ArgumentException>(() => JokesService.SearchForJokesAsync(null));
            await Assert.ThrowsExceptionAsync<ArgumentException>(() => JokesService.SearchForJokesAsync(""));
            await Assert.ThrowsExceptionAsync<ArgumentException>(() => JokesService.SearchForJokesAsync(" "));
            await Assert.ThrowsExceptionAsync<ArgumentException>(() => JokesService.SearchForJokesAsync("\n"));
        }
    }
}
