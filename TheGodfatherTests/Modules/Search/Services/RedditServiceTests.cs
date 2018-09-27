#region USING_DIRECTIVES
using NUnit.Framework;

using System;

using TheGodfather.Modules.Search.Services;
#endregion

namespace TheGodfatherTests.Modules.Search.Services
{
    [TestFixture]
    public class RedditServiceTests
    {
        [Test]
        public void GetFeedURLForSubredditTest()
        {
            string sub = null;
            string aww = "https://www.reddit.com/r/aww/new/.rss";
            string csgo = "https://www.reddit.com/r/globaloffensive/new/.rss";

            Assert.AreEqual(aww, RedditService.GetFeedURLForSubreddit("/r/aww", RedditCategory.New, out sub));
            Assert.AreEqual(sub, "/r/aww");
            Assert.AreEqual(aww, RedditService.GetFeedURLForSubreddit("r/aww", RedditCategory.New, out sub));
            Assert.AreEqual(sub, "/r/aww");
            Assert.AreEqual(aww, RedditService.GetFeedURLForSubreddit("/aww", RedditCategory.New, out sub));
            Assert.AreEqual(sub, "/r/aww");
            Assert.AreEqual(aww, RedditService.GetFeedURLForSubreddit("/r/aWW", RedditCategory.New, out sub));
            Assert.AreEqual(sub, "/r/aww");
            Assert.AreEqual(aww, RedditService.GetFeedURLForSubreddit("/R/aww", RedditCategory.New, out sub));
            Assert.AreEqual(sub, "/r/aww");
            Assert.AreEqual(aww, RedditService.GetFeedURLForSubreddit("/AWW", RedditCategory.New, out sub));
            Assert.AreEqual(sub, "/r/aww");
            Assert.AreEqual(csgo, RedditService.GetFeedURLForSubreddit("GlobalOffensive", RedditCategory.New, out sub));
            Assert.AreEqual(sub, "/r/globaloffensive");

            Assert.Throws<ArgumentException>(() => RedditService.GetFeedURLForSubreddit("Global Offensive", RedditCategory.New, out sub));
            Assert.Throws<ArgumentException>(() => RedditService.GetFeedURLForSubreddit("Global.Offensive", RedditCategory.New, out sub));
            Assert.Throws<ArgumentException>(() => RedditService.GetFeedURLForSubreddit("Global-Offensive", RedditCategory.New, out sub));
            Assert.Throws<ArgumentException>(() => RedditService.GetFeedURLForSubreddit("Global*Offensive", RedditCategory.New, out sub));
            Assert.Throws<ArgumentException>(() => RedditService.GetFeedURLForSubreddit("Global?Offensive", RedditCategory.New, out sub));
            Assert.Throws<ArgumentException>(() => RedditService.GetFeedURLForSubreddit("Global??Offensive", RedditCategory.New, out sub));
            Assert.Throws<ArgumentException>(() => RedditService.GetFeedURLForSubreddit("Global>Offensive", RedditCategory.New, out sub));
            Assert.Throws<ArgumentException>(() => RedditService.GetFeedURLForSubreddit("Global??Off>ensive", RedditCategory.New, out sub));
            Assert.Throws<ArgumentException>(() => RedditService.GetFeedURLForSubreddit(".", RedditCategory.New, out sub));
            Assert.Throws<ArgumentException>(() => RedditService.GetFeedURLForSubreddit("---", RedditCategory.New, out sub));

            Assert.IsNull(RedditService.GetFeedURLForSubreddit("FOOASDSADSANDJSKANDSKJANDSKAD", RedditCategory.New, out sub));
        }
    }
}
