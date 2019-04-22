#region USING_DIRECTIVES
using DSharpPlus.Entities;

using NUnit.Framework;

using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

using TheGodfather.Common;
using TheGodfather.Modules.Search.Services;
#endregion

namespace TheGodfatherTests.Modules.Search.Services
{
    [TestFixture]
    public class WeatherServiceTests
    {
        private WeatherService weather;


        [OneTimeSetUp]
        public async Task InitAsync()
        {
            try {
                string json;
                using (var sr = new StreamReader("Resources/config.json"))
                    json = await sr.ReadToEndAsync();
                BotConfig cfg = JsonConvert.DeserializeObject<BotConfig>(json);
                this.weather = new WeatherService(cfg.WeatherKey);
            } catch {
                Assert.Warn("Config file not found or OpenWeather key isn't valid (service disabled).");
                this.weather = new WeatherService(null);
            }
        }


        [Test]
        public async Task GetEmbeddedCurrentWeatherDataAsyncTest()
        {
            if (this.weather.IsDisabled())
                Assert.Inconclusive("Service has not been properly initialized.");

            Assert.IsNotNull(await this.weather.GetEmbeddedCurrentWeatherDataAsync("belgrade"));
            Assert.IsNotNull(await this.weather.GetEmbeddedCurrentWeatherDataAsync("london"));
            Assert.IsNotNull(await this.weather.GetEmbeddedCurrentWeatherDataAsync("berlin, de"));

            Assert.IsNull(await this.weather.GetEmbeddedCurrentWeatherDataAsync("NOT EXISTING LOCATION"));

            Assert.ThrowsAsync(typeof(ArgumentException), () => this.weather.GetEmbeddedCurrentWeatherDataAsync(null));
            Assert.ThrowsAsync(typeof(ArgumentException), () => this.weather.GetEmbeddedCurrentWeatherDataAsync(""));
            Assert.ThrowsAsync(typeof(ArgumentException), () => this.weather.GetEmbeddedCurrentWeatherDataAsync(" "));
            Assert.ThrowsAsync(typeof(ArgumentException), () => this.weather.GetEmbeddedCurrentWeatherDataAsync("\n"));
        }

        [Test]
        public async Task GetEmbeddedWeatherForecastAsyncTest()
        {
            if (this.weather.IsDisabled())
                Assert.Inconclusive("Service has not been properly initialized.");

            IReadOnlyList<DiscordEmbedBuilder> results;

            results = await this.weather.GetEmbeddedWeatherForecastAsync("belgrade");
            Assert.IsNotNull(results);
            Assert.AreEqual(7, results.Count);
            CollectionAssert.AllItemsAreNotNull(results);

            results = await this.weather.GetEmbeddedWeatherForecastAsync("belgrade", 15);
            Assert.IsNotNull(results);
            Assert.AreEqual(15, results.Count);
            CollectionAssert.AllItemsAreNotNull(results);

            results = await this.weather.GetEmbeddedWeatherForecastAsync("london");
            Assert.IsNotNull(results);
            Assert.AreEqual(7, results.Count);
            CollectionAssert.AllItemsAreNotNull(results);

            results = await this.weather.GetEmbeddedWeatherForecastAsync("berlin, de");
            Assert.IsNotNull(results);
            Assert.AreEqual(7, results.Count);
            CollectionAssert.AllItemsAreNotNull(results);

            Assert.IsNull(await this.weather.GetEmbeddedWeatherForecastAsync("NOT EXISTING LOCATION"));

            Assert.ThrowsAsync(typeof(ArgumentException), () => this.weather.GetEmbeddedWeatherForecastAsync(null));
            Assert.ThrowsAsync(typeof(ArgumentException), () => this.weather.GetEmbeddedWeatherForecastAsync(""));
            Assert.ThrowsAsync(typeof(ArgumentException), () => this.weather.GetEmbeddedWeatherForecastAsync(" "));
            Assert.ThrowsAsync(typeof(ArgumentException), () => this.weather.GetEmbeddedWeatherForecastAsync("\n"));
            Assert.ThrowsAsync(typeof(ArgumentException), () => this.weather.GetEmbeddedWeatherForecastAsync("belgrade", -1));
            Assert.ThrowsAsync(typeof(ArgumentException), () => this.weather.GetEmbeddedWeatherForecastAsync("belgrade", 0));
            Assert.ThrowsAsync(typeof(ArgumentException), () => this.weather.GetEmbeddedWeatherForecastAsync("belgrade", 100));
            Assert.ThrowsAsync(typeof(ArgumentException), () => this.weather.GetEmbeddedWeatherForecastAsync("belgrade", 21));
        }
    }
}
