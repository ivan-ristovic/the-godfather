using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using TheGodfather.Database;
using TheGodfather.Modules.Administration.Common;
using TheGodfather.Services;
using TheGodfather.Services.Common;

namespace TheGodfather.Modules.Administration.Services
{
    public sealed class LinkfilterService : ProtectionServiceBase
    {
        public LinkfilterService(DbContextBuilder dbb, LoggingService ls, SchedulingService ss, GuildConfigService gcs)
            : base(dbb, ls, ss, gcs, "_gf: Linkfilter") { }


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


        private Task<bool> ScanForDiscordInvitesAsync(MessageCreateEventArgs e)
        {
            return LinkfilterMatcherCollection.InviteRegex.IsMatch(e.Message.Content)
                ? this.DeleteAsync(e, "Discord invite", null)
                : Task.FromResult(false);
        }

        private Task<bool> ScanForBootersAsync(MessageCreateEventArgs e)
        {
            LinkfilterMatch match = LinkfilterMatcherCollection.BooterMatcher.Match(e.Message);
            return match.Success
                ? this.DeleteAsync(e, "DDoS/Booter website", match)
                : Task.FromResult(false);
        }

        private Task<bool> ScanForIpLoggersAsync(MessageCreateEventArgs e)
        {
            LinkfilterMatch match = LinkfilterMatcherCollection.IpLoggerMatcher.Match(e.Message);
            return match.Success
                ? this.DeleteAsync(e, "IP logging website", match)
                : Task.FromResult(false);
        }

        private Task<bool> ScanForDisturbingSitesAsync(MessageCreateEventArgs e)
        {
            LinkfilterMatch match = LinkfilterMatcherCollection.DisturbingWebsiteMatcher.Match(e.Message);
            return match.Success
                ? this.DeleteAsync(e, "Disturbing content website", match)
                : Task.FromResult(false);
        }

        private Task<bool> ScanForUrlShortenersAsync(MessageCreateEventArgs e)
        {
            LinkfilterMatch match = LinkfilterMatcherCollection.UrlShortenerRegex.Match(e.Message);
            return match.Success
                ? this.DeleteAsync(e, "URL shortener website", match)
                : Task.FromResult(false);
        }

        private async Task<bool> DeleteAsync(MessageCreateEventArgs e, string cause, LinkfilterMatch? match)
        {
            try {
                await e.Message.DeleteAsync($"_gf: {cause} linkfilter");
                await this.LogLinkfilterMatchAsync(e, cause, match);
                if (e.Author is not DiscordMember member)
                    return true;
                await member.SendMessageAsync($"Your message:{Formatter.BlockCode(e.Message.Content)}was automatically filtered from {Formatter.Bold(e.Guild.Name)}.");
            } catch {

            }
            return true;
        }

        private async Task LogLinkfilterMatchAsync(MessageCreateEventArgs e, string desc, LinkfilterMatch? match)
        {
            if (!this.ls.IsLogEnabledFor(e.Guild.Id, out LocalizedEmbedBuilder emb))
                return;

            emb.WithLocalizedTitle("evt-lf-triggered");
            emb.WithDescription(desc);
            if (match is { } && match.Matched is { })
                emb.AddLocalizedTitleField("str-matched", match.Matched);
            emb.WithColor(DiscordColor.Red);
            emb.AddInvocationFields(e.Author, e.Channel);
            await this.ls.LogAsync(e.Guild, emb);
        }

        public override void Dispose()
        {

        }
    }
}
