using NUnit.Framework;

using TheGodfather.Modules.Misc.Services;

namespace TheGodfather.Tests.Modules.Misc.Services
{
    [TestFixture]
    public sealed class MemeGenServiceTests
    {
        [Test]
        public void GenerateMemeTest()
        {
            Assert.That(
                MemeGenService.GenerateMemeUrl("tmp", "top", "bottom"),
                Is.EqualTo("http://memegen.link/tmp/top/bottom.jpg?font=impact")
            );
            Assert.That(
                MemeGenService.GenerateMemeUrl("tmp", "top?", "bottom"),
                Is.EqualTo("http://memegen.link/tmp/top~q/bottom.jpg?font=impact")
            );
            Assert.That(
                MemeGenService.GenerateMemeUrl("tmp", "top", "bottom?"),
                Is.EqualTo("http://memegen.link/tmp/top/bottom~q.jpg?font=impact")
            );
            Assert.That(
                MemeGenService.GenerateMemeUrl("tmp", "top%", "bottom"),
                Is.EqualTo("http://memegen.link/tmp/top~p/bottom.jpg?font=impact")
            );
            Assert.That(
                MemeGenService.GenerateMemeUrl("tmp", "top", "bottom%"),
                Is.EqualTo("http://memegen.link/tmp/top/bottom~p.jpg?font=impact")
            );
            Assert.That(
                MemeGenService.GenerateMemeUrl("tmp", "top#", "bottom"),
                Is.EqualTo("http://memegen.link/tmp/top~h/bottom.jpg?font=impact")
            );
            Assert.That(
                MemeGenService.GenerateMemeUrl("tmp", "top", "bottom#"),
                Is.EqualTo("http://memegen.link/tmp/top/bottom~h.jpg?font=impact")
            );
            Assert.That(
                MemeGenService.GenerateMemeUrl("tmp", "top/", "bottom"),
                Is.EqualTo("http://memegen.link/tmp/top~s/bottom.jpg?font=impact")
            );
            Assert.That(
                MemeGenService.GenerateMemeUrl("tmp", "top", "bottom/"),
                Is.EqualTo("http://memegen.link/tmp/top/bottom~s.jpg?font=impact")
            );
            Assert.That(
                MemeGenService.GenerateMemeUrl("tmp", "top?", "bottom"),
                Is.EqualTo("http://memegen.link/tmp/top~q/bottom.jpg?font=impact")
            );
            Assert.That(
                MemeGenService.GenerateMemeUrl("tmp", "top text", "bottom"),
                Is.EqualTo("http://memegen.link/tmp/top-text/bottom.jpg?font=impact")
            );
            Assert.That(
                MemeGenService.GenerateMemeUrl("tmp", "top-", "bottom"),
                Is.EqualTo("http://memegen.link/tmp/top--/bottom.jpg?font=impact")
            );
            Assert.That(
                MemeGenService.GenerateMemeUrl("tmp", "top", "bottom-"),
                Is.EqualTo("http://memegen.link/tmp/top/bottom--.jpg?font=impact")
            );
            Assert.That(
                MemeGenService.GenerateMemeUrl("tmp", "top_", "bottom"),
                Is.EqualTo("http://memegen.link/tmp/top__/bottom.jpg?font=impact")
            );
            Assert.That(
                MemeGenService.GenerateMemeUrl("tmp", "top", "bottom_"),
                Is.EqualTo("http://memegen.link/tmp/top/bottom__.jpg?font=impact")
            );
            Assert.That(
                MemeGenService.GenerateMemeUrl("tmp", "top\"", "bottom"),
                Is.EqualTo("http://memegen.link/tmp/top''/bottom.jpg?font=impact")
            );
            Assert.That(
                MemeGenService.GenerateMemeUrl("tmp", "top", "bottom_"),
                Is.EqualTo("http://memegen.link/tmp/top/bottom__.jpg?font=impact")
            );
            Assert.That(
                MemeGenService.GenerateMemeUrl("tmp", "top__", "bottom"),
                Is.EqualTo("http://memegen.link/tmp/top____/bottom.jpg?font=impact")
            );
            Assert.That(
                MemeGenService.GenerateMemeUrl("tmp", "top", "remove multiple      space"),
                Is.EqualTo("http://memegen.link/tmp/top/remove-multiple-space.jpg?font=impact")
            );
            Assert.That(
                MemeGenService.GenerateMemeUrl("tmp", "remove multiple      space", "bottom"),
                Is.EqualTo("http://memegen.link/tmp/remove-multiple-space/bottom.jpg?font=impact")
            );
            Assert.That(
                MemeGenService.GenerateMemeUrl("tmp", "", null),
                Is.EqualTo("http://memegen.link/tmp/_/_.jpg?font=impact")
            );
            Assert.That(
                MemeGenService.GenerateMemeUrl("tmp", null, ""),
                Is.EqualTo("http://memegen.link/tmp/_/_.jpg?font=impact")
            );
            Assert.That(
                MemeGenService.GenerateMemeUrl("tmp", " ", null),
                Is.EqualTo("http://memegen.link/tmp/_/_.jpg?font=impact")
            );
            Assert.That(
                MemeGenService.GenerateMemeUrl("tmp", null, " "),
                Is.EqualTo("http://memegen.link/tmp/_/_.jpg?font=impact")
            );
        }
    }
}
