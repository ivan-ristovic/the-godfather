using NUnit.Framework;
using TheGodfather.Modules.Misc.Services;

namespace TheGodfather.Tests.Modules.Misc.Services;

[TestFixture]
public sealed class MemeGenServiceTests
{
    private const string ApiUrl = "https://api.memegen.link/images";

    [Test]
    public void GenerateMemeTest()
    {
        Assert.That(
            MemeGenService.GenerateMemeUrl("buzz", "top", "bottom"),
            Is.EqualTo($"{ApiUrl}/buzz/top/bottom.jpg?font=impact")
        );
        Assert.That(
            MemeGenService.GenerateMemeUrl("buzz", "top?", "bottom"),
            Is.EqualTo($"{ApiUrl}/buzz/top~q/bottom.jpg?font=impact")
        );
        Assert.That(
            MemeGenService.GenerateMemeUrl("buzz", "top", "bottom?"),
            Is.EqualTo($"{ApiUrl}/buzz/top/bottom~q.jpg?font=impact")
        );
        Assert.That(
            MemeGenService.GenerateMemeUrl("buzz", "top%", "bottom"),
            Is.EqualTo($"{ApiUrl}/buzz/top~p/bottom.jpg?font=impact")
        );
        Assert.That(
            MemeGenService.GenerateMemeUrl("buzz", "top", "bottom%"),
            Is.EqualTo($"{ApiUrl}/buzz/top/bottom~p.jpg?font=impact")
        );
        Assert.That(
            MemeGenService.GenerateMemeUrl("buzz", "top#", "bottom"),
            Is.EqualTo($"{ApiUrl}/buzz/top~h/bottom.jpg?font=impact")
        );
        Assert.That(
            MemeGenService.GenerateMemeUrl("buzz", "top", "bottom#"),
            Is.EqualTo($"{ApiUrl}/buzz/top/bottom~h.jpg?font=impact")
        );
        Assert.That(
            MemeGenService.GenerateMemeUrl("buzz", "top/", "bottom"),
            Is.EqualTo($"{ApiUrl}/buzz/top~s/bottom.jpg?font=impact")
        );
        Assert.That(
            MemeGenService.GenerateMemeUrl("buzz", "top", "bottom/"),
            Is.EqualTo($"{ApiUrl}/buzz/top/bottom~s.jpg?font=impact")
        );
        Assert.That(
            MemeGenService.GenerateMemeUrl("buzz", "top?", "bottom"),
            Is.EqualTo($"{ApiUrl}/buzz/top~q/bottom.jpg?font=impact")
        );
        Assert.That(
            MemeGenService.GenerateMemeUrl("buzz", "top text", "bottom"),
            Is.EqualTo($"{ApiUrl}/buzz/top-text/bottom.jpg?font=impact")
        );
        Assert.That(
            MemeGenService.GenerateMemeUrl("buzz", "top-", "bottom"),
            Is.EqualTo($"{ApiUrl}/buzz/top--/bottom.jpg?font=impact")
        );
        Assert.That(
            MemeGenService.GenerateMemeUrl("buzz", "top", "bottom-"),
            Is.EqualTo($"{ApiUrl}/buzz/top/bottom--.jpg?font=impact")
        );
        Assert.That(
            MemeGenService.GenerateMemeUrl("buzz", "top_", "bottom"),
            Is.EqualTo($"{ApiUrl}/buzz/top__/bottom.jpg?font=impact")
        );
        Assert.That(
            MemeGenService.GenerateMemeUrl("buzz", "top", "bottom_"),
            Is.EqualTo($"{ApiUrl}/buzz/top/bottom__.jpg?font=impact")
        );
        Assert.That(
            MemeGenService.GenerateMemeUrl("buzz", "top\"", "bottom"),
            Is.EqualTo($"{ApiUrl}/buzz/top''/bottom.jpg?font=impact")
        );
        Assert.That(
            MemeGenService.GenerateMemeUrl("buzz", "top", "bottom_"),
            Is.EqualTo($"{ApiUrl}/buzz/top/bottom__.jpg?font=impact")
        );
        Assert.That(
            MemeGenService.GenerateMemeUrl("tmp", "top__", "bottom"),
            Is.EqualTo($"{ApiUrl}/tmp/top____/bottom.jpg?font=impact")
        );
        Assert.That(
            MemeGenService.GenerateMemeUrl("tmp", "top", "remove multiple      space"),
            Is.EqualTo($"{ApiUrl}/tmp/top/remove-multiple-space.jpg?font=impact")
        );
        Assert.That(
            MemeGenService.GenerateMemeUrl("tmp", "remove multiple      space", "bottom"),
            Is.EqualTo($"{ApiUrl}/tmp/remove-multiple-space/bottom.jpg?font=impact")
        );
        Assert.That(
            MemeGenService.GenerateMemeUrl("tmp", "", null),
            Is.EqualTo($"{ApiUrl}/tmp/_/_.jpg?font=impact")
        );
        Assert.That(
            MemeGenService.GenerateMemeUrl("tmp", null, ""),
            Is.EqualTo($"{ApiUrl}/tmp/_/_.jpg?font=impact")
        );
        Assert.That(
            MemeGenService.GenerateMemeUrl("tmp", " ", null),
            Is.EqualTo($"{ApiUrl}/tmp/_/_.jpg?font=impact")
        );
        Assert.That(
            MemeGenService.GenerateMemeUrl("tmp", null, " "),
            Is.EqualTo($"{ApiUrl}/tmp/_/_.jpg?font=impact")
        );
    }
}