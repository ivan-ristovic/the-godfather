#region USING_DIRECTIVES
using NUnit.Framework;

using Newtonsoft.Json;

using System;
using System.IO;
using System.Threading.Tasks;

using TheGodfather.Common;
using TheGodfather.Modules.Search.Services;

using ImageData = GiphyDotNet.Model.GiphyImage.Data;
using RandomImageData = GiphyDotNet.Model.GiphyRandomImage.Data;
#endregion

namespace TheGodfatherTests.Modules.Search.Services
{
    [TestFixture]
    public class GiphyServiceTests
    {
        private GiphyService giphy;
        

        [OneTimeSetUp]
        public async Task InitAsync()
        {
            try {
                string json;
                using (var sr = new StreamReader("Resources/config.json"))
                    json = await sr.ReadToEndAsync();
                BotConfig cfg = JsonConvert.DeserializeObject<BotConfig>(json);
                this.giphy = new GiphyService(cfg.GiphyKey);
            } catch {
                Assert.Warn("Config file not found or GIPHY key isn't valid (service disabled).");
                this.giphy = new GiphyService(null);
            }
        }


        [Test]
        public async Task SearchAsyncTest()
        {
            if (this.giphy.IsDisabled())
                Assert.Inconclusive("Service has not been properly initialized.");

            ImageData[] results;

            results = await this.giphy.SearchAsync("test");
            Assert.IsNotNull(results);
            Assert.AreEqual(1, results.Length);
            Assert.IsNotNull(results[0]?.Url);

            results = await this.giphy.SearchAsync("test", 5);
            Assert.IsNotNull(results);
            Assert.AreEqual(5, results.Length);
            CollectionAssert.AllItemsAreNotNull(results);

            results = await this.giphy.SearchAsync("test", 15);
            Assert.IsNotNull(results);
            Assert.AreEqual(15, results.Length);
            CollectionAssert.AllItemsAreNotNull(results);

            Assert.ThrowsAsync(typeof(ArgumentException), () => this.giphy.SearchAsync("test", 0));
            Assert.ThrowsAsync(typeof(ArgumentException), () => this.giphy.SearchAsync("test", -5));
            Assert.ThrowsAsync(typeof(ArgumentException), () => this.giphy.SearchAsync("test", 100));
            Assert.ThrowsAsync(typeof(ArgumentException), () => this.giphy.SearchAsync("test", 21));
            Assert.ThrowsAsync(typeof(ArgumentException), () => this.giphy.SearchAsync(null));
            Assert.ThrowsAsync(typeof(ArgumentException), () => this.giphy.SearchAsync(""));
            Assert.ThrowsAsync(typeof(ArgumentException), () => this.giphy.SearchAsync(" "));
            Assert.ThrowsAsync(typeof(ArgumentException), () => this.giphy.SearchAsync("\n"));
        }

        [Test]
        public async Task GetRandomGifAsyncTest()
        {
            if (this.giphy.IsDisabled())
                Assert.Inconclusive("Service has not been properly initialized.");

            RandomImageData data = await this.giphy.GetRandomGifAsync();
            Assert.IsNotNull(data?.Url);
        }

        [Test]
        public async Task GetTrendingGifsAsyncTest()
        {
            if (this.giphy.IsDisabled())
                Assert.Inconclusive("Service has not been properly initialized.");

            ImageData[] results;

            results = await this.giphy.GetTrendingGifsAsync();
            Assert.IsNotNull(results);
            Assert.AreEqual(1, results.Length);
            Assert.IsNotNull(results[0]?.Url);

            results = await this.giphy.GetTrendingGifsAsync(5);
            Assert.IsNotNull(results);
            Assert.AreEqual(5, results.Length);
            foreach (ImageData result in results)
                Assert.IsNotNull(result?.Url);

            Assert.ThrowsAsync(typeof(ArgumentException), () => this.giphy.GetTrendingGifsAsync(0));
            Assert.ThrowsAsync(typeof(ArgumentException), () => this.giphy.GetTrendingGifsAsync(-1));
            Assert.ThrowsAsync(typeof(ArgumentException), () => this.giphy.GetTrendingGifsAsync(100));
            Assert.ThrowsAsync(typeof(ArgumentException), () => this.giphy.GetTrendingGifsAsync(21));
        }
    }
}