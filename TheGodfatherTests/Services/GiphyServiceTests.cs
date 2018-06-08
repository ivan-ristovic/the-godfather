using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using TheGodfather.Common;
using TheGodfather.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheGodfather.Services.Tests
{
    [TestClass()]
    public class GiphyServiceTests
    {
        private static GiphyService _service { get; set; }


        [ClassInitialize]
        public static async Task Init(TestContext ctx)
        {
            string json;
            try {
                using (var sr = new StreamReader("Resources/config.json"))
                    json = await sr.ReadToEndAsync();
                var cfg = JsonConvert.DeserializeObject<BotConfig>(json);
                _service = new GiphyService(cfg.GiphyKey);
            } catch {
                Assert.Fail("Config file not found or GIPHY key isn't valid.");
            }
        }


        [TestMethod()]
        public async Task SearchAsyncTest()
        {
            
        }

        [TestMethod()]
        public async Task GetRandomGifAsyncTest()
        {
            var data = await _service.GetRandomGifAsync();
            Assert.IsNotNull(data);
            Assert.IsNotNull(data.Url);
        }

        [TestMethod()]
        public void GetTrendingGifsAsyncTest()
        {

        }
    }
}