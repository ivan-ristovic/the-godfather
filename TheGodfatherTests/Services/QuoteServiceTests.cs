using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;

namespace TheGodfatherTests.Services
{
    [TestClass]
    public class QuoteServiceTests
    {
        [TestMethod]
        public async Task GetQuoteOfTheDayAsync()
        {
            Assert.IsNotNull(await QuoteService.GetQuoteOfTheDayAsync());
            await Task.Delay(100);
            Assert.IsNotNull(await QuoteService.GetQuoteOfTheDayAsync(null));
            await Task.Delay(100);
            Assert.IsNotNull(await QuoteService.GetQuoteOfTheDayAsync(""));
            await Task.Delay(100);
            Assert.IsNotNull(await QuoteService.GetQuoteOfTheDayAsync(" "));
            await Task.Delay(100);
            Assert.IsNotNull(await QuoteService.GetQuoteOfTheDayAsync("\n"));
            await Task.Delay(100);

            Assert.IsNotNull(await QuoteService.GetQuoteOfTheDayAsync("art"));
            await Task.Delay(100);
            Assert.IsNotNull(await QuoteService.GetQuoteOfTheDayAsync("sports"));
            await Task.Delay(100);
            Assert.IsNotNull(await QuoteService.GetQuoteOfTheDayAsync("life"));
            await Task.Delay(100);
            Assert.IsNotNull(await QuoteService.GetQuoteOfTheDayAsync("funny"));
            await Task.Delay(100);

            Assert.IsNull(await QuoteService.GetQuoteOfTheDayAsync("FOOasdnsajdnsjadnj"));
        }

        [TestMethod]
        public async Task GetRandomQuoteAsync()
        {
            for (int i = 0; i < 10; i++)
                Assert.IsNotNull(await QuoteService.GetRandomQuoteAsync());
        }
    }
}
