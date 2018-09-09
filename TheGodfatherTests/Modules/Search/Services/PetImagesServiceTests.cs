#region USING_DIRECTIVES
using NUnit.Framework;

using System.Threading.Tasks;

using TheGodfather.Modules.Search.Services;
#endregion

namespace TheGodfatherTests.Modules.Search.Services
{
    [TestFixture]
    public class PetImagesServiceTests
    {
        [Test]
        public async Task GetRandomCatImageAsyncTest()
        {
            Assert.IsNotNull(await PetImagesService.GetRandomCatImageAsync());
        }

        [Test]
        public async Task GetRandomDogImageAsync()
        {
            Assert.IsNotNull(await PetImagesService.GetRandomDogImageAsync());
        }
    }
}
