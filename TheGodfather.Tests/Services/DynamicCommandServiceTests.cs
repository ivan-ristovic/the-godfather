namespace TheGodfather.Tests.Services;

public sealed class DynamicCommandServiceTests : ITheGodfatherServiceTest<DynamicCommandService>
{
    public DynamicCommandService Service { get; private set; } = null!;


    [SetUp]
    public void InitializeService()
    {
        this.Service = new DynamicCommandService();
    }


    [Test]
    public void AddNormalizesCommandNamesAndRejectsDuplicates()
    {
        Assert.That(this.Service.Add(" Hello ", "world"), Is.True);
        Assert.That(this.Service.Add("hello", "again"), Is.False);

        IReadOnlyDictionary<string, string> commands = this.Service.GetAll();
        Assert.That(commands, Has.Count.EqualTo(1));
        Assert.That(commands["hello"], Is.EqualTo("world"));
    }

    [Test]
    public void RemoveIsCaseInsensitive()
    {
        Assert.That(this.Service.Add("Hello", "world"), Is.True);
        Assert.That(this.Service.Remove("HELLO"), Is.True);
        Assert.That(this.Service.Remove("hello"), Is.False);
        Assert.That(this.Service.GetAll(), Is.Empty);
    }

    [Test]
    public void GetAllReturnsCommandsSortedByName()
    {
        this.Service.Add("zeta", "last");
        this.Service.Add("alpha", "first");

        Assert.That(this.Service.GetAll().Keys, Is.EqualTo(new[] { "alpha", "zeta" }));
    }
}
