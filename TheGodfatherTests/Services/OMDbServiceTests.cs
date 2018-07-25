using DSharpPlus.Interactivity;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TheGodfather.Common;
using TheGodfather.Services;
using TheGodfather.Services.Common;

namespace TheGodfatherTests.Services
{
    [TestClass]
    public class OMDbServiceTests
    {
        private static OMDbService _service;


        [ClassInitialize]
        public static async Task Init(TestContext ctx)
        {
            try {
                string json;
                using (var sr = new StreamReader("Resources/config.json"))
                    json = await sr.ReadToEndAsync();
                var cfg = JsonConvert.DeserializeObject<BotConfig>(json);
                _service = new OMDbService(cfg.OMDbKey);
            } catch {
                Assert.Fail("Config file not found or OMDb key isn't valid.");
            }
        }


        [TestMethod]
        public async Task SearchAsyncTest()
        {
            IReadOnlyList<Page> results;
            List<Page> resultList;

            results = await _service.GetPaginatedResultsAsync("Rocky");
            resultList = results.ToList();
            CollectionAssert.AllItemsAreNotNull(resultList);
            CollectionAssert.AllItemsAreUnique(resultList);

            results = await _service.GetPaginatedResultsAsync("FOOOOOASDJSADBNKSANDKAS");
            Assert.IsNull(results);

            await Assert.ThrowsExceptionAsync<ArgumentException>(() => _service.GetPaginatedResultsAsync(null));
            await Assert.ThrowsExceptionAsync<ArgumentException>(() => _service.GetPaginatedResultsAsync(""));
            await Assert.ThrowsExceptionAsync<ArgumentException>(() => _service.GetPaginatedResultsAsync(" "));
            await Assert.ThrowsExceptionAsync<ArgumentException>(() => _service.GetPaginatedResultsAsync("\n"));
        }

        [TestMethod]
        public async Task GetSingleResultAsyncTest()
        {
            MovieInfo result;

            result = await _service.GetSingleResultAsync(OMDbQueryType.Title, "Rocky");
            Assert.IsNotNull(result);
            
            result = await _service.GetSingleResultAsync(OMDbQueryType.Id, "tt0475784");
            Assert.IsNotNull(result);

            result = await _service.GetSingleResultAsync(OMDbQueryType.Title, "FASODFSOADOSADNOSADNA");
            Assert.IsNull(result);

            result = await _service.GetSingleResultAsync(OMDbQueryType.Id, "FASODFSOADOSADNOSADNA");
            Assert.IsNull(result);

            await Assert.ThrowsExceptionAsync<ArgumentException>(() => _service.GetSingleResultAsync(OMDbQueryType.Id, null));
            await Assert.ThrowsExceptionAsync<ArgumentException>(() => _service.GetSingleResultAsync(OMDbQueryType.Id, ""));
            await Assert.ThrowsExceptionAsync<ArgumentException>(() => _service.GetSingleResultAsync(OMDbQueryType.Id, " "));
            await Assert.ThrowsExceptionAsync<ArgumentException>(() => _service.GetSingleResultAsync(OMDbQueryType.Id, "\n"));
        }
    }
}
