using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Microsoft.Extensions.DependencyInjection;
using TheGodfather.Modules.Administration.Common;
using TheGodfather.Modules.Administration.Extensions;

namespace TheGodfather.Modules.Administration.Services
{
    public sealed class LinkfilterService : ProtectionService
    {
        public LinkfilterService(TheGodfatherShard shard)
            : base(shard, "_gf: Linkfilter") { }


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
                ? this.DeleteAsync(e, "Discord invite")
                : Task.FromResult(false);
        }

        private Task<bool> ScanForBootersAsync(MessageCreateEventArgs e)
        {
            LinkfilterMatch match = LinkfilterMatcherCollection.BooterMatcher.Match(e.Message);
            return match.Success 
                ? this.DeleteAsync(e, "DDoS/Booter website", Formatter.InlineCode(match.Matched)) 
                : Task.FromResult(false);
        }

        private Task<bool> ScanForIpLoggersAsync(MessageCreateEventArgs e)
        {
            LinkfilterMatch match = LinkfilterMatcherCollection.IpLoggerMatcher.Match(e.Message);
            return match.Success 
                ? this.DeleteAsync(e, "IP logging website", Formatter.InlineCode(match.Matched)) 
                : Task.FromResult(false);
        }

        private Task<bool> ScanForDisturbingSitesAsync(MessageCreateEventArgs e)
        {
            LinkfilterMatch match = LinkfilterMatcherCollection.DisturbingWebsiteMatcher.Match(e.Message);
            return match.Success
                ? this.DeleteAsync(e, "Disturbing content website", Formatter.InlineCode(match.Matched))
                : Task.FromResult(false);
        }

        private Task<bool> ScanForUrlShortenersAsync(MessageCreateEventArgs e)
        {
            LinkfilterMatch match = LinkfilterMatcherCollection.UrlShortenerRegex.Match(e.Message);
            return match.Success
                ? this.DeleteAsync(e,
                   "URL shortener website",
                    $"{Formatter.InlineCode(match.Matched)} (possible origin: {LinkfilterMatcherCollection.UrlShorteners[match.Matched]}).\n\n" +
                    $"If you think this is a false detection, please report."
                )
                : Task.FromResult(false);
        }



        // TODO


        private async Task<bool> DeleteAsync(MessageCreateEventArgs e, string cause, string? additionalText = null)
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
            return true;
        }

        private async Task LogLinkfilterMatchAsync(MessageCreateEventArgs e, string desc)
        {
            if (this.shard.Services.GetRequiredService<GuildConfigService>().GetLogChannelForGuild(e.Guild) is null)
                return;

            // TODO
            //var emb = new DiscordLogEmbedBuilder("Linkfilter action triggered", desc, DiscordEventType.MessageDeleted);
            //emb.AddInvocationFields(e.Author);
            //await this.shard.Services.GetRequiredService<LoggingService>().LogAsync(e.Guild, emb);
        }

        public override void Dispose() 
        { 
        
        }
    }
}
