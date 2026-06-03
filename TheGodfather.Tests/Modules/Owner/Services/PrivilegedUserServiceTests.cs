using TheGodfather.Modules.Owner.Services;

namespace TheGodfather.Tests.Modules.Owner.Services;

[TestFixture]
public sealed class PrivilegedUserServiceTests : ITheGodfatherServiceTest<PrivilegedUserService>
{
    public PrivilegedUserService Service { get; private set; } = null!;


    [SetUp]
    public void InitializeService()
        => this.Service = new PrivilegedUserService(TestDbProvider.Database);


    [Test]
    public async Task AddRemoveAndQueryPrivilegedUsers()
    {
        await TestDbProvider.AlterAndVerifyAsync(
            async _ => {
                Assert.That(await this.Service.AddAsync(MockData.Ids.Take(2)), Is.EqualTo(2));
                Assert.That(await this.Service.AddAsync(MockData.Ids[0]), Is.Zero);
                Assert.That(await this.Service.ContainsAsync(MockData.Ids[1]), Is.True);
                Assert.That(await this.Service.RemoveAsync(MockData.Ids[0]), Is.EqualTo(1));
            },
            async _ => {
                Assert.That(this.Service.GetIds(), Is.EqualTo(new[] { MockData.Ids[1] }));
                Assert.That(await this.Service.ContainsAsync(MockData.Ids[0]), Is.False);
            }
        );
    }
}
