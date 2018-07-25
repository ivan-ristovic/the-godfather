using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TheGodfather.Services;

namespace TheGodfatherTests.Services
{
    [TestClass]
    public class UrbanDictServiceTests
    {
        [TestMethod]
        public async Task GetDefinitionForTermAsyncTest()
        {
            Assert.IsNotNull(await UrbanDictService.GetDefinitionForTermAsync("umbrella"));
            Assert.IsNotNull(await UrbanDictService.GetDefinitionForTermAsync("jewnazi"));
            Assert.IsNotNull(await UrbanDictService.GetDefinitionForTermAsync("machine learning"));
            Assert.IsNotNull(await UrbanDictService.GetDefinitionForTermAsync("banzai!"));

            Assert.IsNull(await UrbanDictService.GetDefinitionForTermAsync("SDSANDJKSANDkJSANDKJSANDKAJND"));

            await Assert.ThrowsExceptionAsync<ArgumentException>(() => UrbanDictService.GetDefinitionForTermAsync(null));
            await Assert.ThrowsExceptionAsync<ArgumentException>(() => UrbanDictService.GetDefinitionForTermAsync(""));
            await Assert.ThrowsExceptionAsync<ArgumentException>(() => UrbanDictService.GetDefinitionForTermAsync(" "));
            await Assert.ThrowsExceptionAsync<ArgumentException>(() => UrbanDictService.GetDefinitionForTermAsync("\n"));
        }
    }
}
