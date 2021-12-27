using DSharpPlus;
using DSharpPlus.Entities;
using TheGodfather.Database.Models;
using TheGodfather.EventListeners.Common;
using TheGodfather.Services;
using TheGodfather.Services.Common;
using TheGodfather.Translations;

namespace TheGodfather.Modules.Administration.Extensions
{
    internal static class GuildConfigExtensions
    {
        public static DiscordEmbed ToDiscordEmbed(this GuildConfig gcfg, DiscordGuild guild, LocalizationService lcs, bool update = false)
        {
            var emb = new LocalizedEmbedBuilder(lcs, guild.Id);

            emb.WithLocalizedTitle(DiscordEventType.GuildUpdated, update ? TranslationKey.evt_cfg_upd : TranslationKey.str_guild_cfg);
            emb.WithThumbnail(guild.IconUrl);

            emb.AddLocalizedField(TranslationKey.str_prefix, gcfg.Prefix ?? lcs.GetString(guild.Id, TranslationKey.str_default), inline: true);
            emb.AddLocalizedField(TranslationKey.str_replies_s, gcfg.ReactionResponse, inline: true);
            emb.AddLocalizedField(TranslationKey.str_cmd_suggestions, gcfg.SuggestionsEnabled, inline: true);
            emb.AddLocalizedField(TranslationKey.str_actionhistory, gcfg.ActionHistoryEnabled, inline: true);
            emb.AddLocalizedField(TranslationKey.str_lvlup_s, gcfg.SilentLevelUpEnabled, inline: true);

            if (gcfg.LoggingEnabled) {
                DiscordChannel? logchn = guild.GetChannel(gcfg.LogChannelId);
                if (logchn is null)
                    emb.AddLocalizedField(TranslationKey.str_logging, TranslationKey.err_log_404, inline: true);
                else
                    emb.AddLocalizedField(TranslationKey.str_logging, TranslationKey.fmt_logs(logchn.Mention), inline: true);
            } else {
                emb.AddLocalizedField(TranslationKey.str_logging, TranslationKey.str_off, inline: true);
            }

            emb.AddLocalizedField(TranslationKey.str_backup, gcfg.BackupEnabled ? TranslationKey.str_enabled : TranslationKey.str_disabled, inline: true);

            if (gcfg.WelcomeChannelId != default) {
                DiscordChannel? wchn = guild.GetChannel(gcfg.WelcomeChannelId);
                if (wchn is null) {
                    emb.AddLocalizedField(TranslationKey.str_memupd_w, TranslationKey.err_memupd_w_404, inline: true);
                } else {
                    string msg = Formatter.Strip(gcfg.WelcomeMessage ?? lcs.GetString(guild.Id, TranslationKey.str_default));
                    emb.AddLocalizedField(TranslationKey.str_memupd_w, TranslationKey.fmt_memupd(wchn.Mention, msg), inline: true);
                }
            } else {
                emb.AddLocalizedField(TranslationKey.str_memupd_w, TranslationKey.str_off, inline: true);
            }

            if (gcfg.LeaveChannelId != default) {
                DiscordChannel? lchn = guild.GetChannel(gcfg.LeaveChannelId);
                if (lchn is null) {
                    emb.AddLocalizedField(TranslationKey.str_memupd_l, TranslationKey.err_memupd_l_404, inline: true);
                } else {
                    string msg = Formatter.Strip(gcfg.LeaveMessage ?? lcs.GetString(guild.Id, TranslationKey.str_default));
                    emb.AddLocalizedField(TranslationKey.str_memupd_l, TranslationKey.fmt_memupd(lchn.Mention, msg), inline: true);
                }
            } else {
                emb.AddLocalizedField(TranslationKey.str_memupd_l, TranslationKey.str_off, inline: true);
            }

            emb.AddLocalizedField(TranslationKey.str_ratelimit, gcfg.RatelimitSettings.ToEmbedFieldString(), inline: true);
            emb.AddLocalizedField(TranslationKey.str_antispam, gcfg.AntispamSettings.ToEmbedFieldString(), inline: true);
            emb.AddLocalizedField(TranslationKey.str_antiflood, gcfg.AntifloodSettings.ToEmbedFieldString(), inline: true);
            emb.AddLocalizedField(TranslationKey.str_instantleave, gcfg.AntiInstantLeaveSettings.ToEmbedFieldString(), inline: true);

            if (gcfg.MuteRoleId != default) {
                DiscordRole? muteRole = guild.GetRole(gcfg.MuteRoleId);
                if (muteRole is null)
                    emb.AddLocalizedField(TranslationKey.str_muterole, TranslationKey.err_muterole_404, inline: true);
                else
                    emb.AddLocalizedField(TranslationKey.str_muterole, muteRole.Name, inline: true);
            } else {
                emb.AddLocalizedField(TranslationKey.str_muterole, TranslationKey.str_none, inline: true);
            }

            emb.AddLocalizedField(TranslationKey.str_lf, gcfg.LinkfilterSettings.ToEmbedFieldString(), inline: true);

            return emb.Build();
        }
    }
}
