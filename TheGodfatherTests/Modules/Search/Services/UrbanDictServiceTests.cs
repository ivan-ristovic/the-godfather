#region USING_DIRECTIVES
using NUnit.Framework;

using System;
using System.Threading.Tasks;

using TheGodfather.Modules.Search.Services;
#endregion

namespace TheGodfatherTests.Modules.Search.Services
{
    [TestFixture]
    public class UrbanDictServiceTests
    {
        [Test]
        public async Task GetDefinitionForTermAsyncTest()
        {
            Assert.IsNotNull(await UrbanDictService.GetDefinitionForTermAsync("umbrella"));
            Assert.IsNotNull(await UrbanDictService.GetDefinitionForTermAsync("jewnazi"));
            Assert.IsNotNull(await UrbanDictService.GetDefinitionForTermAsync("machine learning"));
            Assert.IsNotNull(await UrbanDictService.GetDefinitionForTermAsync("banzai!"));

            Assert.IsNull(await UrbanDictService.GetDefinitionForTermAsync("SDSANDJKSANDkJSANDKJSANDKAJND"));

            Assert.ThrowsAsync(typeof(ArgumentException), () => UrbanDictService.GetDefinitionForTermAsync(null));
            Assert.ThrowsAsync(typeof(ArgumentException), () => UrbanDictService.GetDefinitionForTermAsync(""));
            Assert.ThrowsAsync(typeof(ArgumentException), () => UrbanDictService.GetDefinitionForTermAsync(" "));
            Assert.ThrowsAsync(typeof(ArgumentException), () => UrbanDictService.GetDefinitionForTermAsync("\n"));
        }
    }
}
