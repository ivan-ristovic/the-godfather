#region USING_DIRECTIVES
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Exceptions;
using System.Threading.Tasks;
using TheGodfather.EventListeners;
using TheGodfather.Modules.Administration.Common;
#endregion

namespace TheGodfather.Modules.Administration.Services
{
    public class LinkfilterService : ProtectionService
    {
        public LinkfilterService(TheGodfatherShard shard)
            : base(shard)
        {
            this.reason = "_gf: Linkfilter";
        }


        public override bool TryAddGuildToWatch(ulong gid) => true;
        public override bool TryRemoveGuildFromWatch(ulong gid) => true;


        public async Task<bool> HandleNewMessageAsync(MessageCreateEventArgs e, LinkfilterSettings settings)
        {
            if (e.Channel.PermissionsFor(e.Author as DiscordMember).HasPermission(Permissions.Administrator))
                return false;

            if (settings.BlockDiscordInvites && await this.ScanForDiscordInvitesAsync(e))
                return true;

            if (settings.BlockBooterWebsites && await this.ScanForBootersAsync(e))
                return true;

            if (settings.BlockIpLoggingWebsites && await this.ScanForIpLoggersAsync(e))
                return true;

            if (settings.BlockDisturbingWebsites && await this.ScanForDisturbingSitesAsync(e))
                return true;

            if (settings.BlockUrlShorteners && await this.ScanForUrlShortenersAsync(e))
                return true;

            return false;
        }


        private async Task<bool> ScanForDiscordInvitesAsync(MessageCreateEventArgs e)
        {
            if (!LinkfilterMatcherCollection.InviteRegex.IsMatch(e.Message.Content))
                return false;

            await this.DeleteAsync(e, "Discord invite");
            return true;
        }

        private async Task<bool> ScanForBootersAsync(MessageCreateEventArgs e)
        {
            var match = LinkfilterMatcherCollection.BooterMatcher.Check(e.Message);
            if (!match.Success)
                return false;

            await this.DeleteAsync(e, "DDoS/Booter website", Formatter.InlineCode(match.Matched));
            return true;
        }

        private async Task<bool> ScanForIpLoggersAsync(MessageCreateEventArgs e)
        {
            var match = LinkfilterMatcherCollection.IpLoggerMatcher.Check(e.Message);
            if (!match.Success)
                return false;

            await this.DeleteAsync(e, "IP logging website", Formatter.InlineCode(match.Matched));
            return true;
        }

        private async Task<bool> ScanForDisturbingSitesAsync(MessageCreateEventArgs e)
        {
            var match = LinkfilterMatcherCollection.DisturbingWebsiteMatcher.Check(e.Message);
            if (!match.Success)
                return false;

            await this.DeleteAsync(e, "Disturbing content website", Formatter.InlineCode(match.Matched));
            return true;
        }

        private async Task<bool> ScanForUrlShortenersAsync(MessageCreateEventArgs e)
        {
            var match = LinkfilterMatcherCollection.UrlShortenerRegex.Check(e.Message);
            if (!match.Success)
                return false;

            await this.DeleteAsync(e, "URL shortener website", $"{Formatter.InlineCode(match.Matched)} (possible origin: {LinkfilterMatcherCollection.UrlShorteners[match.Matched]}).\n\nIf you think this is a false detection, please report.");
            return true;
        }

        private async Task DeleteAsync(MessageCreateEventArgs e, string cause, string additionalText = null)
        {
            try {
                await e.Message.DeleteAsync($"_gf: {cause} linkfilter");
                await this.LogLinkfilterMatchAsync(e, $"{cause} matched");
                await (e.Author as DiscordMember).SendMessageAsync(
                    $"Your message:\n{Formatter.BlockCode(e.Message.Content)}was automatically removed from " +
                    $"{Formatter.Bold(e.Guild.Name)} because it contained a {cause}{(string.IsNullOrWhiteSpace(additionalText) ? "." : $": {additionalText}")}"
                );
            } catch {

            }
        }

        private async Task LogLinkfilterMatchAsync(MessageCreateEventArgs e, string desc)
        {
            DiscordChannel logchn = this.shard.SharedData.GetLogChannelForGuild(this.shard.Client, e.Guild);
            if (logchn is null)
                return;

            DiscordEmbedBuilder emb = Listeners.FormEmbedBuilder(EventOrigin.Linkfilter, "Linkfilter action triggered", desc);
            emb.AddField("User responsible", e.Author.Mention);

            await logchn.SendMessageAsync(embed: emb.Build());
        }
    }
}
