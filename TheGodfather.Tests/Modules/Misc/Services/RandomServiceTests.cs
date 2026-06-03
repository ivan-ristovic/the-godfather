using TheGodfather.Modules.Misc.Services;

namespace TheGodfather.Tests.Modules.Misc.Services;

[TestFixture]
public sealed class RandomServiceTests : ITheGodfatherServiceTest<RandomService>
{
    public RandomService Service { get; private set; } = null!;


    [SetUp]
    public void InitializeService()
        => this.Service = new RandomService(loadData: false);


    [Test]
    public void DeterministicHelpersReturnExpectedValues()
    {
        Assert.That(this.Service.Color(0).ToString(), Is.EqualTo("https://www.color-hex.com/color/4b3932"));
        Assert.That(this.Service.Size(3), Is.EqualTo("8===D"));
        Assert.That(this.Service.FromLeet("h3ll0"), Is.EqualTo("Hello"));
    }

    [Test]
    public void RandomHelpersStayInsideTheirContracts()
    {
        Assert.That(this.Service.Dice(), Is.InRange(1, 6));
        Assert.That(this.Service.Choice("red, blue, red"), Is.AnyOf("red", "blue"));
        Assert.That(this.Service.GetRandomYesNoAnswer().Key, Does.StartWith("eightball-"));
        Assert.That(this.Service.GetRandomTimeAnswer().Key, Does.StartWith("eightball-t-"));
        Assert.That(this.Service.GetRandomQuantityAnswer().Key, Does.StartWith("eightball-q-"));
    }
}
