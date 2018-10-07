#region USING_DIRECTIVES
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;

using System;
using System.Linq;
using System.Threading.Tasks;

using TheGodfather.Common;
using TheGodfather.Common.Attributes;
using TheGodfather.Database;
using TheGodfather.Database.Entities;
using TheGodfather.Extensions;
#endregion

namespace TheGodfather.EventListeners
{
    internal static partial class Listeners
    {
        [AsyncEventListener(DiscordEventType.ClientErrored)]
        public static Task ClientErrorEventHandlerAsync(TheGodfatherShard shard, ClientErrorEventArgs e)
        {
            var ex = e.Exception;
            while (ex is AggregateException)
                ex = ex.InnerException;

            shard.Log(
                LogLevel.Critical,
                $"| Client errored with exception: {ex.GetType()}\n" +
                $"| Message: {ex.Message}" +
                (ex.InnerException is null ? "" : $"| Inner exception: {ex.InnerException.GetType()}\n| Inner exception message: {ex.InnerException.Message}")
            );

            return Task.CompletedTask;
        }

        [AsyncEventListener(DiscordEventType.GuildAvailable)]
        public static Task GuildAvailableEventHandlerAsync(TheGodfatherShard shard, GuildCreateEventArgs e)
        {
            shard.Log(LogLevel.Info, $"| Guild available: {e.Guild.ToString()}");

            if (shard.SharedData.GuildConfigurations.ContainsKey(e.Guild.Id))
                return Task.CompletedTask;

            return RegisterGuildAsync(shard.SharedData, shard.Database, e.Guild.Id);
        }

        [AsyncEventListener(DiscordEventType.GuildCreated)]
        public static async Task GuildCreateEventHandlerAsync(TheGodfatherShard shard, GuildCreateEventArgs e)
        {
            shard.Log(LogLevel.Info, $"| Joined guild: {e.Guild.ToString()}");

            await RegisterGuildAsync(shard.SharedData, shard.Database, e.Guild.Id);

            DiscordChannel defChannel = e.Guild.GetDefaultChannel();

            if (!defChannel.PermissionsFor(e.Guild.CurrentMember).HasPermission(Permissions.SendMessages))
                return;

            await defChannel.EmbedAsync(
                $"{Formatter.Bold("Thank you for adding me!")}\n\n" +
                $"{StaticDiscordEmoji.SmallBlueDiamond} The default prefix for commands is {Formatter.Bold(shard.SharedData.BotConfiguration.DefaultPrefix)}, but it can be changed using {Formatter.Bold("prefix")} command.\n" +
                $"{StaticDiscordEmoji.SmallBlueDiamond} I advise you to run the configuration wizard for this guild in order to quickly configure functions like logging, notifications etc. The wizard can be invoked using {Formatter.Bold("guild config setup")} command.\n" +
                $"{StaticDiscordEmoji.SmallBlueDiamond} You can use the {Formatter.Bold("help")} command as a guide, though it is recommended to read the documentation @ https://github.com/ivan-ristovic/the-godfather\n" +
                $"{StaticDiscordEmoji.SmallBlueDiamond} If you have any questions or problems, feel free to use the {Formatter.Bold("report")} command in order send a message to the bot owner ({e.Client.CurrentApplication.Owner.Username}#{e.Client.CurrentApplication.Owner.Discriminator}). Alternatively, you can create an issue on GitHub or join WorldMafia discord server for quick support (https://discord.me/worldmafia)."
                , StaticDiscordEmoji.Wave
            );
        }

        [AsyncEventListener(DiscordEventType.SocketOpened)]
        public static async Task SocketOpeneEventHandlerAsync(TheGodfatherShard shard)
        {
            await Task.Yield();
            shard.SharedData.UptimeInformation.SocketStartTime = DateTimeOffset.UtcNow;
        }


        private static async Task RegisterGuildAsync(SharedData shared, DatabaseContextBuilder dbb, ulong gid)
        {
            shared.GuildConfigurations.TryAdd(gid, CachedGuildConfig.Default);
            using (DatabaseContext db = dbb.CreateContext()) {
                var gcfg = new DatabaseGuildConfig() { GuildIdDb = (long)gid };
                if (!db.GuildConfig.Contains(gcfg)) {
                    db.GuildConfig.Add(gcfg);
                    await db.SaveChangesAsync();
                }
            }
        }
    }
}
