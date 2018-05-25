#region USING_DIRECTIVES
using System.Threading.Tasks;

using TheGodfather.Common;
using TheGodfather.Common.Attributes;
using TheGodfather.Extensions;

using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
#endregion

namespace TheGodfather.EventListeners
{
    internal static class ClientListeners
    {
        [AsyncExecuter(EventTypes.ClientErrored)]
        public static Task Client_Errored(TheGodfatherShard shard, ClientErrorEventArgs e)
        {
            shard.Log(LogLevel.Critical, $"Client errored: {e.Exception.GetType()}: {e.Exception.Message}");
            return Task.CompletedTask;
        }

        [AsyncExecuter(EventTypes.GuildAvailable)]
        public static Task Client_GuildAvailable(TheGodfatherShard shard, GuildCreateEventArgs e)
        {
            shard.Log(LogLevel.Info, $"Guild available: {e.Guild.ToString()}");
            return Task.CompletedTask;
        }

        [AsyncExecuter(EventTypes.GuildCreated)]
        public static async Task Client_GuildCreated(TheGodfatherShard shard, GuildCreateEventArgs e)
        {
            shard.Log(LogLevel.Info, $"Joined guild: {e.Guild.ToString()}");

            await shard.Database.RegisterGuildAsync(e.Guild.Id)
                .ConfigureAwait(false);
            shard.Shared.GuildConfigurations.TryAdd(e.Guild.Id, PartialGuildConfig.Default);

            var emoji = DiscordEmoji.FromName(e.Client, ":small_blue_diamond:");
            await e.Guild.GetDefaultChannel().SendIconEmbedAsync(
                $"{Formatter.Bold("Thank you for adding me!")}\n\n" +
                $"{emoji} The default prefix for commands is {Formatter.Bold(shard.Shared.BotConfiguration.DefaultPrefix)}, but it can be changed using {Formatter.Bold("prefix")} command.\n" +
                $"{emoji} I advise you to run the configuration wizard for this guild in order to quickly configure functions like logging, notifications etc. The wizard can be invoked using {Formatter.Bold("guild config setup")} command.\n" +
                $"{emoji} You can use the {Formatter.Bold("help")} command as a guide, though it is recommended to read the documentation @ https://github.com/ivan-ristovic/the-godfather\n" +
                $"{emoji} If you have any questions or problems, feel free to use the {Formatter.Bold("report")} command in order send a message to the bot owner ({e.Client.CurrentApplication.Owner.Username}#{e.Client.CurrentApplication.Owner.Discriminator}). Alternatively, you can create an issue on GitHub or join WorldMafia discord server for quick support (https://discord.me/worldmafia).\n"
                , StaticDiscordEmoji.Wave
            ).ConfigureAwait(false);
        }
    }
}
