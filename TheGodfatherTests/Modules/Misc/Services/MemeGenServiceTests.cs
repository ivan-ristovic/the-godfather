#region USING_DIRECTIVES
using NUnit.Framework;

using System.Collections.Generic;
using System.Threading.Tasks;

using TheGodfather.Modules.Misc.Services;
#endregion

namespace TheGodfatherTests.Modules.Misc.Services
{
    [TestFixture]
    public class MemeGenServiceTests
    {
        [Test]
        public void GenerateMemeTest()
        {
            Assert.AreEqual(
                "http://memegen.link/tmp/top/bottom.jpg?font=impact",
                MemeGenService.GenerateMeme("tmp", "top", "bottom")
            );
            Assert.AreEqual(
                "http://memegen.link/tmp/top~q/bottom.jpg?font=impact",
                MemeGenService.GenerateMeme("tmp", "top?", "bottom")
            );
            Assert.AreEqual(
                "http://memegen.link/tmp/top/bottom~q.jpg?font=impact",
                MemeGenService.GenerateMeme("tmp", "top", "bottom?")
            );
            Assert.AreEqual(
                "http://memegen.link/tmp/top~p/bottom.jpg?font=impact",
                MemeGenService.GenerateMeme("tmp", "top%", "bottom")
            );
            Assert.AreEqual(
                "http://memegen.link/tmp/top/bottom~p.jpg?font=impact",
                MemeGenService.GenerateMeme("tmp", "top", "bottom%")
            );
            Assert.AreEqual(
                "http://memegen.link/tmp/top~h/bottom.jpg?font=impact",
                MemeGenService.GenerateMeme("tmp", "top#", "bottom")
            );
            Assert.AreEqual(
                "http://memegen.link/tmp/top/bottom~h.jpg?font=impact",
                MemeGenService.GenerateMeme("tmp", "top", "bottom#")
            );
            Assert.AreEqual(
                "http://memegen.link/tmp/top~s/bottom.jpg?font=impact",
                MemeGenService.GenerateMeme("tmp", "top/", "bottom")
            );
            Assert.AreEqual(
                "http://memegen.link/tmp/top/bottom~s.jpg?font=impact",
                MemeGenService.GenerateMeme("tmp", "top", "bottom/")
            );
            Assert.AreEqual(
                "http://memegen.link/tmp/top~q/bottom.jpg?font=impact",
                MemeGenService.GenerateMeme("tmp", "top?", "bottom")
            );
            Assert.AreEqual(
                "http://memegen.link/tmp/top-text/bottom.jpg?font=impact",
                MemeGenService.GenerateMeme("tmp", "top text", "bottom")
            );
            Assert.AreEqual(
                "http://memegen.link/tmp/top--/bottom.jpg?font=impact",
                MemeGenService.GenerateMeme("tmp", "top-", "bottom")
            );
            Assert.AreEqual(
                "http://memegen.link/tmp/top/bottom--.jpg?font=impact",
                MemeGenService.GenerateMeme("tmp", "top", "bottom-")
            );
            Assert.AreEqual(
                "http://memegen.link/tmp/top__/bottom.jpg?font=impact",
                MemeGenService.GenerateMeme("tmp", "top_", "bottom")
            );
            Assert.AreEqual(
                "http://memegen.link/tmp/top/bottom__.jpg?font=impact",
                MemeGenService.GenerateMeme("tmp", "top", "bottom_")
            );
            Assert.AreEqual(
                "http://memegen.link/tmp/top''/bottom.jpg?font=impact",
                MemeGenService.GenerateMeme("tmp", "top\"", "bottom")
            );
            Assert.AreEqual(
                "http://memegen.link/tmp/top/bottom__.jpg?font=impact",
                MemeGenService.GenerateMeme("tmp", "top", "bottom_")
            );
            Assert.AreEqual(
                "http://memegen.link/tmp/top____/bottom.jpg?font=impact",
                MemeGenService.GenerateMeme("tmp", "top__", "bottom")
            );
            Assert.AreEqual(
                "http://memegen.link/tmp/top/remove-multiple-space.jpg?font=impact",
                MemeGenService.GenerateMeme("tmp", "top", "remove multiple      space")
            );
            Assert.AreEqual(
                "http://memegen.link/tmp/remove-multiple-space/bottom.jpg?font=impact",
                MemeGenService.GenerateMeme("tmp", "remove multiple      space", "bottom")
            );
            Assert.AreEqual(
                "http://memegen.link/tmp/_/_.jpg?font=impact",
                MemeGenService.GenerateMeme("tmp", "", null)
            );
            Assert.AreEqual(
                "http://memegen.link/tmp/_/_.jpg?font=impact",
                MemeGenService.GenerateMeme("tmp", null, "")
            );
            Assert.AreEqual(
                "http://memegen.link/tmp/_/_.jpg?font=impact",
                MemeGenService.GenerateMeme("tmp", " ", null)
            );
            Assert.AreEqual(
                "http://memegen.link/tmp/_/_.jpg?font=impact",
                MemeGenService.GenerateMeme("tmp", null, " ")
            );
        }

        [Test]
        public async Task GetMemeTemplatesAsync()
        {
            IReadOnlyList<string> templates = await MemeGenService.GetMemeTemplatesAsync();
            Assert.IsNotNull(templates);
            CollectionAssert.AllItemsAreNotNull(templates);
            CollectionAssert.AllItemsAreUnique(templates);
            CollectionAssert.IsOrdered(templates);
        }
    }
}
