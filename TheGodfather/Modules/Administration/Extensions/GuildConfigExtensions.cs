using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;
using TheGodfather.Database.Models;
using TheGodfather.EventListeners.Common;
using TheGodfather.Modules.Administration.Common;
using TheGodfather.Services;

namespace TheGodfather.Modules.Administration.Extensions
{
    internal static class GuildConfigExtensions
    {
        public static DiscordEmbed ToDiscordEmbed(this GuildConfig gcfg, DiscordGuild guild, LocalizationService lcs, bool update = false)
        {
            var emb = new LocalizedEmbedBuilder(lcs, guild.Id);

            emb.WithLocalizedTitle(DiscordEventType.GuildUpdated, update ? "evt-cfg-update" : "str-guild-cfg");
            emb.WithThumbnail(guild.IconUrl);

            emb.AddLocalizedTitleField("str-prefix", gcfg.Prefix ?? lcs.GetString(guild.Id, "str-default"), inline: true);
            emb.AddLocalizedTitleField("str-silent", gcfg.ReactionResponse, inline: true);
            emb.AddLocalizedTitleField("str-cmd-suggestions", gcfg.SuggestionsEnabled, inline: true);

            if (gcfg.LoggingEnabled) {
                DiscordChannel? logchn = guild.GetChannel(gcfg.LogChannelId);
                if (logchn is null)
                    emb.AddLocalizedField("str-logging", "err-log-404", inline: true);
                else
                    emb.AddLocalizedField("str-logging", "fmt-logs", inline: true, contentArgs: new[] { logchn.Mention });
            } else {
                emb.AddLocalizedField("str-logging", "str-off", inline: true);
            }

            if (gcfg.WelcomeChannelId != 0) {
                DiscordChannel? wchn = guild.GetChannel(gcfg.WelcomeChannelId);
                if (wchn is null) {
                    emb.AddLocalizedField("str-memupd-w", "err-memupd-w-404", inline: true);
                } else {
                    emb.AddLocalizedField("str-memupd-w", "fmt-memupd", inline: true, contentArgs: new[] {
                        wchn.Mention,
                        Formatter.Strip(gcfg.WelcomeMessage ?? lcs.GetString(guild.Id, "str-default"))
                    });
                }
            } else {
                emb.AddLocalizedField("str-memupd-w", "str-off", inline: true);
            }

            if (gcfg.LeaveChannelId != 0) {
                    DiscordChannel? lchn = guild.GetChannel(gcfg.LeaveChannelId);
                if (lchn is null) {
                    emb.AddLocalizedField("str-memupd-l", "err-memupd-l-404", inline: true);
                } else {
                    emb.AddLocalizedField("str-memupd-l", "fmt-memupd", inline: true, contentArgs: new[] {
                        lchn.Mention,
                        Formatter.Strip(gcfg.LeaveMessage ?? lcs.GetString(guild.Id, "str-default"))
                    });
                }
            } else {
                emb.AddLocalizedField("str-memupd-l", "str-off", inline: true);
            }

            emb.AddLocalizedTitleField("str-ratelimit", gcfg.RatelimitSettings.ToEmbedFieldString(guild.Id, lcs), inline: true);
            emb.AddLocalizedTitleField("str-antispam", gcfg.AntispamSettings.ToEmbedFieldString(guild.Id, lcs), inline: true);
            emb.AddLocalizedTitleField("str-antiflood", gcfg.AntifloodSettings.ToEmbedFieldString(guild.Id, lcs), inline: true);
            emb.AddLocalizedTitleField("str-instantleave", gcfg.AntiInstantLeaveSettings.ToEmbedFieldString(guild.Id, lcs), inline: true);

            if (gcfg.MuteRoleId != 0) {
                DiscordRole? muteRole = guild.GetRole(gcfg.MuteRoleId);
                if (muteRole is null)
                    emb.AddLocalizedField("str-muterole", "err-muterole-404", inline: true);
                else
                    emb.AddLocalizedTitleField("str-muterole", muteRole.Name, inline: true);
            } else {
                emb.AddLocalizedField("str-muterole", "str-none", inline: true);
            }

            emb.AddLocalizedTitleField("str-lf", gcfg.LinkfilterSettings.ToEmbedFieldString(guild.Id, lcs), inline: true);

            return emb.Build();
        }
    }
}
