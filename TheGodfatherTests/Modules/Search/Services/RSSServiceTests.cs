#region USING_DIRECTIVES
using Microsoft.VisualStudio.TestTools.UnitTesting;

using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Syndication;

using TheGodfather.Modules.Search.Services;
#endregion

namespace TheGodfatherTests.Modules.Search.Services
{
    [TestClass]
    public class RSSServiceTests
    {
        [TestMethod]
        public void GetFeedResultsTest()
        {
            IReadOnlyList<SyndicationItem> results;

            results = RssService.GetFeedResults("https://www.reddit.com/r/aww/.rss");
            Assert.IsNotNull(results);
            Assert.IsTrue(results.Any());
            Assert.AreEqual(5, results.Count);
            CollectionAssert.AllItemsAreNotNull(results.ToList());

            results = RssService.GetFeedResults("https://www.reddit.com/r/aww/.rss", 10);
            Assert.IsNotNull(results);
            Assert.IsTrue(results.Any());
            Assert.AreEqual(10, results.Count);
            CollectionAssert.AllItemsAreNotNull(results.ToList());

            results = RssService.GetFeedResults("https://www.reddit.com/r/MrRobot/.rss", 10);
            Assert.IsNotNull(results);
            Assert.IsTrue(results.Any());
            Assert.AreEqual(10, results.Count);
            CollectionAssert.AllItemsAreNotNull(results.ToList());

            results = RssService.GetFeedResults("https://news.google.com/news/rss/headlines/section/topic/WORLD?ned=us&hl=en", 1);
            Assert.IsNotNull(results);
            Assert.IsTrue(results.Any());
            CollectionAssert.AllItemsAreNotNull(results.ToList());

            results = RssService.GetFeedResults("https://www.youtube.com/feeds/videos.xml?channel_id=UCA5u8UquvO44Jcd3wZApyDg", 1);
            Assert.IsNotNull(results);
            Assert.IsTrue(results.Any());
            CollectionAssert.AllItemsAreNotNull(results.ToList());

            results = RssService.GetFeedResults("https://non_existing_URL.com/asdsada/");
            Assert.IsNull(results);

            Assert.ThrowsException<ArgumentException>(() => {
                RssService.GetFeedResults(null);
            });
            Assert.ThrowsException<ArgumentException>(() => {
                RssService.GetFeedResults("");
            });
            Assert.ThrowsException<ArgumentException>(() => {
                RssService.GetFeedResults(" ");
            });
            Assert.ThrowsException<ArgumentException>(() => {
                RssService.GetFeedResults("\n");
            });
            Assert.ThrowsException<ArgumentException>(() => {
                RssService.GetFeedResults("https://www.reddit.com/r/aww/new/.rss", -1);
            });
            Assert.ThrowsException<ArgumentException>(() => {
                RssService.GetFeedResults("https://www.reddit.com/r/aww/new/.rss", 0);
            });
            Assert.ThrowsException<ArgumentException>(() => {
                RssService.GetFeedResults("https://www.reddit.com/r/aww/new/.rss", 21);
            });
        }

        [TestMethod]
        public void GetFeedURLForSubredditTest()
        {
            string aww = "https://www.reddit.com/r/aww/new/.rss";
            string csgo = "https://www.reddit.com/r/globaloffensive/new/.rss";
            string sub = null;

            Assert.AreEqual(aww, RssService.GetFeedURLForSubreddit("/r/aww", out sub));
            Assert.AreEqual(sub, "/r/aww");
            Assert.AreEqual(aww, RssService.GetFeedURLForSubreddit("r/aww", out sub));
            Assert.AreEqual(sub, "/r/aww");
            Assert.AreEqual(aww, RssService.GetFeedURLForSubreddit("/aww", out sub));
            Assert.AreEqual(sub, "/r/aww");
            Assert.AreEqual(aww, RssService.GetFeedURLForSubreddit("/r/aWW", out sub));
            Assert.AreEqual(sub, "/r/aww");
            Assert.AreEqual(aww, RssService.GetFeedURLForSubreddit("/R/aww", out sub));
            Assert.AreEqual(sub, "/r/aww");
            Assert.AreEqual(aww, RssService.GetFeedURLForSubreddit("/AWW", out sub));
            Assert.AreEqual(sub, "/r/aww");
            Assert.AreEqual(csgo, RssService.GetFeedURLForSubreddit("GlobalOffensive", out sub));
            Assert.AreEqual(sub, "/r/globaloffensive");

            Assert.ThrowsException<ArgumentException>(() => {
                Assert.IsNull(RssService.GetFeedURLForSubreddit("Global Offensive", out sub));
            });
            Assert.ThrowsException<ArgumentException>(() => {
                Assert.IsNull(RssService.GetFeedURLForSubreddit("Global.Offensive", out sub));
            });
            Assert.ThrowsException<ArgumentException>(() => {
                Assert.IsNull(RssService.GetFeedURLForSubreddit("Global-Offensive", out sub));
            });
            Assert.ThrowsException<ArgumentException>(() => {
                Assert.IsNull(RssService.GetFeedURLForSubreddit("Global*Offensive", out sub));
            });
            Assert.ThrowsException<ArgumentException>(() => {
                Assert.IsNull(RssService.GetFeedURLForSubreddit("Global+Offensive", out sub));
            });
            Assert.ThrowsException<ArgumentException>(() => {
                Assert.IsNull(RssService.GetFeedURLForSubreddit("Global?Offensive", out sub));
            });
            Assert.ThrowsException<ArgumentException>(() => {
                Assert.IsNull(RssService.GetFeedURLForSubreddit("Global??Offensive", out sub));
            });
            Assert.ThrowsException<ArgumentException>(() => {
                Assert.IsNull(RssService.GetFeedURLForSubreddit("Global??Offens>ive", out sub));
            });
            Assert.ThrowsException<ArgumentException>(() => {
                Assert.IsNull(RssService.GetFeedURLForSubreddit(".", out sub));
            });
            Assert.ThrowsException<ArgumentException>(() => {
                Assert.IsNull(RssService.GetFeedURLForSubreddit("---", out sub));
            });

            Assert.IsNull(RssService.GetFeedURLForSubreddit("FOOASDSADSANDJSKANDSKJANDSKAD", out sub));
        }

        [TestMethod]
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