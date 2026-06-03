using TheGodfather.Modules.Currency.Services;

namespace TheGodfather.Tests.Modules.Currency.Services;

[TestFixture]
public sealed class ShopServiceTests : ITheGodfatherServiceTest<ShopService>
{
    public ShopService Service { get; private set; } = null!;


    [SetUp]
    public void InitializeService()
        => this.Service = new ShopService(TestDbProvider.Database);


    [Test]
    public async Task ItemsAndPurchasesPersistWithExpectedScopes()
    {
        await TestDbProvider.AlterAndVerifyAsync(
            async _ => {
                Assert.That(await this.Service.AddAsync(new PurchasableItem {
                    Id = 10,
                    GuildId = MockData.Ids[0],
                    Name = "VIP",
                    Price = 250
                }), Is.EqualTo(1));
                Assert.That(await this.Service.AddAsync(new PurchasableItem {
                    Id = 11,
                    GuildId = MockData.Ids[1],
                    Name = "Color",
                    Price = 125
                }), Is.EqualTo(1));

                Assert.That(await this.Service.Purchases.AddAsync(MockData.Ids[2], 10), Is.EqualTo(1));
                Assert.That(await this.Service.Purchases.AddAsync(MockData.Ids[2], 10), Is.Zero);
            },
            async _ => {
                Assert.That(this.Service.GetIds(MockData.Ids[0]), Is.EqualTo(new[] { 10 }));
                Assert.That(this.Service.GetIds(MockData.Ids[1]), Is.EqualTo(new[] { 11 }));

                IReadOnlyList<PurchasedItem> purchases = await this.Service.Purchases.GetAllCompleteAsync(MockData.Ids[2]);
                Assert.That(purchases, Has.Exactly(1).Items);
                Assert.That(purchases.Single().Item.Name, Is.EqualTo("VIP"));
            }
        );
    }
}

[TestFixture]
public sealed class WorkServiceTests : ITheGodfatherServiceTest<WorkService>
{
    public WorkService Service { get; private set; } = null!;

    private BankAccountService bankAccounts = null!;


    [SetUp]
    public void InitializeService()
    {
        var cfg = new BotConfigService();
        var guildConfig = new GuildConfigService(cfg, TestDbProvider.Database, false);
        var localization = new LocalizationService(guildConfig, cfg);

        this.bankAccounts = new BankAccountService(TestDbProvider.Database);
        this.Service = new WorkService(localization, this.bankAccounts);
    }


    [Test]
    public async Task WorkActionsReturnLocalizedTextAndPersistBalanceChanges()
    {
        await TestDbProvider.AlterAndVerifyAsync(
            async _ => {
                string work = await this.Service.WorkAsync(MockData.Ids[0], MockData.Ids[1], "coins");
                string crime = await this.Service.CrimeAsync(MockData.Ids[0], MockData.Ids[1], "coins");
                string streets = await this.Service.StreetsAsync(MockData.Ids[0], MockData.Ids[1], "coins");

                Assert.That(work, Is.Not.Empty);
                Assert.That(crime, Is.Not.Empty);
                Assert.That(streets, Is.Not.Empty);
            },
            async _ => {
                BankAccount? account = await this.bankAccounts.GetAsync(MockData.Ids[0], MockData.Ids[1]);
                Assert.That(account, Is.Not.Null);
                Assert.That(account!.Balance, Is.GreaterThanOrEqualTo(0));
            }
        );
    }
}
