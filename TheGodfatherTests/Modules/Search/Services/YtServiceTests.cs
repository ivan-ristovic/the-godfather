#region USING_DIRECTIVES
using DSharpPlus.Interactivity;

using NUnit.Framework;

using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

using TheGodfather.Common;
using TheGodfather.Modules.Music.Common;
using TheGodfather.Modules.Search.Services;
#endregion

namespace TheGodfatherTests.Modules.Search.Services
{
    [TestFixture]
    public class YtServiceTests
    {
        private YtService yt;


        [OneTimeSetUp]
        public async Task InitAsync()
        {
            try {
                string json;
                using (var sr = new StreamReader("Resources/config.json"))
                    json = await sr.ReadToEndAsync();
                var cfg = JsonConvert.DeserializeObject<BotConfig>(json);
                this.yt = new YtService(cfg.YouTubeKey);
            } catch {
                Assert.Warn("Config file not found or YouTube key isn't valid (service disabled).");
                this.yt = new YtService(null);
            }
        }


        [Test]
        public void GetRssUrlForChannelTest()
        {
            if (this.yt.IsDisabled())
                Assert.Inconclusive("Service has not been properly initialized.");

            Assert.IsNotNull(YtService.GetRssUrlForChannel("UCuAXFkgsw1L7xaCfnd5JJOw"));

            Assert.Throws<ArgumentException>(() => YtService.GetRssUrlForChannel(null));
            Assert.Throws<ArgumentException>(() => YtService.GetRssUrlForChannel(""));
            Assert.Throws<ArgumentException>(() => YtService.GetRssUrlForChannel(" "));
            Assert.Throws<ArgumentException>(() => YtService.GetRssUrlForChannel("\n"));
            Assert.Throws<ArgumentException>(() => YtService.GetRssUrlForChannel("/"));
            Assert.Throws<ArgumentException>(() => YtService.GetRssUrlForChannel("test|"));
            Assert.Throws<ArgumentException>(() => YtService.GetRssUrlForChannel("@4"));
            Assert.Throws<ArgumentException>(() => YtService.GetRssUrlForChannel("user/123"));
            Assert.Throws<ArgumentException>(() => YtService.GetRssUrlForChannel("*user"));
            Assert.Throws<ArgumentException>(() => YtService.GetRssUrlForChannel("* user"));
        }

        [Test]
        public async Task ExtractChannelIdAsyncTest()
        {
            if (this.yt.IsDisabled())
                Assert.Inconclusive("Service has not been properly initialized.");

            Assert.AreEqual(
                "UCLNd5EtH77IyN1frExzwPRQ", 
                await this.yt.ExtractChannelIdAsync("https://www.youtube.com/channel/UCLNd5EtH77IyN1frExzwPRQ")
            );
            Assert.AreEqual(
                "UCuAXFkgsw1L7xaCfnd5JJOw", 
                await this.yt.ExtractChannelIdAsync("https://www.youtube.com/channel/UCuAXFkgsw1L7xaCfnd5JJOw")
            );
            Assert.AreEqual(
                "UCHnyfMqiRRG1u-2MsSQLbXA", 
                await this.yt.ExtractChannelIdAsync("https://www.youtube.com/channel/UCHnyfMqiRRG1u-2MsSQLbXA")
            );
            Assert.AreEqual(
                "UCHnyfMqiRRG1u-2MsSQLbXA", 
                await this.yt.ExtractChannelIdAsync("https://www.youtube.com/user/1veritasium/")
            );

            Assert.IsNull(await this.yt.ExtractChannelIdAsync("https://www.youtube.com/watch?v=dQw4w9WgXcQ"));
            Assert.IsNull(await this.yt.ExtractChannelIdAsync("https://www.google.com/watch?v=dQw4w9WgXcQ"));
            Assert.IsNull(await this.yt.ExtractChannelIdAsync("aaa"));

            Assert.ThrowsAsync(typeof(ArgumentException), () => this.yt.ExtractChannelIdAsync(null));
            Assert.ThrowsAsync(typeof(ArgumentException), () => this.yt.ExtractChannelIdAsync(""));
            Assert.ThrowsAsync(typeof(ArgumentException), () => this.yt.ExtractChannelIdAsync(" "));
            Assert.ThrowsAsync(typeof(ArgumentException), () => this.yt.ExtractChannelIdAsync("\n"));
        }

        [Test]
        public async Task GetFirstVideoResultAsyncTest()
        {
            if (this.yt.IsDisabled())
                Assert.Inconclusive("Service has not been properly initialized.");

            Assert.IsNotNull(await this.yt.GetFirstVideoResultAsync("numberphile"));
            Assert.IsNotNull(await this.yt.GetFirstVideoResultAsync("rick astley"));
            Assert.IsNotNull(await this.yt.GetFirstVideoResultAsync("|WM|"));
            Assert.IsNotNull(await this.yt.GetFirstVideoResultAsync("#triggered"));

            Assert.IsNull(await this.yt.GetFirstVideoResultAsync("nsakjdnkjsandjksandjksadkansdjks45"));

            Assert.ThrowsAsync(typeof(ArgumentException), () => this.yt.GetFirstVideoResultAsync(null));
            Assert.ThrowsAsync(typeof(ArgumentException), () => this.yt.GetFirstVideoResultAsync(""));
            Assert.ThrowsAsync(typeof(ArgumentException), () => this.yt.GetFirstVideoResultAsync(" "));
            Assert.ThrowsAsync(typeof(ArgumentException), () => this.yt.GetFirstVideoResultAsync("\n"));
        }

        [Test]
        public async Task GetPaginatedResultsAsyncTest()
        {
            if (this.yt.IsDisabled())
                Assert.Inconclusive("Service has not been properly initialized.");

            IReadOnlyList<Page> results;

            results = await this.yt.GetPaginatedResultsAsync("rick astley");
            Assert.IsNotNull(results);
            Assert.AreEqual(1, results.Count);
            CollectionAssert.AllItemsAreNotNull(results);

            results = await this.yt.GetPaginatedResultsAsync("rick astley", 5);
            Assert.IsNotNull(results);
            Assert.AreEqual(5, results.Count);
            CollectionAssert.AllItemsAreNotNull(results);

            results = await this.yt.GetPaginatedResultsAsync("rick astley", 5, "video");
            Assert.IsNotNull(results);
            Assert.AreEqual(5, results.Count);
            CollectionAssert.AllItemsAreNotNull(results);
            foreach (Page page in results)
                Assert.IsTrue(page.Embed.Url.OriginalString.Contains("/watch"));

            results = await this.yt.GetPaginatedResultsAsync("rick astley", 2, "channel");
            Assert.IsNotNull(results);
            Assert.AreEqual(2, results.Count);
            CollectionAssert.AllItemsAreNotNull(results);
            foreach (Page page in results)
                Assert.IsTrue(page.Embed.Description.Contains("/channel/"));

            Assert.IsNull(await this.yt.GetFirstVideoResultAsync("nsakjdnkjsandjksandjksadkanksadsksandjkada"));

            Assert.ThrowsAsync(typeof(ArgumentException), () => this.yt.GetPaginatedResultsAsync(null));
            Assert.ThrowsAsync(typeof(ArgumentException), () => this.yt.GetPaginatedResultsAsync(""));
            Assert.ThrowsAsync(typeof(ArgumentException), () => this.yt.GetPaginatedResultsAsync(" "));
            Assert.ThrowsAsync(typeof(ArgumentException), () => this.yt.GetPaginatedResultsAsync("\n"));
            Assert.ThrowsAsync(typeof(ArgumentException), () => this.yt.GetPaginatedResultsAsync("rick astley", -1));
            Assert.ThrowsAsync(typeof(ArgumentException), () => this.yt.GetPaginatedResultsAsync("rick astley", 0));
            Assert.ThrowsAsync(typeof(ArgumentException), () => this.yt.GetPaginatedResultsAsync("rick astley", 100));
            Assert.ThrowsAsync(typeof(ArgumentException), () => this.yt.GetPaginatedResultsAsync("rick astley", 21));
        }

        [Test]
        public async Task GetSongInfoAsyncTest()
        {
            if (this.yt.IsDisabled())
                Assert.Inconclusive("Service has not been properly initialized.");

            SongInfo info;
            
            info = await this.yt.GetSongInfoAsync("https://www.youtube.com/watch?v=dQw4w9WgXcQ");
            Assert.IsNotNull(info?.Uri);
            Assert.That(info.Title.Contains("Rick Astley - Never Gonna Give You Up"));

            info = await this.yt.GetSongInfoAsync("https://www.youtube.com/watch?v=RB-RcX5DS5A");
            Assert.IsNotNull(info?.Uri);
            Assert.That(info.Title.Contains("Coldplay - The Scientist"));

            Assert.IsNull(await this.yt.GetSongInfoAsync("aaaaa"));
            Assert.IsNull(await this.yt.GetSongInfoAsync("http://google.com"));

            Assert.ThrowsAsync(typeof(ArgumentException), () => this.yt.GetSongInfoAsync(null));
            Assert.ThrowsAsync(typeof(ArgumentException), () => this.yt.GetSongInfoAsync(""));
            Assert.ThrowsAsync(typeof(ArgumentException), () => this.yt.GetSongInfoAsync(" "));
            Assert.ThrowsAsync(typeof(ArgumentException), () => this.yt.GetSongInfoAsync("\n"));
        }
    }
}
