using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using TheGodfather.Attributes;
using TheGodfather.Common;
using TheGodfather.Database.Models;
using TheGodfather.Exceptions;
using TheGodfather.Extensions;
using TheGodfather.Modules.Administration.Extensions;
using TheGodfather.Modules.Misc.Services;

namespace TheGodfather.Modules.Misc
{
    [Group("starboard"), Module(ModuleType.Misc), NotBlocked]
    [Aliases("star", "sb")]
    [RequireGuild, RequireUserPermissions(Permissions.ManageGuild)]
    [Cooldown(3, 5, CooldownBucketType.Guild)]
    public sealed class StarboardModule : TheGodfatherServiceModule<StarboardService>
    {
        #region starboard
        [GroupCommand, Priority(2)]
        public Task ExecuteGroupAsync(CommandContext ctx,
                                     [Description(TranslationKey.desc_enable)] bool enable,
                                     [Description(TranslationKey.desc_emoji)] DiscordEmoji emoji,
                                     [Description(TranslationKey.desc_chn)] DiscordChannel channel,
                                     [Description(TranslationKey.desc_sens)] int? sens = null)
            => this.InternalStarboardAsync(ctx, enable, channel, emoji, sens);

        [GroupCommand, Priority(1)]
        public Task ExecuteGroupAsync(CommandContext ctx,
                                     [Description(TranslationKey.desc_enable)] bool enable,
                                     [Description(TranslationKey.desc_emoji)] DiscordChannel channel,
                                     [Description(TranslationKey.desc_chn)] DiscordEmoji? emoji = null,
                                     [Description(TranslationKey.desc_sens)] int? sens = null)
            => this.InternalStarboardAsync(ctx, enable, channel, emoji, sens);

        [GroupCommand, Priority(0)]
        public async Task ExecuteGroupAsync(CommandContext ctx)
        {
            DiscordChannel? starChn = null;
            DiscordEmoji? starEmoji = null;
            int? totalMsgs = null;
            int? totalStars = null;
            if (this.Service.IsStarboardEnabled(ctx.Guild.Id, out ulong cid, out string emoji)) {
                starChn = ctx.Guild.GetChannel(cid);
                try {
                    starEmoji = DiscordEmoji.FromName(ctx.Client, emoji);
                } catch {

                }

                IReadOnlyList<StarboardMessage> all = await this.Service.GetAllAsync(ctx.Guild.Id);
                totalMsgs = all.Count;
                totalStars = all.Sum(sm => sm.Stars);
            }

            await ctx.RespondWithLocalizedEmbedAsync(emb => {
                emb.WithColor(this.ModuleColor);
                emb.WithLocalizedTitle(TranslationKey.str_starboard);
                emb.AddLocalizedField(TranslationKey.str_status, cid == 0 ? TranslationKey.str_disabled : TranslationKey.str_enabled, inline: true);
                emb.AddLocalizedField(TranslationKey.str_chn, starChn?.Mention, inline: true, unknown: false);
                emb.AddLocalizedField(TranslationKey.str_star, starEmoji, inline: true, unknown: false);
                emb.AddLocalizedField(TranslationKey.str_sensitivity, this.Service.GetStarboardSensitivity(ctx.Guild.Id), inline: true);
                emb.AddLocalizedField(TranslationKey.str_starmsgs, totalMsgs, inline: true, unknown: false);
                emb.AddLocalizedField(TranslationKey.str_stars_total, totalStars, inline: true, unknown: false);
            });
        }
        #endregion

        #region starboard channel
        [Command("channel")]
        [Aliases("chn", "setchannel", "setchn", "setc", "location")]
        public async Task ChannelAsync(CommandContext ctx,
                                      [Description(TranslationKey.desc_chn)] DiscordChannel? channel = null)
        {
            if (channel is null) {
                ulong cid = this.Service.GetStarboardChannel(ctx.Guild.Id);
                channel = ctx.Guild.GetChannel(cid);
                if (channel is null)
                    throw new CommandFailedException(ctx, TranslationKey.cmd_err_sb_chn(cid));
                await ctx.ImpInfoAsync(this.ModuleColor, Emojis.Star, TranslationKey.evt_sb_chn(channel.Mention));
                return;
            }

            this.PerformChannelChecks(ctx, channel);

            await this.Service.SetStarboardChannelAsync(ctx.Guild.Id, channel.Id);
            await ctx.GuildLogAsync(emb => {
                emb.WithLocalizedTitle(TranslationKey.evt_sb_upd);
                emb.WithColor(this.ModuleColor);
                emb.WithLocalizedDescription(TranslationKey.evt_sb_chn(channel.Mention));
            });

            await ctx.InfoAsync(this.ModuleColor, TranslationKey.evt_sb_chn(channel.Mention));
        }
        #endregion

        #region starboard sensitivity
        [Command("sensitivity")]
        [Aliases("setsensitivity", "setsens", "sens", "s")]
        public async Task SensitivityAsync(CommandContext ctx,
                                          [Description(TranslationKey.desc_sens)] int? sens = null)
        {
            if (sens is null) {
                sens = this.Service.GetStarboardSensitivity(ctx.Guild.Id);
                await ctx.ImpInfoAsync(this.ModuleColor, Emojis.Star, TranslationKey.evt_sb_sens(sens));
                return;
            }

            if (sens is < 1)
                throw new CommandFailedException(ctx, TranslationKey.cmd_err_range_sens_g(0));

            await this.Service.SetStarboardSensitivityAsync(ctx.Guild.Id, sens.Value);
            await ctx.GuildLogAsync(emb => {
                emb.WithLocalizedTitle(TranslationKey.evt_sb_upd);
                emb.WithColor(this.ModuleColor);
                emb.WithLocalizedDescription(TranslationKey.evt_sb_sens(sens.Value));
            });

            await ctx.InfoAsync(this.ModuleColor, TranslationKey.evt_sb_sens(sens.Value));
        }
        #endregion

        #region starboard emoji
        [Command("emoji")]
        [Aliases("e", "star")]
        public async Task EmojiAsync(CommandContext ctx,
                                    [Description(TranslationKey.desc_emoji)] DiscordEmoji? emoji = null)
        {
            if (emoji is null) {
                string star = this.Service.GetStarboardEmoji(ctx.Guild.Id);
                await ctx.ImpInfoAsync(this.ModuleColor, Emojis.Star, TranslationKey.evt_sb_emoji(star));
                return;
            }

            string emojiStr = emoji.GetDiscordName();
            await this.Service.SetStarboardEmojiAsync(ctx.Guild.Id, emojiStr);
            await ctx.GuildLogAsync(emb => {
                emb.WithLocalizedTitle(TranslationKey.evt_sb_upd);
                emb.WithColor(this.ModuleColor);
                emb.WithLocalizedDescription(TranslationKey.evt_sb_emoji(emojiStr));
            });

            await ctx.InfoAsync(this.ModuleColor, TranslationKey.evt_sb_emoji(emojiStr));
        }
        #endregion


        #region internals
        private void PerformChannelChecks(CommandContext ctx, DiscordChannel chn)
        {
            if (chn.Type != ChannelType.Text)
                throw new InvalidCommandUsageException(ctx, TranslationKey.cmd_err_sb_chn_type);
        }

        private async Task InternalStarboardAsync(CommandContext ctx, bool enable, DiscordChannel channel, DiscordEmoji? emoji = null, int? sens = null)
        {
            if (enable)
                await this.Service.ModifySettingsAsync(ctx.Guild.Id, channel.Id, emoji?.GetDiscordName(), sens);
            else
                await this.Service.ModifySettingsAsync(ctx.Guild.Id, null);

            this.PerformChannelChecks(ctx, channel);

            await ctx.GuildLogAsync(emb => {
                emb.WithLocalizedTitle(TranslationKey.evt_sb_upd);
                emb.WithColor(this.ModuleColor);
                emb.AddLocalizedField(TranslationKey.str_status, emoji is null ? TranslationKey.str_disabled : TranslationKey.str_enabled, inline: true);
                emb.AddLocalizedField(TranslationKey.str_chn, channel.Mention, inline: true, unknown: false);
                emb.AddLocalizedField(TranslationKey.str_star, emoji ?? Emojis.Star, inline: true, unknown: false);
                emb.AddLocalizedField(TranslationKey.str_sensitivity, this.Service.GetStarboardSensitivity(ctx.Guild.Id), inline: true);
            });

            await this.ExecuteGroupAsync(ctx);
        }
        #endregion
    }
}
