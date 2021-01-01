using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Enums;
using DSharpPlus.Interactivity.EventHandling;
using DSharpPlus.Interactivity.Extensions;
using Humanizer;
using TheGodfather.Attributes;
using TheGodfather.Common;
using TheGodfather.Exceptions;
using TheGodfather.Extensions;
using TheGodfather.Modules.Administration.Common;

namespace TheGodfather.Modules.Administration
{
    [Group("message"), Module(ModuleType.Administration), NotBlocked]
    [Aliases("m", "msg", "msgs", "messages")]
    [Cooldown(3, 5, CooldownBucketType.Channel)]
    public sealed partial class MessageModule : TheGodfatherModule
    {
        #region message attachments
        [Command("attachments")]
        [Aliases("a", "files", "la")]
        [RequirePermissions(Permissions.ReadMessageHistory)]
        public async Task ListAttachmentsAsync(CommandContext ctx,
                                              [Description("desc-msg")] DiscordMessage? msg = null)
        {
            msg ??= await ctx.Channel.GetLastMessageAsync();
            if (msg is null || msg.Attachments.Count > DiscordLimits.EmbedFieldLimit)
                throw new CommandFailedException(ctx, "cmd-err-msg-404");

            await ctx.RespondWithLocalizedEmbedAsync(emb => {
                emb.WithLocalizedTitle("str-attachments");
                emb.WithColor(this.ModuleColor);
                foreach (DiscordAttachment attachment in msg.Attachments)
                    emb.AddField($"{Formatter.Strip(attachment.FileName)} ({attachment.FileSize.ToMetric()})", attachment.Url);
            });
        }
        #endregion

        #region message flag
        [Command("flag")]
        [Aliases("f")]
        [RequirePermissions(Permissions.ReadMessageHistory), RequireBotPermissions(Permissions.ManageMessages)]
        [Cooldown(1, 60, CooldownBucketType.User)]
        public async Task FlagMessageAsync(CommandContext ctx,
                                          [Description("desc-msg")] DiscordMessage? msg = null,
                                          [Description("desc-voting-timespan")] TimeSpan? timespan = null)
        {
            msg ??= await ctx.Channel.GetLastMessageAsync();
            if (msg is null)
                throw new CommandFailedException(ctx, "cmd-err-msg-404");

            if (timespan?.TotalSeconds is < 5 or > 300)
                throw new InvalidCommandUsageException(ctx, "cmd-err-timespan", 5, 300);

            IEnumerable<PollEmoji> res = await msg.DoPollAsync(
                new[] { Emojis.ArrowUp, Emojis.ArrowDown },
                PollBehaviour.DeleteEmojis,
                timeoutOverride: timespan ?? TimeSpan.FromMinutes(1)
            );
            var votes = res.ToDictionary(pe => pe.Emoji, pe => pe.Total);
            if (votes.GetValueOrDefault(Emojis.ArrowDown) > 2 * votes.GetValueOrDefault(Emojis.ArrowUp)) {
                string sanitized = Formatter.Strip(msg.Content);
                await msg.DeleteAsync("_gf: Flagged for deletion");
                await ctx.RespondWithLocalizedEmbedAsync(emb => {
                    emb.WithColor(this.ModuleColor);
                    emb.WithLocalizedDescription("fmt-filter", msg.Author.Mention, sanitized);
                });
            } else {
                await ctx.FailAsync("cmd-err-flag");
            }
        }
        #endregion

        #region message listpinned
        [Command("listpinned")]
        [Aliases("lp", "listpins", "listpin", "pinned")]
        public async Task ListPinnedMessagesAsync(CommandContext ctx,
                                                 [Description("desc-chn-pins")] DiscordChannel? chn = null)
        {
            chn ??= ctx.Channel;
            if (!chn.PermissionsFor(ctx.Member).HasPermission(Permissions.AccessChannels))
                throw new CommandFailedException(ctx, "cmd-chk-perms-usr", Permissions.AccessChannels);

            IReadOnlyList<DiscordMessage> pinned = await ctx.Channel.GetPinnedMessagesAsync();
            if (!pinned.Any())
                throw new CommandFailedException(ctx, "cmd-err-pinned-none");

            IEnumerable<Page> pages = pinned.Select(m => new Page(
                $"{Formatter.Bold(m.Author.Username)} @ {this.Localization.GetLocalizedTime(ctx.Guild.Id, m.CreationTimestamp)}",
                // TODO 
                GetFirstEmbedOrDefaultAsBuilder(m).AddField("URL", Formatter.MaskedUrl(this.Localization.GetString(ctx.Guild.Id, "str-jumplink"), m.JumpLink))
            ));

            await ctx.Client.GetInteractivity().SendPaginatedMessageAsync(ctx.Channel, ctx.User, pages);

            DiscordEmbedBuilder GetFirstEmbedOrDefaultAsBuilder(DiscordMessage m)
            {
                DiscordEmbed? em = m.Embeds.FirstOrDefault();
                if (em is { })
                    return new DiscordEmbedBuilder(em);

                var emb = new LocalizedEmbedBuilder(this.Localization, ctx.Guild.Id);
                if (!string.IsNullOrWhiteSpace(m.Content))
                    emb.WithDescription(m.Content);
                return emb.GetBuilder();
            }
        }
        #endregion

        #region message pin
        [Command("pin")]
        [Aliases("p")]
        [RequirePermissions(Permissions.ManageMessages)]
        public async Task PinMessageAsync(CommandContext ctx,
                                         [Description("desc-msg")] DiscordMessage? msg = null)
        {
            msg ??= await ctx.Channel.GetLastMessageAsync();
            if (msg is null)
                throw new CommandFailedException(ctx, "cmd-err-msg-404");
            await msg.PinAsync();
        }
        #endregion

        #region message unpin
        [Command("unpin"), Priority(1)]
        [Aliases("up")]
        [RequirePermissions(Permissions.ManageMessages)]
        public async Task UnpinMessageAsync(CommandContext ctx,
                                           [Description("desc-msg")] DiscordMessage message)
        {
            await message.UnpinAsync();
            await ctx.InfoAsync(this.ModuleColor);
        }

        [Command("unpin"), Priority(0)]
        public async Task UnpinMessageAsync(CommandContext ctx,
                                           [Description("desc-index-1")] int index = 1)
        {
            IReadOnlyList<DiscordMessage> pinned = await ctx.Channel.GetPinnedMessagesAsync();
            if (index < 1 || index > pinned.Count)
                throw new CommandFailedException(ctx, "cmd-err-index", 1, pinned.Count);

            await pinned.ElementAt(index - 1).UnpinAsync();
            await ctx.InfoAsync(this.ModuleColor);
        }
        #endregion

        #region message unpinall
        [Command("unpinall")]
        [Aliases("upa")]
        [RequirePermissions(Permissions.ManageMessages)]
        public async Task UnpinAllMessagesAsync(CommandContext ctx,
                                               [Description("desc-chn-pins-del")] DiscordChannel? chn = null)
        {
            chn ??= ctx.Channel;
            if (!chn.PermissionsFor(ctx.Member).HasPermission(Permissions.AccessChannels))
                throw new CommandFailedException(ctx, "cmd-chk-perms-usr", Permissions.AccessChannels);

            if (!await ctx.WaitForBoolReplyAsync("q-unpin-all", args: chn))
                return;

            IReadOnlyList<DiscordMessage> pinned = await ctx.Channel.GetPinnedMessagesAsync();
            if (!pinned.Any())
                throw new CommandFailedException(ctx, "cmd-err-pinned-none");

            await Task.WhenAll(pinned.Select(m => m.UnpinAsync()));
            await ctx.InfoAsync(this.ModuleColor);
        }
        #endregion
    }
}