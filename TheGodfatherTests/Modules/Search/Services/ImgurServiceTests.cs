#region USING_DIRECTIVES
using Imgur.API.Enums;
using Imgur.API.Models;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using TheGodfather.Common;
using TheGodfather.Modules.Search.Services;
#endregion

namespace TheGodfatherTests.Modules.Search.Services
{
    [TestClass]
    public class ImgurServiceTests
    {
        private static ImgurService _service;


        [ClassInitialize]
        public static async Task Init(TestContext ctx)
        {
            try {
                string json;
                using (var sr = new StreamReader("Resources/config.json"))
                    json = await sr.ReadToEndAsync();
                var cfg = JsonConvert.DeserializeObject<BotConfig>(json);
                _service = new ImgurService(cfg.ImgurKey);
            } catch {
                Assert.Fail("Config file not found or Imgur key isn't valid.");
            }
        }


        [TestMethod]
        public async Task GetItemsFromSubAsyncTest()
        {
            IEnumerable<IGalleryItem> results;

            results = await _service.GetItemsFromSubAsync("test", 1, SubredditGallerySortOrder.Top, TimeWindow.All);
            Assert.IsNotNull(results);
            Assert.AreEqual(1, results.Count());
            Assert.IsNotNull(results.FirstOrDefault());

            results = await _service.GetItemsFromSubAsync("test", 5, SubredditGallerySortOrder.Top, TimeWindow.All);
            Assert.IsNotNull(results);
            Assert.AreEqual(5, results.Count());
            CollectionAssert.AllItemsAreNotNull(results.ToList());

            await Assert.ThrowsExceptionAsync<ArgumentException>(
                () => _service.GetItemsFromSubAsync("test", 0, SubredditGallerySortOrder.Top, TimeWindow.All)
            );
            await Assert.ThrowsExceptionAsync<ArgumentException>(
                () => _service.GetItemsFromSubAsync("test", -1, SubredditGallerySortOrder.Top, TimeWindow.All)
            );
            await Assert.ThrowsExceptionAsync<ArgumentException>(
                () => _service.GetItemsFromSubAsync("test", 100, SubredditGallerySortOrder.Top, TimeWindow.All)
            );
            await Assert.ThrowsExceptionAsync<ArgumentException>(
                () => _service.GetItemsFromSubAsync("test", 21, SubredditGallerySortOrder.Top, TimeWindow.All)
            );
            await Assert.ThrowsExceptionAsync<ArgumentException>(
                () => _service.GetItemsFromSubAsync(null, 5, SubredditGallerySortOrder.Top, TimeWindow.All)
            );
            await Assert.ThrowsExceptionAsync<ArgumentException>(
                () => _service.GetItemsFromSubAsync("", 5, SubredditGallerySortOrder.Top, TimeWindow.All)
            );
            await Assert.ThrowsExceptionAsync<ArgumentException>(
                () => _service.GetItemsFromSubAsync(" ", 5, SubredditGallerySortOrder.Top, TimeWindow.All)
            );
            await Assert.ThrowsExceptionAsync<ArgumentException>(
                () => _service.GetItemsFromSubAsync("\n", 5, SubredditGallerySortOrder.Top, TimeWindow.All)
            );
        }
    }
}
