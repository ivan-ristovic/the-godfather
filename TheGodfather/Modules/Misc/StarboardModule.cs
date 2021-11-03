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
                                     [Description("desc-enable")] bool enable,
                                     [Description("desc-emoji")] DiscordEmoji emoji,
                                     [Description("desc-chn")] DiscordChannel channel,
                                     [Description("desc-sens")] int? sens = null)
            => this.InternalStarboardAsync(ctx, enable, channel, emoji, sens);

        [GroupCommand, Priority(1)]
        public Task ExecuteGroupAsync(CommandContext ctx,
                                     [Description("desc-enable")] bool enable,
                                     [Description("desc-emoji")] DiscordChannel channel,
                                     [Description("desc-chn")] DiscordEmoji? emoji = null,
                                     [Description("desc-sens")] int? sens = null)
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
                emb.WithLocalizedTitle("str-starboard");
                emb.AddLocalizedField("str-status", cid == 0 ? "str-disabled" : "str-enabled", inline: true);
                emb.AddLocalizedTitleField("str-chn", starChn?.Mention, inline: true, unknown: false);
                emb.AddLocalizedTitleField("str-star", starEmoji, inline: true, unknown: false);
                emb.AddLocalizedTitleField("str-sensitivity", this.Service.GetStarboardSensitivity(ctx.Guild.Id), inline: true);
                emb.AddLocalizedTitleField("str-starmsgs", totalMsgs, inline: true, unknown: false);
                emb.AddLocalizedTitleField("str-stars-total", totalStars, inline: true, unknown: false);
            });
        }
        #endregion

        #region starboard channel
        [Command("channel")]
        [Aliases("chn", "setchannel", "setchn", "setc", "location")]
        public async Task ChannelAsync(CommandContext ctx,
                                      [Description("desc-chn")] DiscordChannel? channel = null)
        {
            if (channel is null) {
                ulong cid = this.Service.GetStarboardChannel(ctx.Guild.Id);
                channel = ctx.Guild.GetChannel(cid);
                if (channel is null)
                    throw new CommandFailedException(ctx, "cmd-err-sb-chn", cid);
                await ctx.ImpInfoAsync(this.ModuleColor, Emojis.Star, "evt-sb-shn", channel.Mention);
                return;
            }

            this.PerformChannelChecks(ctx, channel);

            await this.Service.SetStarboardChannelAsync(ctx.Guild.Id, channel.Id);
            await ctx.GuildLogAsync(emb => {
                emb.WithLocalizedTitle("evt-sb-upd");
                emb.WithColor(this.ModuleColor);
                emb.WithLocalizedDescription("evt-sb-chn", channel.Mention);
            });

            await ctx.InfoAsync(this.ModuleColor, "evt-sb-chn", channel.Mention);
        }
        #endregion

        #region starboard sensitivity
        [Command("sensitivity")]
        [Aliases("setsensitivity", "setsens", "sens", "s")]
        public async Task SensitivityAsync(CommandContext ctx,
                                          [Description("desc-sens")] int? sens = null)
        {
            if (sens is null) {
                sens = this.Service.GetStarboardSensitivity(ctx.Guild.Id);
                await ctx.ImpInfoAsync(this.ModuleColor, Emojis.Star, "evt-sb-sens", sens);
                return;
            }

            if (sens is < 1)
                throw new CommandFailedException(ctx, "cmd-err-range-sens", 0);

            await this.Service.SetStarboardSensitivityAsync(ctx.Guild.Id, sens.Value);
            await ctx.GuildLogAsync(emb => {
                emb.WithLocalizedTitle("evt-sb-upd");
                emb.WithColor(this.ModuleColor);
                emb.WithLocalizedDescription("evt-sb-sens", sens.Value);
            });

            await ctx.InfoAsync(this.ModuleColor, "evt-sb-sens", sens.Value);
        }
        #endregion

        #region starboard emoji
        [Command("emoji")]
        [Aliases("e", "star")]
        public async Task EmojiAsync(CommandContext ctx,
                                    [Description("desc-emoji")] DiscordEmoji? emoji = null)
        {
            if (emoji is null) {
                string star = this.Service.GetStarboardEmoji(ctx.Guild.Id);
                await ctx.ImpInfoAsync(this.ModuleColor, Emojis.Star, "evt-sb-emoji", star);
                return;
            }

            string emojiStr = emoji.GetDiscordName();
            await this.Service.SetStarboardEmojiAsync(ctx.Guild.Id, emojiStr);
            await ctx.GuildLogAsync(emb => {
                emb.WithLocalizedTitle("evt-sb-upd");
                emb.WithColor(this.ModuleColor);
                emb.WithLocalizedDescription("evt-sb-emoji", emojiStr);
            });

            await ctx.InfoAsync(this.ModuleColor, "evt-sb-emoji", emojiStr);
        }
        #endregion


        #region internals
        private void PerformChannelChecks(CommandContext ctx, DiscordChannel chn)
        {
            if (chn.Type != ChannelType.Text)
                throw new InvalidCommandUsageException(ctx, "cmd-err-sb-chn-type");
        }

        private async Task InternalStarboardAsync(CommandContext ctx, bool enable, DiscordChannel channel, DiscordEmoji? emoji = null, int? sens = null)
        {
            if (enable)
                await this.Service.ModifySettingsAsync(ctx.Guild.Id, channel.Id, emoji?.GetDiscordName(), sens);
            else
                await this.Service.ModifySettingsAsync(ctx.Guild.Id, null);

            this.PerformChannelChecks(ctx, channel);

            await ctx.GuildLogAsync(emb => {
                emb.WithLocalizedTitle("evt-sb-upd");
                emb.WithColor(this.ModuleColor);
                emb.AddLocalizedField("str-status", emoji is null ? "str-disabled" : "str-enabled", inline: true);
                emb.AddLocalizedTitleField("str-chn", channel.Mention, inline: true, unknown: false);
                emb.AddLocalizedTitleField("str-star", emoji ?? Emojis.Star, inline: true, unknown: false);
                emb.AddLocalizedTitleField("str-sensitivity", this.Service.GetStarboardSensitivity(ctx.Guild.Id), inline: true);
            });

            await this.ExecuteGroupAsync(ctx);
        }
        #endregion
    }
}
