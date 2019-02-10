#region USING_DIRECTIVES
using NUnit.Framework;

using System;
using System.Threading.Tasks;

using TheGodfather.Modules.Search.Services;
#endregion

namespace TheGodfatherTests.Modules.Search.Services
{
    [TestFixture]
    public class JokesServiceTests
    {
        [Test]
        public async Task GetRandomJokeAsyncTest()
        {
            Assert.IsNotNull(await JokesService.GetRandomJokeAsync());
        }

        [Test]
        public async Task GetRandomYoMommaJokeAsync()
        {
            // FIXME Wait until the service is fixed
            // Assert.IsNotNull(await JokesService.GetRandomYoMommaJokeAsync());
        }

        [Test]
        public async Task SearchForJokesAsyncTest()
        {
            Assert.IsNotNull(await JokesService.SearchForJokesAsync("dad joke"));
            Assert.IsNull(await JokesService.SearchForJokesAsync("FOOOOOOOOOOOOO1231231313123"));

            Assert.ThrowsAsync(typeof(ArgumentException), () => JokesService.SearchForJokesAsync(null));
            Assert.ThrowsAsync(typeof(ArgumentException), () => JokesService.SearchForJokesAsync(""));
            Assert.ThrowsAsync(typeof(ArgumentException), () => JokesService.SearchForJokesAsync(" "));
            Assert.ThrowsAsync(typeof(ArgumentException), () => JokesService.SearchForJokesAsync("\n"));
        }
    }
}
