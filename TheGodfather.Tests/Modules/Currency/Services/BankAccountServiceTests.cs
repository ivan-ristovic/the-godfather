using TheGodfather.Modules.Currency.Services;

namespace TheGodfather.Tests.Modules.Currency.Services;

[TestFixture]
public sealed class BankAccountServiceTests : ITheGodfatherServiceTest<BankAccountService>
{
    public BankAccountService Service { get; private set; } = null!;


    [SetUp]
    public void InitializeService()
        => this.Service = new BankAccountService(TestDbProvider.Database);


    [Test]
    public async Task ModifyAndDecreasePersistBalances()
    {
        await TestDbProvider.AlterAndVerifyAsync(
            async _ => {
                await this.Service.IncreaseBankAccountAsync(MockData.Ids[0], MockData.Ids[1], 250);
                Assert.That(await this.Service.TryDecreaseBankAccountAsync(MockData.Ids[0], MockData.Ids[1], 300), Is.True);
                Assert.That(await this.Service.TryDecreaseBankAccountAsync(MockData.Ids[0], MockData.Ids[1], BankAccount.StartingBalance * 2), Is.False);
                await this.Service.ModifyBankAccountAsync(MockData.Ids[0], MockData.Ids[2], balance => balance - BankAccount.StartingBalance * 2);
            },
            async db => {
                BankAccount? first = await db.BankAccounts.FindAsync((long)MockData.Ids[0], (long)MockData.Ids[1]);
                BankAccount? second = await db.BankAccounts.FindAsync((long)MockData.Ids[0], (long)MockData.Ids[2]);
                Assert.That(first, Is.Not.Null);
                Assert.That(first!.Balance, Is.EqualTo(BankAccount.StartingBalance - 50));
                Assert.That(second, Is.Not.Null);
                Assert.That(second!.Balance, Is.Zero);
            }
        );
    }

    [Test]
    public async Task TransferPersistsBothSidesAndRejectsInsufficientFunds()
    {
        await TestDbProvider.SetupAlterAndVerifyAsync(
            db => {
                db.BankAccounts.Add(new BankAccount { GuildId = MockData.Ids[0], UserId = MockData.Ids[1], Balance = 750 });
                db.BankAccounts.Add(new BankAccount { GuildId = MockData.Ids[0], UserId = MockData.Ids[2], Balance = 100 });
                return Task.CompletedTask;
            },
            async _ => {
                Assert.That(await this.Service.TransferAsync(MockData.Ids[0], MockData.Ids[1], MockData.Ids[2], 250), Is.True);
                Assert.That(await this.Service.TransferAsync(MockData.Ids[0], MockData.Ids[1], MockData.Ids[2], 1000), Is.False);
            },
            async db => {
                BankAccount? source = await db.BankAccounts.FindAsync((long)MockData.Ids[0], (long)MockData.Ids[1]);
                BankAccount? destination = await db.BankAccounts.FindAsync((long)MockData.Ids[0], (long)MockData.Ids[2]);
                Assert.That(source!.Balance, Is.EqualTo(500));
                Assert.That(destination!.Balance, Is.EqualTo(350));
            }
        );
    }

    [Test]
    public async Task RemoveAllDeletesUserAccountsAcrossGuilds()
    {
        await TestDbProvider.SetupAlterAndVerifyAsync(
            db => {
                db.BankAccounts.Add(new BankAccount { GuildId = MockData.Ids[0], UserId = MockData.Ids[1] });
                db.BankAccounts.Add(new BankAccount { GuildId = MockData.Ids[1], UserId = MockData.Ids[1] });
                db.BankAccounts.Add(new BankAccount { GuildId = MockData.Ids[1], UserId = MockData.Ids[2] });
                return Task.CompletedTask;
            },
            _ => this.Service.RemoveAllAsync(MockData.Ids[1]),
            db => {
                Assert.That(db.BankAccounts.AsEnumerable().Select(acc => acc.UserId), Is.EquivalentTo(new[] { MockData.Ids[2] }));
                return Task.CompletedTask;
            }
        );
    }
}
