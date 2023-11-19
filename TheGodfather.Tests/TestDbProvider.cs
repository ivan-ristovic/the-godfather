using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using TheGodfather.Database;
using TheGodfather.Database.Models;

namespace TheGodfather.Tests;

public static class TestDbProvider
{
    public static DbContextBuilder Database { get; }
    public static string ConnectionString { get; }
    public static SqliteConnection DatabaseConnection { get; }


    static TestDbProvider()
    {
        ConnectionString = "DataSource=:memory:;foreign keys=true;";
        DatabaseConnection = new SqliteConnection(ConnectionString);

        var cfg = new DbConfig {
            Provider = DbProvider.SqliteInMemory
        };
        DbContextOptions<TheGodfatherDbContext> options = new DbContextOptionsBuilder<TheGodfatherDbContext>()
            .UseSqlite(DatabaseConnection)
            .UseLazyLoadingProxies()
            .Options;

        Database = new DbContextBuilder(cfg, options);
    }


    public static void Verify(Action<TheGodfatherDbContext> verify)
        => InternalSetupAlterAndVerify(null, null, verify);

    public static Task VerifyAsync(Func<TheGodfatherDbContext, Task> verify)
        => InternalSetupAlterAndVerifyAsync(null, null, verify);

    public static void SetupAndVerify(Action<TheGodfatherDbContext> setup, Action<TheGodfatherDbContext> verify, bool ensureSave = false)
        => InternalSetupAlterAndVerify(setup, null, verify, ensureSave);

    public static Task SetupAndVerifyAsync(Func<TheGodfatherDbContext, Task> setup, Func<TheGodfatherDbContext, Task> verify, bool ensureSave = false)
        => InternalSetupAlterAndVerifyAsync(setup, null, verify, ensureSave);

    public static void AlterAndVerify(Action<TheGodfatherDbContext> alter, Action<TheGodfatherDbContext> verify, bool ensureSave = false)
        => InternalSetupAlterAndVerify(null, alter, verify, ensureSave);

    public static Task AlterAndVerifyAsync(Func<TheGodfatherDbContext, Task> alter, Func<TheGodfatherDbContext, Task> verify, bool ensureSave = false)
        => InternalSetupAlterAndVerifyAsync(null, alter, verify, ensureSave);

    public static void SetupAlterAndVerify(Action<TheGodfatherDbContext> setup,
        Action<TheGodfatherDbContext> alter,
        Action<TheGodfatherDbContext> verify,
        bool ensureSave = false)
        => InternalSetupAlterAndVerify(setup, alter, verify, ensureSave);

    public static Task SetupAlterAndVerifyAsync(Func<TheGodfatherDbContext, Task> setup,
        Func<TheGodfatherDbContext, Task> alter,
        Func<TheGodfatherDbContext, Task> verify,
        bool ensureSave = false)
        => InternalSetupAlterAndVerifyAsync(setup, alter, verify, ensureSave);


    private static void CreateDatabase()
    {
        using TheGodfatherDbContext context = Database.CreateContext();
        context.Database.EnsureCreated();
    }

    private static void SeedGuildData()
    {
        using TheGodfatherDbContext context = Database.CreateContext();
        context.Configs.AddRange(MockData.Ids.Select(id => new GuildConfig { GuildId = id }));
        context.SaveChanges();
    }

    private static void InternalSetupAlterAndVerify(Action<TheGodfatherDbContext>? setup,
        Action<TheGodfatherDbContext>? alter,
        Action<TheGodfatherDbContext>? verify,
        bool ensureSave = false)
    {
        DatabaseConnection.Open();
        try {
            CreateDatabase();
            SeedGuildData();

            if (setup is not null) {
                using TheGodfatherDbContext context = Database.CreateContext();
                setup(context);
                context.SaveChanges();
            }

            if (alter is not null) {
                using TheGodfatherDbContext context = Database.CreateContext();
                alter(context);
                if (ensureSave)
                    context.SaveChanges();
            }

            if (verify is not null) {
                using TheGodfatherDbContext context = Database.CreateContext();
                verify(context);
            }
        } finally {
            DatabaseConnection.Close();
        }
    }

    private static async Task InternalSetupAlterAndVerifyAsync(Func<TheGodfatherDbContext, Task>? setup,
        Func<TheGodfatherDbContext, Task>? alter,
        Func<TheGodfatherDbContext, Task>? verify,
        bool ensureSave = false)
    {
        DatabaseConnection.Open();
        try {
            CreateDatabase();
            SeedGuildData();

            if (setup is not null) {
                await using TheGodfatherDbContext context = Database.CreateContext();
                await setup(context);
                await context.SaveChangesAsync();
            }

            if (alter is not null) {
                await using TheGodfatherDbContext context = Database.CreateContext();
                await alter(context);
                if (ensureSave)
                    await context.SaveChangesAsync();
            }

            if (verify is not null) {
                await using TheGodfatherDbContext context = Database.CreateContext();
                await verify(context);
            }
        } finally {
            DatabaseConnection.Close();
        }
    }
}