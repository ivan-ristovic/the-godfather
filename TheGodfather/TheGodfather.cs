using System.Diagnostics;
using Microsoft.EntityFrameworkCore;

namespace TheGodfather;

internal static class TheGodfather
{
    public static string ApplicationName { get; }
    public static string ApplicationVersion { get; }

    internal static TheGodfatherBot? Bot { get; private set; }


    static TheGodfather()
    {
        AssemblyName info = Assembly.GetExecutingAssembly().GetName();
        ApplicationName = info.Name ?? "TheGodfather";
        ApplicationVersion = $"v{info.Version?.ToString() ?? "<unknown>"}";
    }

    internal static async Task Main(string[] _)
    {
        PrintBuildInformation();

        try {
            BotConfigService cfg = await LoadBotConfigAsync();
            Log.Logger = LogExt.CreateLogger(cfg.CurrentConfiguration);
            Log.Information("Logger created");

            DbContextBuilder dbb = await InitializeDatabaseAsync(cfg);

            Bot = new TheGodfatherBot(cfg, dbb);
            await Bot.StartAsync();
            Log.Information("Booting complete!");

            Console.CancelKeyPress += (_, e) => {
                e.Cancel = true;
                Bot.Services.GetRequiredService<BotActivityService>().MainLoopCts.Cancel();
            };

            await Task.Delay(Timeout.Infinite, Bot.Services.GetRequiredService<BotActivityService>().MainLoopCts.Token);
        } catch (TaskCanceledException) {
            Log.Information("Shutdown signal received");
        } catch (Exception e) {
            Log.Fatal(e, "Critical exception occurred");
            Environment.ExitCode = 1;
        } finally {
            if (Bot is not null)
                await Bot.DisposeAsync();

            Log.Information("Powering off");
            await Log.CloseAndFlushAsync();
        }

        Environment.Exit(Environment.ExitCode);
    }

    public static Task Stop(int exitCode = 0, TimeSpan? after = null)
    {
        Environment.ExitCode = exitCode;
        Bot?.Services.GetRequiredService<BotActivityService>().MainLoopCts.CancelAfter(after ?? TimeSpan.Zero);
        return Task.CompletedTask;
    }


    private static void PrintBuildInformation()
    {
        var fileVersionInfo = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location);
        Console.WriteLine($@"{ApplicationName} {ApplicationVersion} ({fileVersionInfo.FileVersion})");
        Console.WriteLine();
    }

    private static async Task<BotConfigService> LoadBotConfigAsync()
    {
        Console.Write(@"Loading configuration... ");

        var cfg = new BotConfigService();
        await cfg.LoadConfigAsync();

        Console.Write("\r");
        return cfg;
    }

    private static async Task<DbContextBuilder> InitializeDatabaseAsync(BotConfigService cfg)
    {
        Log.Information("Establishing database connection");
        var dbb = new DbContextBuilder(cfg.CurrentConfiguration.DatabaseConfig);

        Log.Information("Testing database context creation");
        await using TheGodfatherDbContext db = dbb.CreateContext();
        IEnumerable<string> pendingMigrations = await db.Database.GetPendingMigrationsAsync();
        var migrations = pendingMigrations.ToList();
        if (!migrations.Any())
            return dbb;

        Log.Information("Applying pending database migrations: {PendingDbMigrations}", migrations);
        await db.Database.MigrateAsync();
        return dbb;
    }
}
