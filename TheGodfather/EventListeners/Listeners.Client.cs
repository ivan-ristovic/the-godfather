using System;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Humanizer;
using Microsoft.Extensions.DependencyInjection;
using TheGodfather.Common;
using TheGodfather.EventListeners.Attributes;
using TheGodfather.EventListeners.Common;
using TheGodfather.Extensions;
using TheGodfather.Modules.Administration.Services;
using TheGodfather.Services;

namespace TheGodfather.EventListeners
{
    internal static partial class Listeners
    {
        [AsyncEventListener(DiscordEventType.ClientErrored)]
        public static Task ClientErrorEventHandlerAsync(TheGodfatherShard shard, ClientErrorEventArgs e)
        {
            Exception ex = e.Exception;
            while (ex is AggregateException)
                ex = ex.InnerException ?? ex;

            LogExt.Error(shard.Id, ex, "Client errored!");
            return Task.CompletedTask;
        }

        [AsyncEventListener(DiscordEventType.GuildAvailable)]
        public static Task GuildAvailableEventHandlerAsync(TheGodfatherShard shard, GuildCreateEventArgs e)
        {
            LogExt.Information(shard.Id, "Available {AvailableGuild}", e.Guild);
            GuildConfigService gcs = shard.Services.GetRequiredService<GuildConfigService>();
            return gcs.IsGuildRegistered(e.Guild.Id) ? Task.CompletedTask : gcs.RegisterGuildAsync(e.Guild.Id);
        }

        [AsyncEventListener(DiscordEventType.GuildUnavailable)]
        public static Task GuildUnvailableEventHandlerAsync(TheGodfatherShard shard, GuildDeleteEventArgs e)
        {
            LogExt.Warning(shard.Id, "Unvailable {UnvailableGuild}", e.Guild);
            return Task.CompletedTask;
        }

        [AsyncEventListener(DiscordEventType.GuildDownloadCompleted)]
        public static Task GuildDownloadCompletedEventHandlerAsync(TheGodfatherShard shard, GuildDownloadCompletedEventArgs _)
        {
            LogExt.Information(shard.Id, "All guilds for this shard are now downloaded");
            return Task.CompletedTask;
        }

        [AsyncEventListener(DiscordEventType.GuildCreated)]
        public static async Task GuildCreateEventHandlerAsync(TheGodfatherShard shard, GuildCreateEventArgs e)
        {
            LogExt.Information(shard.Id, "Joined {NewGuild}", e.Guild);

            await shard.Services.GetRequiredService<GuildConfigService>().RegisterGuildAsync(e.Guild.Id);

            DiscordChannel defChannel = e.Guild.GetDefaultChannel();
            if (!defChannel.PermissionsFor(e.Guild.CurrentMember).HasPermission(Permissions.SendMessages))
                return;

            string prefix = shard.Services.GetService<BotConfigService>().CurrentConfiguration.Prefix;
            string owners = e.Client.CurrentApplication.Owners.Select(o => o.ToDiscriminatorString()).Humanize(", ");
            await defChannel.EmbedAsync(
                $"{Formatter.Bold("Thank you for adding me!")}\n\n" +
                $"{Emojis.SmallBlueDiamond} The default prefix for commands is {Formatter.Bold(prefix)}, but it can be changed " +
                $"via {Formatter.Bold("prefix")} command.\n" +
                $"{Emojis.SmallBlueDiamond} I advise you to run the configuration wizard for this guild in order to quickly configure " +
                $"functions like logging, notifications etc. The wizard can be invoked using {Formatter.Bold("guild config setup")} command.\n" +
                $"{Emojis.SmallBlueDiamond} You can use the {Formatter.Bold("help")} command as a guide, though it is recommended to " +
                $"read the documentation @ https://github.com/ivan-ristovic/the-godfather \n" +
                $"{Emojis.SmallBlueDiamond} If you have any questions or problems, feel free to use the {Formatter.Bold("report")} " +
                $"command in order to send a message to the bot owners ({owners}). Alternatively, you can create an issue on " +
                $"GitHub or join WorldMafia Discord server for quick support (https://worldmafia.net/discord)."
                , Emojis.Wave
            );
        }
        
        [AsyncEventListener(DiscordEventType.SocketOpened)]
        public static Task SocketOpenedEventHandlerAsync(TheGodfatherShard shard)
        {
            LogExt.Debug(shard.Id, "Socket opened");
            shard.Services.GetRequiredService<BotActivityService>().ShardUptimeInformation[shard.Id].SocketStartTime = DateTimeOffset.Now;
            return Task.CompletedTask;
        }
    }
}
