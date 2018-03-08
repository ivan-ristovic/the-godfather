using Microsoft.VisualStudio.TestTools.UnitTesting;
using TheGodfather.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace TheGodfather.Services.Tests
{
    [TestClass()]
    public class RSSServiceTests
    {
        [TestMethod()]
        public void IsValidRSSFeedURLTest()
        {
            Assert.IsTrue(RSSService.IsValidRSSFeedURL("https://www.reddit.com/r/MrRobot/.rss"));
            Assert.IsTrue(RSSService.IsValidRSSFeedURL("https://news.google.com/news/rss/headlines/section/topic/WORLD?ned=us&hl=en"));
            Assert.IsTrue(RSSService.IsValidRSSFeedURL("https://www.youtube.com/feeds/videos.xml?channel_id=UCA5u8UquvO44Jcd3wZApyDg"));
            Assert.IsFalse(RSSService.IsValidRSSFeedURL("https://www.reddit.com/r/MrRobot"));
            Assert.IsFalse(RSSService.IsValidRSSFeedURL("https://nonexisting.dsdsd/info.rss"));
        }

        [TestMethod()]
        public void GetFeedURLForSubredditTest()
        {
            string s = null;
            
            Assert.AreEqual(
                "https://www.reddit.com/r/aww/new/.rss", 
                RSSService.GetFeedURLForSubreddit("aww", out s)
            );
            Assert.AreEqual(s, "/r/aww");
            
            Assert.AreEqual(
                "https://www.reddit.com/r/aww/new/.rss",
                RSSService.GetFeedURLForSubreddit("r/aww", out s)
            );
            Assert.AreEqual(s, "/r/aww");
            
            Assert.AreEqual(
                "https://www.reddit.com/r/aww/new/.rss",
                RSSService.GetFeedURLForSubreddit("/r/aww", out s)
            );
            Assert.AreEqual(s, "/r/aww");
            
            Assert.AreEqual(
                "https://www.reddit.com/r/aww/new/.rss",
                RSSService.GetFeedURLForSubreddit("/aww", out s)
            );
            Assert.AreEqual(s, "/r/aww");
            
            Assert.AreEqual(
                "https://www.reddit.com/r/aww/new/.rss",
                RSSService.GetFeedURLForSubreddit("/AwW", out s)
            );
            Assert.AreEqual(s, "/r/aww");
            Assert.AreEqual(
                "https://www.reddit.com/r/globaloffensive/new/.rss",
                RSSService.GetFeedURLForSubreddit("GlobalOffensive", out s)
            );
            Assert.AreEqual(s, "/r/globaloffensive");
        }

        [TestMethod()]
        public void GetFeedResultsTest()
        {
            Assert.IsTrue(RSSService.GetFeedResults("https://www.reddit.com/r/aww/new/.rss").Any());
            Assert.IsTrue(RSSService.GetFeedResults("https://www.reddit.com/r/aww/new/.rss").Count() == 5);
            Assert.IsTrue(RSSService.GetFeedResults("https://www.reddit.com/r/aww/new/.rss", 10).Count() == 10);
            Assert.IsTrue(RSSService.GetFeedResults("https://www.reddit.com/r/aww/new/.rss", 10).Count() == 10);
            Assert.IsTrue(RSSService.GetFeedResults("https://www.reddit.com/r/MrRobot/.rss").Any());
            Assert.IsTrue(RSSService.GetFeedResults("https://news.google.com/news/rss/headlines/section/topic/WORLD?ned=us&hl=en").Any());
            Assert.IsTrue(RSSService.GetFeedResults("https://www.youtube.com/feeds/videos.xml?channel_id=UCA5u8UquvO44Jcd3wZApyDg").Any());
        }
    }
}