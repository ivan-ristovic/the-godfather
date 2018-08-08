using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;

namespace TheGodfatherTests.Services
{
    [TestClass]
    public class PetImagesServiceTests
    {
        [TestMethod]
        public async Task GetRandomCatImageAsyncTest()
        {
            for (int i = 0; i < 10; i++)
                Assert.IsNotNull(await PetImagesService.GetRandomCatImageAsync());
        }

        [TestMethod]
        public async Task GetRandomDogImageAsync()
        {
            for (int i = 0; i < 10; i++)
                Assert.IsNotNull(await PetImagesService.GetRandomDogImageAsync());
        }
    }
}
