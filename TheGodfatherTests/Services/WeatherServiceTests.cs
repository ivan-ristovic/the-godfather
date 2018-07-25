using DSharpPlus.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TheGodfather.Common;
using TheGodfather.Services;

namespace TheGodfatherTests.Services
{
    [TestClass]
    public class WeatherServiceTests
    {
        private static WeatherService _service;


        [ClassInitialize]
        public static async Task Init(TestContext ctx)
        {
            try {
                string json;
                using (var sr = new StreamReader("Resources/config.json"))
                    json = await sr.ReadToEndAsync();
                var cfg = JsonConvert.DeserializeObject<BotConfig>(json);
                _service = new WeatherService(cfg.WeatherKey);
            } catch {
                Assert.Fail("Config file not found or OpenWeatherAPI key isn't valid.");
            }
        }


        [TestMethod]
        public async Task GetEmbeddedCurrentWeatherDataAsyncTest()
        {
            Assert.IsNotNull(await _service.GetEmbeddedCurrentWeatherDataAsync("belgrade"));
            Assert.IsNotNull(await _service.GetEmbeddedCurrentWeatherDataAsync("london"));
            Assert.IsNotNull(await _service.GetEmbeddedCurrentWeatherDataAsync("berlin, de"));

            Assert.IsNull(await _service.GetEmbeddedCurrentWeatherDataAsync("NOT EXISTING LOCATION"));

            await Assert.ThrowsExceptionAsync<ArgumentException>(() => _service.GetEmbeddedCurrentWeatherDataAsync(null));
            await Assert.ThrowsExceptionAsync<ArgumentException>(() => _service.GetEmbeddedCurrentWeatherDataAsync(""));
            await Assert.ThrowsExceptionAsync<ArgumentException>(() => _service.GetEmbeddedCurrentWeatherDataAsync(" "));
            await Assert.ThrowsExceptionAsync<ArgumentException>(() => _service.GetEmbeddedCurrentWeatherDataAsync("\n"));
        }

        [TestMethod]
        public async Task GetEmbeddedWeatherForecastAsyncTest()
        {
            IReadOnlyList<DiscordEmbed> results;

            results = await _service.GetEmbeddedWeatherForecastAsync("belgrade");
            Assert.IsNotNull(results);
            Assert.AreEqual(7, results.Count);
            CollectionAssert.AllItemsAreNotNull(results.ToList());

            results = await _service.GetEmbeddedWeatherForecastAsync("belgrade", 15);
            Assert.IsNotNull(results);
            Assert.AreEqual(15, results.Count);
            CollectionAssert.AllItemsAreNotNull(results.ToList());

            results = await _service.GetEmbeddedWeatherForecastAsync("london");
            Assert.IsNotNull(results);
            Assert.AreEqual(7, results.Count);
            CollectionAssert.AllItemsAreNotNull(results.ToList());

            results = await _service.GetEmbeddedWeatherForecastAsync("berlin, de");
            Assert.IsNotNull(results);
            Assert.AreEqual(7, results.Count);
            CollectionAssert.AllItemsAreNotNull(results.ToList());

            Assert.IsNull(await _service.GetEmbeddedWeatherForecastAsync("NOT EXISTING LOCATION"));

            await Assert.ThrowsExceptionAsync<ArgumentException>(() => _service.GetEmbeddedWeatherForecastAsync(null));
            await Assert.ThrowsExceptionAsync<ArgumentException>(() => _service.GetEmbeddedWeatherForecastAsync(""));
            await Assert.ThrowsExceptionAsync<ArgumentException>(() => _service.GetEmbeddedWeatherForecastAsync(" "));
            await Assert.ThrowsExceptionAsync<ArgumentException>(() => _service.GetEmbeddedWeatherForecastAsync("\n"));
            await Assert.ThrowsExceptionAsync<ArgumentException>(() => _service.GetEmbeddedWeatherForecastAsync("belgrade", -1));
            await Assert.ThrowsExceptionAsync<ArgumentException>(() => _service.GetEmbeddedWeatherForecastAsync("belgrade", 0));
            await Assert.ThrowsExceptionAsync<ArgumentException>(() => _service.GetEmbeddedWeatherForecastAsync("belgrade", 100));
            await Assert.ThrowsExceptionAsync<ArgumentException>(() => _service.GetEmbeddedWeatherForecastAsync("belgrade", 21));
        }
    }
}
