#region USING_DIRECTIVES
using Imgur.API.Enums;
using Imgur.API.Models;

using NUnit.Framework;

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
    [TestFixture]
    public class ImgurServiceTests
    {
        private ImgurService imgur;


        [OneTimeSetUp]
        public async Task InitAsync()
        {
            try {
                string json;
                using (var sr = new StreamReader("Resources/config.json"))
                    json = await sr.ReadToEndAsync();
                BotConfig cfg = JsonConvert.DeserializeObject<BotConfig>(json);
                this.imgur = new ImgurService(cfg.ImgurKey);
            } catch {
                Assert.Warn("Config file not found or Imgur key isn't valid (service disabled).");
                this.imgur = new ImgurService(null);
            }
        }


        [Test]
        public async Task GetItemsFromSubAsyncTest()
        {
            if (this.imgur.IsDisabled())
                Assert.Inconclusive("Service has not been properly initialized.");

            IEnumerable<IGalleryItem> results;

            results = await this.imgur.GetItemsFromSubAsync("test", 1, SubredditGallerySortOrder.Top, TimeWindow.All);
            Assert.IsNotNull(results);
            Assert.AreEqual(1, results.Count());
            Assert.IsNotNull(results.FirstOrDefault());

            results = await this.imgur.GetItemsFromSubAsync("test", 5, SubredditGallerySortOrder.Top, TimeWindow.All);
            Assert.IsNotNull(results);
            Assert.AreEqual(5, results.Count());
            CollectionAssert.AllItemsAreNotNull(results.ToList());

            Assert.ThrowsAsync(typeof(ArgumentException),
                () => this.imgur.GetItemsFromSubAsync("test", 0, SubredditGallerySortOrder.Top, TimeWindow.All)
            );
            Assert.ThrowsAsync(typeof(ArgumentException),
                () => this.imgur.GetItemsFromSubAsync("test", -1, SubredditGallerySortOrder.Top, TimeWindow.All)
            );
            Assert.ThrowsAsync(typeof(ArgumentException),
                () => this.imgur.GetItemsFromSubAsync("test", 100, SubredditGallerySortOrder.Top, TimeWindow.All)
            );
            Assert.ThrowsAsync(typeof(ArgumentException),
                () => this.imgur.GetItemsFromSubAsync("test", 21, SubredditGallerySortOrder.Top, TimeWindow.All)
            );
            Assert.ThrowsAsync(typeof(ArgumentException),
                () => this.imgur.GetItemsFromSubAsync(null, 5, SubredditGallerySortOrder.Top, TimeWindow.All)
            );
            Assert.ThrowsAsync(typeof(ArgumentException),
                () => this.imgur.GetItemsFromSubAsync("", 5, SubredditGallerySortOrder.Top, TimeWindow.All)
            );
            Assert.ThrowsAsync(typeof(ArgumentException),
                () => this.imgur.GetItemsFromSubAsync(" ", 5, SubredditGallerySortOrder.Top, TimeWindow.All)
            );
            Assert.ThrowsAsync(typeof(ArgumentException),
                () => this.imgur.GetItemsFromSubAsync("\n", 5, SubredditGallerySortOrder.Top, TimeWindow.All)
            );
        }
    }
}
