#region USING_DIRECTIVES
using DSharpPlus.Interactivity;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using TheGodfather.Common;
using TheGodfather.Modules.Music.Common;
using TheGodfather.Modules.Search.Services;
#endregion

namespace TheGodfatherTests.Modules.Search.Services
{
    [TestClass]
    public class YtServiceTests
    {
        private static YtService _service;


        [ClassInitialize]
        public static async Task Init(TestContext ctx)
        {
            try {
                string json;
                using (var sr = new StreamReader("Resources/config.json"))
                    json = await sr.ReadToEndAsync();
                var cfg = JsonConvert.DeserializeObject<BotConfig>(json);
                _service = new YtService(cfg.YouTubeKey);
            } catch {
                Assert.Fail("Config file not found or YouTube API key isn't valid.");
            }
        }


        [TestMethod]
        public void GetRssUrlForChannelTest()
        {
            Assert.IsNotNull(YtService.GetRssUrlForChannel("UCuAXFkgsw1L7xaCfnd5JJOw"));

            Assert.ThrowsException<ArgumentException>(() => YtService.GetRssUrlForChannel(null));
            Assert.ThrowsException<ArgumentException>(() => YtService.GetRssUrlForChannel(""));
            Assert.ThrowsException<ArgumentException>(() => YtService.GetRssUrlForChannel(" "));
            Assert.ThrowsException<ArgumentException>(() => YtService.GetRssUrlForChannel("\n"));
            Assert.ThrowsException<ArgumentException>(() => YtService.GetRssUrlForChannel("/"));
            Assert.ThrowsException<ArgumentException>(() => YtService.GetRssUrlForChannel("test|"));
            Assert.ThrowsException<ArgumentException>(() => YtService.GetRssUrlForChannel("@4"));
            Assert.ThrowsException<ArgumentException>(() => YtService.GetRssUrlForChannel("user/123"));
        }

        [TestMethod]
        public async Task ExtractChannelIdAsyncTest()
        {
            Assert.AreEqual("UCLNd5EtH77IyN1frExzwPRQ", await _service.ExtractChannelIdAsync("https://www.youtube.com/channel/UCLNd5EtH77IyN1frExzwPRQ"));
            Assert.AreEqual("UCuAXFkgsw1L7xaCfnd5JJOw", await _service.ExtractChannelIdAsync("https://www.youtube.com/channel/UCuAXFkgsw1L7xaCfnd5JJOw"));
            Assert.AreEqual("UCHnyfMqiRRG1u-2MsSQLbXA", await _service.ExtractChannelIdAsync("https://www.youtube.com/channel/UCHnyfMqiRRG1u-2MsSQLbXA"));
            Assert.AreEqual("UCHnyfMqiRRG1u-2MsSQLbXA", await _service.ExtractChannelIdAsync("https://www.youtube.com/user/1veritasium/"));

            Assert.IsNull(await _service.ExtractChannelIdAsync("https://www.youtube.com/watch?v=dQw4w9WgXcQ"));
            Assert.IsNull(await _service.ExtractChannelIdAsync("https://www.google.com/watch?v=dQw4w9WgXcQ"));
            Assert.IsNull(await _service.ExtractChannelIdAsync("aaa"));

            await Assert.ThrowsExceptionAsync<ArgumentException>(() => _service.ExtractChannelIdAsync(null));
            await Assert.ThrowsExceptionAsync<ArgumentException>(() => _service.ExtractChannelIdAsync(""));
            await Assert.ThrowsExceptionAsync<ArgumentException>(() => _service.ExtractChannelIdAsync(" "));
            await Assert.ThrowsExceptionAsync<ArgumentException>(() => _service.ExtractChannelIdAsync("\n"));
        }

        [TestMethod]
        public async Task GetFirstVideoResultAsyncTest()
        {
            Assert.IsNotNull(await _service.GetFirstVideoResultAsync("numberphile"));
            Assert.IsNotNull(await _service.GetFirstVideoResultAsync("rick astley"));
            Assert.IsNotNull(await _service.GetFirstVideoResultAsync("|WM|"));
            Assert.IsNotNull(await _service.GetFirstVideoResultAsync("#triggered"));

            Assert.IsNull(await _service.GetFirstVideoResultAsync("nsakjdnkjsandjksandjksadkansdjksadsksandjkada"));

            await Assert.ThrowsExceptionAsync<ArgumentException>(() => _service.GetFirstVideoResultAsync(null));
            await Assert.ThrowsExceptionAsync<ArgumentException>(() => _service.GetFirstVideoResultAsync(""));
            await Assert.ThrowsExceptionAsync<ArgumentException>(() => _service.GetFirstVideoResultAsync(" "));
            await Assert.ThrowsExceptionAsync<ArgumentException>(() => _service.GetFirstVideoResultAsync("\n"));
        }

        [TestMethod]
        public async Task GetPaginatedResultsAsyncTest()
        {
            IReadOnlyList<Page> results;

            results = await _service.GetPaginatedResultsAsync("rick astley");
            Assert.IsNotNull(results);
            Assert.AreEqual(1, results.Count);
            CollectionAssert.AllItemsAreNotNull(results.ToList());

            results = await _service.GetPaginatedResultsAsync("rick astley", 5);
            Assert.IsNotNull(results);
            Assert.AreEqual(5, results.Count);
            CollectionAssert.AllItemsAreNotNull(results.ToList());

            results = await _service.GetPaginatedResultsAsync("rick astley", 5, "video");
            Assert.IsNotNull(results);
            Assert.AreEqual(5, results.Count);
            CollectionAssert.AllItemsAreNotNull(results.ToList());
            foreach (Page page in results)
                Assert.IsTrue(page.Embed.Url.OriginalString.Contains("/watch"));

            results = await _service.GetPaginatedResultsAsync("rick astley", 2, "channel");
            Assert.IsNotNull(results);
            Assert.AreEqual(2, results.Count);
            CollectionAssert.AllItemsAreNotNull(results.ToList());
            foreach (Page page in results)
                Assert.IsTrue(page.Embed.Description.Contains("/channel/"));

            Assert.IsNull(await _service.GetFirstVideoResultAsync("nsakjdnkjsandjksandjksadkansdjksadsksandjkada"));

            await Assert.ThrowsExceptionAsync<ArgumentException>(() => _service.GetPaginatedResultsAsync(null));
            await Assert.ThrowsExceptionAsync<ArgumentException>(() => _service.GetPaginatedResultsAsync(""));
            await Assert.ThrowsExceptionAsync<ArgumentException>(() => _service.GetPaginatedResultsAsync(" "));
            await Assert.ThrowsExceptionAsync<ArgumentException>(() => _service.GetPaginatedResultsAsync("\n"));
            await Assert.ThrowsExceptionAsync<ArgumentException>(() => _service.GetPaginatedResultsAsync("rick astley", -1));
            await Assert.ThrowsExceptionAsync<ArgumentException>(() => _service.GetPaginatedResultsAsync("rick astley", 0));
            await Assert.ThrowsExceptionAsync<ArgumentException>(() => _service.GetPaginatedResultsAsync("rick astley", 100));
            await Assert.ThrowsExceptionAsync<ArgumentException>(() => _service.GetPaginatedResultsAsync("rick astley", 21));
        }

        [TestMethod]
        public async Task GetSongInfoAsyncTest()
        {
            SongInfo info;

            info = await _service.GetSongInfoAsync("https://www.youtube.com/watch?v=dQw4w9WgXcQ");
            Assert.IsNotNull(info);
            Assert.IsNotNull(info.Uri);
            Assert.AreEqual("Rick Astley - Never Gonna Give You Up (Video)", info.Title);

            info = await _service.GetSongInfoAsync("https://www.youtube.com/watch?v=RB-RcX5DS5A");
            Assert.IsNotNull(info);
            Assert.IsNotNull(info.Uri);
            Assert.AreEqual("Coldplay - The Scientist", info.Title);

            Assert.IsNull(await _service.GetSongInfoAsync("aaaaa"));
            Assert.IsNull(await _service.GetSongInfoAsync("http://google.com"));

            await Assert.ThrowsExceptionAsync<ArgumentException>(() => _service.GetSongInfoAsync(null));
            await Assert.ThrowsExceptionAsync<ArgumentException>(() => _service.GetSongInfoAsync(""));
            await Assert.ThrowsExceptionAsync<ArgumentException>(() => _service.GetSongInfoAsync(" "));
            await Assert.ThrowsExceptionAsync<ArgumentException>(() => _service.GetSongInfoAsync("\n"));
        }
    }
}
