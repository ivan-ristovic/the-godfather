#region USING_DIRECTIVES
using NUnit.Framework;

using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Syndication;

using TheGodfather.Modules.Search.Services;
#endregion

namespace TheGodfatherTests.Modules.Search.Services
{
    [TestFixture]
    public class RssServiceTests
    {
        [Test]
        public void GetFeedResultsTest()
        {
            IReadOnlyList<SyndicationItem> results;

            results = RssService.GetFeedResults("https://www.reddit.com/r/aww/.rss");
            Assert.IsNotNull(results);
            Assert.IsTrue(results.Any());
            Assert.AreEqual(5, results.Count);
            CollectionAssert.AllItemsAreNotNull(results);

            results = RssService.GetFeedResults("https://www.reddit.com/r/aww/.rss", 10);
            Assert.IsNotNull(results);
            Assert.IsTrue(results.Any());
            Assert.AreEqual(10, results.Count);
            CollectionAssert.AllItemsAreNotNull(results);

            results = RssService.GetFeedResults("https://www.reddit.com/r/MrRobot/.rss", 10);
            Assert.IsNotNull(results);
            Assert.IsTrue(results.Any());
            Assert.AreEqual(10, results.Count);
            CollectionAssert.AllItemsAreNotNull(results);

            results = RssService.GetFeedResults("https://news.google.com/news/rss/headlines/section/topic/WORLD?ned=us&hl=en", 1);
            Assert.IsNotNull(results);
            Assert.IsTrue(results.Any());
            CollectionAssert.AllItemsAreNotNull(results);

            results = RssService.GetFeedResults("https://www.youtube.com/feeds/videos.xml?channel_id=UCA5u8UquvO44Jcd3wZApyDg", 1);
            Assert.IsNotNull(results);
            Assert.IsTrue(results.Any());
            CollectionAssert.AllItemsAreNotNull(results);

            results = RssService.GetFeedResults("https://non_existing_URL.com/asdsada/");
            Assert.IsNull(results);

            Assert.Throws<ArgumentException>(() => RssService.GetFeedResults(null));
            Assert.Throws<ArgumentException>(() => RssService.GetFeedResults(""));
            Assert.Throws<ArgumentException>(() => RssService.GetFeedResults(" "));
            Assert.Throws<ArgumentException>(() => RssService.GetFeedResults("\n"));

            string aww = "https://www.reddit.com/r/aww/new/.rss";
            Assert.Throws<ArgumentException>(() => RssService.GetFeedResults(aww, -1));
            Assert.Throws<ArgumentException>(() => RssService.GetFeedResults(aww, 0));
            Assert.Throws<ArgumentException>(() => RssService.GetFeedResults(aww, 21));
        }

        [Test]
        public void IsValidRSSFeedURLTest()
        {
            Assert.IsTrue(RssService.IsValidFeedURL("https://www.reddit.com/r/MrRobot/.rss"));
            Assert.IsTrue(RssService.IsValidFeedURL("https://news.google.com/news/rss/headlines/section/topic/WORLD?ned=us&hl=en"));
            Assert.IsTrue(RssService.IsValidFeedURL("https://www.youtube.com/feeds/videos.xml?channel_id=UCA5u8UquvO44Jcd3wZApyDg"));

            Assert.IsFalse(RssService.IsValidFeedURL("https://www.reddit.com/r/MrRobot"));
            Assert.IsFalse(RssService.IsValidFeedURL("https://nonexisting.dsdsd/info.rss"));
            Assert.IsFalse(RssService.IsValidFeedURL(null));
            Assert.IsFalse(RssService.IsValidFeedURL(""));
            Assert.IsFalse(RssService.IsValidFeedURL(" "));
            Assert.IsFalse(RssService.IsValidFeedURL("\n"));
        }
    }
}