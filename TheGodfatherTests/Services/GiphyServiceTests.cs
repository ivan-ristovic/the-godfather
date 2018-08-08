using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using TheGodfather.Common;
using System;
using System.IO;
using System.Threading.Tasks;
using ImageData = GiphyDotNet.Model.GiphyImage.Data;
using RandomImageData = GiphyDotNet.Model.GiphyRandomImage.Data;

namespace TheGodfatherTests.Services
{
    [TestClass]
    public class GiphyServiceTests
    {
        private static GiphyService _service;


        [ClassInitialize]
        public static async Task Init(TestContext ctx)
        {
            try {
                string json;
                using (var sr = new StreamReader("Resources/config.json"))
                    json = await sr.ReadToEndAsync();
                var cfg = JsonConvert.DeserializeObject<BotConfig>(json);
                _service = new GiphyService(cfg.GiphyKey);
            } catch {
                Assert.Fail("Config file not found or GIPHY key isn't valid.");
            }
        }


        [TestMethod]
        public async Task SearchAsyncTest()
        {
            ImageData[] results;

            results = await _service.SearchAsync("test");
            Assert.IsNotNull(results);
            Assert.AreEqual(1, results.Length);
            Assert.IsNotNull(results[0]);
            Assert.IsNotNull(results[0].Url);

            results = await _service.SearchAsync("test", 5);
            Assert.IsNotNull(results);
            Assert.AreEqual(5, results.Length);
            CollectionAssert.AllItemsAreNotNull(results);

            results = await _service.SearchAsync("test", 15);
            Assert.IsNotNull(results);
            Assert.AreEqual(15, results.Length);
            CollectionAssert.AllItemsAreNotNull(results);

            await Assert.ThrowsExceptionAsync<ArgumentException>(() => _service.SearchAsync("test", 0));
            await Assert.ThrowsExceptionAsync<ArgumentException>(() => _service.SearchAsync("test", -5));
            await Assert.ThrowsExceptionAsync<ArgumentException>(() => _service.SearchAsync("test", 100));
            await Assert.ThrowsExceptionAsync<ArgumentException>(() => _service.SearchAsync("test", 21));
            await Assert.ThrowsExceptionAsync<ArgumentException>(() => _service.SearchAsync(null));
            await Assert.ThrowsExceptionAsync<ArgumentException>(() => _service.SearchAsync(""));
            await Assert.ThrowsExceptionAsync<ArgumentException>(() => _service.SearchAsync(" "));
            await Assert.ThrowsExceptionAsync<ArgumentException>(() => _service.SearchAsync("\n"));
        }

        [TestMethod]
        public async Task GetRandomGifAsyncTest()
        {
            RandomImageData data = await _service.GetRandomGifAsync();
            Assert.IsNotNull(data);
            Assert.IsNotNull(data.Url);
        }

        [TestMethod]
        public async Task GetTrendingGifsAsyncTest()
        {
            ImageData[] results;

            results = await _service.GetTrendingGifsAsync();
            Assert.IsNotNull(results);
            Assert.AreEqual(1, results.Length);
            Assert.IsNotNull(results[0]);
            Assert.IsNotNull(results[0].Url);

            results = await _service.GetTrendingGifsAsync(5);
            Assert.IsNotNull(results);
            Assert.AreEqual(5, results.Length);
            foreach (ImageData result in results) {
                Assert.IsNotNull(result);
                Assert.IsNotNull(result.Url);
            }

            await Assert.ThrowsExceptionAsync<ArgumentException>(() => _service.GetTrendingGifsAsync(0));
            await Assert.ThrowsExceptionAsync<ArgumentException>(() => _service.GetTrendingGifsAsync(-1));
            await Assert.ThrowsExceptionAsync<ArgumentException>(() => _service.GetTrendingGifsAsync(100));
            await Assert.ThrowsExceptionAsync<ArgumentException>(() => _service.GetTrendingGifsAsync(21));
        }
    }
}