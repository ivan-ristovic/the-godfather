#region USING_DIRECTIVES
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Exceptions;
using System.Threading.Tasks;
using TheGodfather.Common;
using TheGodfather.Common.Attributes;
using TheGodfather.EventListeners.Common;
#endregion

namespace TheGodfather.EventListeners
{
    internal static partial class Listeners
    {
        [AsyncEventListener(DiscordEventType.MessageCreated)]
        public static async Task Client_MessageCreatedLinkfilter(TheGodfatherShard shard, MessageCreateEventArgs e)
        {
            if (e.Author.IsBot || e.Channel.IsPrivate || shard.SharedData.BlockedChannels.Contains(e.Channel.Id))
                return;

            if (e.Message?.Content == null)
                return;

            CachedGuildConfig gcfg = shard.SharedData.GetGuildConfig(e.Guild.Id);
            if (!gcfg.LinkfilterEnabled)
                return;

            if ((e.Channel.PermissionsFor(e.Author as DiscordMember).HasPermission(Permissions.ManageMessages)))
                return;

            if (gcfg.BlockDiscordInvites && await ScanForInvitesInvitesAsync(shard, e))
                return;

            if (gcfg.BlockBooterWebsites && await ScanForBootersAsync(shard, e))
                return;

            if (gcfg.BlockIpLoggingWebsites && await ScanForIpLoggersAsync(shard, e))
                return;

            if (gcfg.BlockDisturbingWebsites && await ScanForDisturbingSitesAsync(shard, e))
                return;

            if (gcfg.BlockUrlShorteners && await ScanForUrlShortenersAsync(shard, e))
                return;
        }


        #region HELPER_FUNCTIONS
        private static async Task<bool> ScanForInvitesInvitesAsync(TheGodfatherShard shard, MessageCreateEventArgs e)
        {
            if (!LinkfilterMatcherCollection.InviteRegex.IsMatch(e.Message.Content))
                return false;

            try {
                await e.Message.DeleteAsync("_gf: Invite linkfilter");
                await LogLinkfilterMatchAsync(shard, e, "Discord invite matched");
                await (e.Author as DiscordMember).SendMessageAsync(
                    $"Your message:\n{Formatter.BlockCode(e.Message.Content)}\nwas automatically removed from " +
                    $"{Formatter.Bold(e.Guild.Name)} because it contained a Discord invite."
                );
            } catch {

            }

            return true;
        }

        private static async Task<bool> ScanForBootersAsync(TheGodfatherShard shard, MessageCreateEventArgs e)
        {
            var match = LinkfilterMatcherCollection.BooterMatcher.Check(e.Message);
            if (!match.Success)
                return false;

            try {
                await e.Message.DeleteAsync("_gf: Booter linkfilter");
                await LogLinkfilterMatchAsync(shard, e, "DDoS/Booter website matched");
                await (e.Author as DiscordMember).SendMessageAsync(
                    $"Your message:\n{Formatter.BlockCode(e.Message.Content)}\nwas automatically removed from " +
                    $"{Formatter.Bold(e.Guild.Name)} because it contained a link to a DDoS/Booter website: {Formatter.InlineCode(match.Matched)}"
                );
            } catch {

            }
            return true;
        }

        private static async Task<bool> ScanForIpLoggersAsync(TheGodfatherShard shard, MessageCreateEventArgs e)
        {
            var match = LinkfilterMatcherCollection.IpLoggerMatcher.Check(e.Message);
            if (!match.Success)
                return false;

            try {
                await e.Message.DeleteAsync("_gf: IP logger linkfilter");
                await LogLinkfilterMatchAsync(shard, e, "IP logging website matched");
                await (e.Author as DiscordMember).SendMessageAsync(
                    $"Your message:\n{Formatter.BlockCode(e.Message.Content)}\nwas automatically removed from " +
                    $"{Formatter.Bold(e.Guild.Name)} because it contained a link to a IP logger website: {Formatter.InlineCode(match.Matched)}"
                );
            } catch {

            }

            return true;
        }

        private static async Task<bool> ScanForDisturbingSitesAsync(TheGodfatherShard shard, MessageCreateEventArgs e)
        {
            var match = LinkfilterMatcherCollection.DisturbingWebsiteMatcher.Check(e.Message);
            if (!match.Success)
                return false;

            try {
                await e.Message.DeleteAsync("_gf: Disturbing content linkfilter");
                await LogLinkfilterMatchAsync(shard, e, "Disturbing content website matched");
                await (e.Author as DiscordMember).SendMessageAsync(
                    $"Your message:\n{Formatter.BlockCode(e.Message.Content)}\nwas automatically removed from " +
                    $"{Formatter.Bold(e.Guild.Name)} because it contained a link to a website marked as disturbing: {Formatter.InlineCode(match.Matched)}"
                );
            } catch {

            }
            return true;
        }

        private static async Task<bool> ScanForUrlShortenersAsync(TheGodfatherShard shard, MessageCreateEventArgs e)
        {
            var match = LinkfilterMatcherCollection.UrlShortenerRegex.Check(e.Message);
            if (!match.Success)
                return false;

            try {
                await e.Message.DeleteAsync("_gf: URL shortener linkfilter");
                await LogLinkfilterMatchAsync(shard, e, "URL shortener website matched");
                await (e.Author as DiscordMember).SendMessageAsync(
                    $"Your message:\n{Formatter.BlockCode(e.Message.Content)}\nwas automatically removed from " +
                    $"{Formatter.Bold(e.Guild.Name)} because it contained a link to a link shortener website: {Formatter.InlineCode(match.Matched)} (possible origin: {LinkfilterMatcherCollection.UrlShorteners[match.Matched]})"
                );
            } catch (UnauthorizedException) {

            }

            return true;
        }

        private static async Task LogLinkfilterMatchAsync(TheGodfatherShard shard, MessageCreateEventArgs e, string desc)
        {
            DiscordChannel logchn = shard.SharedData.GetLogChannelForGuild(shard.Client, e.Guild);
            if (logchn == null)
                return;

            DiscordEmbedBuilder emb = FormEmbedBuilder(EventOrigin.Linkfilter, "Linkfilter action triggered", desc);
            emb.AddField("User responsible", e.Author.Mention);

            await logchn.SendMessageAsync(embed: emb.Build());
        }
        #endregion
    }
}
