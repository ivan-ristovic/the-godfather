#region USING_DIRECTIVES
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;

using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Context;
using Serilog.Events;
using System;
using System.Linq;
using System.Threading.Tasks;

using TheGodfather.Common;
using TheGodfather.Common.Attributes;
using TheGodfather.Database;
using TheGodfather.Database.Entities;
using TheGodfather.Extensions;
using TheGodfather.Modules.Administration.Services;
using TheGodfather.Services;
#endregion

namespace TheGodfather.EventListeners
{
    internal static partial class Listeners
    {
        [AsyncEventListener(DiscordEventType.ClientErrored)]
        public static Task ClientErrorEventHandlerAsync(TheGodfatherShard shard, ClientErrorEventArgs e)
        {
            Exception ex = e.Exception;
            while (ex is AggregateException)
                ex = ex.InnerException;

            LogExt.Fatal(shard.Id, ex, "Client errored!");
            return Task.CompletedTask;
        }

        [AsyncEventListener(DiscordEventType.GuildAvailable)]
        public static Task GuildAvailableEventHandlerAsync(TheGodfatherShard shard, GuildCreateEventArgs e)
        {
            LogExt.Information(shard.Id, "Available {AvailableGuild}", e.Guild);
            GuildConfigService gcs = shard.Services.GetService<GuildConfigService>();
            return gcs.IsGuildRegistered(e.Guild.Id) ? Task.CompletedTask : gcs.RegisterGuildAsync(e.Guild.Id);
        }

        [AsyncEventListener(DiscordEventType.GuildDownloadCompleted)]
        public static Task GuildDownloadCompletedEventHandlerAsync(TheGodfatherShard shard, GuildDownloadCompletedEventArgs _)
        {
            LogExt.Information(shard.Id, "All guilds are now available");
            return Task.CompletedTask;
        }

        [AsyncEventListener(DiscordEventType.GuildCreated)]
        public static async Task GuildCreateEventHandlerAsync(TheGodfatherShard shard, GuildCreateEventArgs e)
        {
            LogExt.Information(shard.Id, "Joined {NewGuild}", e.Guild);

            await shard.Services.GetService<GuildConfigService>().RegisterGuildAsync(e.Guild.Id);

            DiscordChannel defChannel = e.Guild.GetDefaultChannel();
            if (!defChannel.PermissionsFor(e.Guild.CurrentMember).HasPermission(Permissions.SendMessages))
                return;

            await defChannel.EmbedAsync(
                $"{Formatter.Bold("Thank you for adding me!")}\n\n" +
                $"{StaticDiscordEmoji.SmallBlueDiamond} The default prefix for commands is {Formatter.Bold(shard.Services.GetService<BotConfigService>().CurrentConfiguration.Prefix)}, but it can be changed using {Formatter.Bold("prefix")} command.\n" +
                $"{StaticDiscordEmoji.SmallBlueDiamond} I advise you to run the configuration wizard for this guild in order to quickly configure functions like logging, notifications etc. The wizard can be invoked using {Formatter.Bold("guild config setup")} command.\n" +
                $"{StaticDiscordEmoji.SmallBlueDiamond} You can use the {Formatter.Bold("help")} command as a guide, though it is recommended to read the documentation @ https://github.com/ivan-ristovic/the-godfather \n" +
                $"{StaticDiscordEmoji.SmallBlueDiamond} If you have any questions or problems, feel free to use the {Formatter.Bold("report")} command in order to send a message to the bot owner ({string.Join(",", e.Client.CurrentApplication.Owners.Select(o => $"{o.Username}#{o.Discriminator}"))}). Alternatively, you can create an issue on GitHub or join WorldMafia Discord server for quick support (https://discord.me/worldmafia)."
                , StaticDiscordEmoji.Wave
            );
        }
    }
}
