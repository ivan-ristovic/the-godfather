#region USING_DIRECTIVES
using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using TheGodfather.Common;
using TheGodfather.Common.Attributes;
using TheGodfather.EventListeners.Common;

using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Exceptions;
#endregion

namespace TheGodfather.EventListeners
{
    internal static class LinkfilterListeners
    {
        public static readonly Regex InviteRegex = new Regex(@"discord(?:\.gg|app\.com\/invite)\/([\w\-]+)", RegexOptions.Compiled | RegexOptions.IgnoreCase);


        [AsyncExecuter(EventTypes.MessageCreated)]
        public static async Task Client_MessageCreatedLinkfilter(TheGodfatherShard shard, MessageCreateEventArgs e)
        {
            if (e.Author.IsBot || e.Channel.IsPrivate || shard.SharedData.BlockedChannels.Contains(e.Channel.Id))
                return;

            if (e.Message?.Content == null)
                return;

            var gcfg = shard.SharedData.GetGuildConfig(e.Guild.Id);
            if (!gcfg.LinkfilterEnabled)
                return;
            
            if ((e.Channel.PermissionsFor(e.Author as DiscordMember).HasPermission(Permissions.ManageMessages)))
                return;

            if (gcfg.BlockDiscordInvites && await FoundAndDeletedInvitesAsync(shard, e).ConfigureAwait(false))
                return;

            if (gcfg.BlockBooterWebsites && await FoundAndDeletedBootersAsync(shard, e).ConfigureAwait(false))
                return;

            if (gcfg.BlockIpLoggingWebsites && await FoundAndDeletedIpLoggersAsync(shard, e).ConfigureAwait(false))
                return;

            if (gcfg.BlockDisturbingWebsites && await FoundAndDeletedDisturbingContentAsync(shard, e).ConfigureAwait(false))
                return;

            if (gcfg.BlockUrlShorteners && await FoundAndDeletedUrlShortenersAsync(shard, e).ConfigureAwait(false))
                return;
        }


        #region HELPER_FUNCTIONS
        private static async Task<bool> FoundAndDeletedInvitesAsync(TheGodfatherShard shard, MessageCreateEventArgs e)
        {
            if (!InviteRegex.IsMatch(e.Message.Content))
                return false;

            try {
                await e.Message.DeleteAsync("_gf: Invite linkfilter")
                    .ConfigureAwait(false);

                await LogLinkfilterMatchAsync(shard, e, "Discord invite matched")
                    .ConfigureAwait(false);

                await (e.Author as DiscordMember).SendMessageAsync(
                    $"Your message:\n{Formatter.BlockCode(e.Message.Content)}\nwas automatically removed from " +
                    $"{Formatter.Bold(e.Guild.Name)} because it contained a Discord invite."
                ).ConfigureAwait(false);

            } catch {

            }

            return true;
        }

        private static async Task<bool> FoundAndDeletedBootersAsync(TheGodfatherShard shard, MessageCreateEventArgs e)
        {
            var match = SuspiciousSites.BooterMatcher.Check(e.Message);
            if (!match.Success)
                return false;

            try {
                await e.Message.DeleteAsync("_gf: Booter linkfilter")
                    .ConfigureAwait(false);

                await LogLinkfilterMatchAsync(shard, e, "DDoS/Booter website matched")
                    .ConfigureAwait(false);

                await (e.Author as DiscordMember).SendMessageAsync(
                    $"Your message:\n{Formatter.BlockCode(e.Message.Content)}\nwas automatically removed from " +
                    $"{Formatter.Bold(e.Guild.Name)} because it contained a link to a DDoS/Booter website: {Formatter.InlineCode(match.Matched)}"
                ).ConfigureAwait(false);

            } catch {

            }
            return true;
        }

        private static async Task<bool> FoundAndDeletedIpLoggersAsync(TheGodfatherShard shard, MessageCreateEventArgs e)
        {
            var match = SuspiciousSites.IpLoggerMatcher.Check(e.Message);
            if (!match.Success)
                return false;

            try {
                await e.Message.DeleteAsync("_gf: IP logger linkfilter")
                    .ConfigureAwait(false);

                await LogLinkfilterMatchAsync(shard, e, "IP logging website matched")
                    .ConfigureAwait(false);

                await (e.Author as DiscordMember).SendMessageAsync(
                    $"Your message:\n{Formatter.BlockCode(e.Message.Content)}\nwas automatically removed from " +
                    $"{Formatter.Bold(e.Guild.Name)} because it contained a link to a IP logger website: {Formatter.InlineCode(match.Matched)}"
                ).ConfigureAwait(false);

            } catch {

            }

            return true;
        }

        private static async Task<bool> FoundAndDeletedDisturbingContentAsync(TheGodfatherShard shard, MessageCreateEventArgs e)
        {
            var match = SuspiciousSites.DisturbingWebsiteMatcher.Check(e.Message);
            if (!match.Success)
                return false;

            try {
                await e.Message.DeleteAsync("_gf: Disturbing content linkfilter")
                    .ConfigureAwait(false);

                await LogLinkfilterMatchAsync(shard, e, "Disturbing content website matched")
                    .ConfigureAwait(false);

                await (e.Author as DiscordMember).SendMessageAsync(
                    $"Your message:\n{Formatter.BlockCode(e.Message.Content)}\nwas automatically removed from " +
                    $"{Formatter.Bold(e.Guild.Name)} because it contained a link to a website marked as disturbing: {Formatter.InlineCode(match.Matched)}"
                ).ConfigureAwait(false);
            } catch {

            }
            return true;
        }

        private static async Task<bool> FoundAndDeletedUrlShortenersAsync(TheGodfatherShard shard, MessageCreateEventArgs e)
        {
            var match = UrlShortenerConstants.UrlShortenerRegex.Check(e.Message);
            if (!match.Success)
                return false;

            try {
                await e.Message.DeleteAsync("_gf: URL shortener linkfilter")
                    .ConfigureAwait(false);

                await LogLinkfilterMatchAsync(shard, e, "URL shortener website matched")
                    .ConfigureAwait(false);

                await (e.Author as DiscordMember).SendMessageAsync(
                    $"Your message:\n{Formatter.BlockCode(e.Message.Content)}\nwas automatically removed from " +
                    $"{Formatter.Bold(e.Guild.Name)} because it contained a link to a link shortener website: {Formatter.InlineCode(match.Matched)} (possible origin: {UrlShortenerConstants.UrlShorteners[match.Matched]})"
                ).ConfigureAwait(false);
            } catch (UnauthorizedException) {

            }
            return true;
        }
       
        private static async Task LogLinkfilterMatchAsync(TheGodfatherShard shard, MessageCreateEventArgs e, string desc)
        {
            var logchn = shard.SharedData.GetLogChannelForGuild(shard.Client, e.Guild);
            if (logchn != null) {
                var emb = new DiscordEmbedBuilder() {
                    Title = "Linkfilter action triggered",
                    Description = desc,
                    Color = DiscordColor.Red,
                };
                emb.AddField("User responsible", e.Author.Mention);

                await logchn.SendMessageAsync(embed: emb.Build())
                    .ConfigureAwait(false);
            }
        }
        #endregion
    }
}
