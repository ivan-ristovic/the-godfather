#region USING_DIRECTIVES
using DSharpPlus.Interactivity;

using NUnit.Framework;

using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

using TheGodfather.Common;
using TheGodfather.Modules.Search.Common;
using TheGodfather.Modules.Search.Services;
#endregion

namespace TheGodfatherTests.Modules.Search.Services
{
    [TestFixture]
    public class OMDbServiceTests
    {
        private OMDbService omdb;


        [OneTimeSetUp]
        public async Task Init()
        {
            try {
                string json;
                using (var sr = new StreamReader("Resources/config.json"))
                    json = await sr.ReadToEndAsync();
                var cfg = JsonConvert.DeserializeObject<BotConfig>(json);
                this.omdb = new OMDbService(cfg.OMDbKey);
            } catch {
                Assert.Warn("Config file not found or OMDb key isn't valid (service disabled).");
                this.omdb = new OMDbService(null);
            }
        }


        [Test]
        public async Task SearchAsyncTest()
        {
            if (this.omdb.IsDisabled())
                Assert.Inconclusive("Service has not been properly initialized.");

            IReadOnlyList<Page> results;

            results = await this.omdb.GetPaginatedResultsAsync("Rocky");
            CollectionAssert.AllItemsAreNotNull(results);
            CollectionAssert.AllItemsAreUnique(results);

            results = await this.omdb.GetPaginatedResultsAsync("FOOOOOASDJSADBNKSANDKAS");
            Assert.IsNull(results);

            Assert.ThrowsAsync(typeof(ArgumentException), () => this.omdb.GetPaginatedResultsAsync(null));
            Assert.ThrowsAsync(typeof(ArgumentException), () => this.omdb.GetPaginatedResultsAsync(""));
            Assert.ThrowsAsync(typeof(ArgumentException), () => this.omdb.GetPaginatedResultsAsync(" "));
            Assert.ThrowsAsync(typeof(ArgumentException), () => this.omdb.GetPaginatedResultsAsync("\n"));
        }

        [Test]
        public async Task GetSingleResultAsyncTest()
        {
            if (this.omdb.IsDisabled())
                Assert.Inconclusive("Service has not been properly initialized.");

            MovieInfo result;

            result = await this.omdb.GetSingleResultAsync(OMDbQueryType.Title, "Rocky");
            Assert.IsNotNull(result);
            
            result = await this.omdb.GetSingleResultAsync(OMDbQueryType.Id, "tt0475784");
            Assert.IsNotNull(result);

            result = await this.omdb.GetSingleResultAsync(OMDbQueryType.Title, "FASODFSOADOSADNOSADNA");
            Assert.IsNull(result);

            result = await this.omdb.GetSingleResultAsync(OMDbQueryType.Id, "FASODFSOADOSADNOSADNA");
            Assert.IsNull(result);

            Assert.ThrowsAsync(typeof(ArgumentException), () => this.omdb.GetSingleResultAsync(OMDbQueryType.Id, null));
            Assert.ThrowsAsync(typeof(ArgumentException), () => this.omdb.GetSingleResultAsync(OMDbQueryType.Id, ""));
            Assert.ThrowsAsync(typeof(ArgumentException), () => this.omdb.GetSingleResultAsync(OMDbQueryType.Id, " "));
            Assert.ThrowsAsync(typeof(ArgumentException), () => this.omdb.GetSingleResultAsync(OMDbQueryType.Id, "\n"));
        }
    }
}
