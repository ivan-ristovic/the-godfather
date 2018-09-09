#region USING_DIRECTIVES
using NUnit.Framework;

using System.Threading.Tasks;

using TheGodfather.Modules.Search.Services;
#endregion

namespace TheGodfatherTests.Modules.Search.Services
{
    [TestFixture]
    public class QuoteServiceTests
    {
        [Test]
        public async Task GetQuoteOfTheDayAsync()
        {
            Assert.IsNotNull(await QuoteService.GetQuoteOfTheDayAsync());
            Assert.IsNotNull(await QuoteService.GetQuoteOfTheDayAsync("art"));
            Assert.IsNull(await QuoteService.GetQuoteOfTheDayAsync("FOOasdnsajdnsjadnj"));
        }

        [Test]
        public async Task GetRandomQuoteAsync()
        {
            Assert.IsNotNull(await QuoteService.GetRandomQuoteAsync());
        }
    }
}
