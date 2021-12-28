using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Enums;
using DSharpPlus.Interactivity.EventHandling;
using DSharpPlus.Interactivity.Extensions;
using TheGodfather.Services.Common;

namespace TheGodfather.Modules.Administration;

[Group("message")][Module(ModuleType.Administration)][NotBlocked]
[Aliases("m", "msg", "msgs", "messages")]
[Cooldown(3, 5, CooldownBucketType.Channel)]
public sealed partial class MessageModule : TheGodfatherModule
{
    #region message attachments
    [Command("attachments")]
    [Aliases("a", "files", "la")]
    [RequirePermissions(Permissions.ReadMessageHistory)]
    public async Task ListAttachmentsAsync(CommandContext ctx,
        [Description(TranslationKey.desc_msg)] DiscordMessage? msg = null)
    {
        msg ??= await ctx.Channel.GetLastMessageAsync();
        if (msg is null || msg.Attachments.Count > DiscordLimits.EmbedFieldLimit)
            throw new CommandFailedException(ctx, TranslationKey.cmd_err_msg_404);

        await ctx.RespondWithLocalizedEmbedAsync(emb => {
            emb.WithLocalizedTitle(TranslationKey.str_attachments);
            emb.WithColor(this.ModuleColor);
            foreach (DiscordAttachment attachment in msg.Attachments)
                emb.AddField($"{Formatter.Strip(attachment.FileName)} ({attachment.FileSize.ToMetric()})", attachment.Url);
        });
    }
    #endregion

    #region message flag
    [Command("flag")]
    [Aliases("f")]
    [RequirePermissions(Permissions.ReadMessageHistory)][RequireBotPermissions(Permissions.ManageMessages)]
    [Cooldown(1, 60, CooldownBucketType.User)]
    public async Task FlagMessageAsync(CommandContext ctx,
        [Description(TranslationKey.desc_msg)] DiscordMessage? msg = null,
        [Description(TranslationKey.desc_voting_timespan)] TimeSpan? timespan = null)
    {
        msg ??= await ctx.Channel.GetLastMessageAsync();
        if (msg is null)
            throw new CommandFailedException(ctx, TranslationKey.cmd_err_msg_404);

        if (timespan?.TotalSeconds is < 5 or > 300)
            throw new InvalidCommandUsageException(ctx, TranslationKey.cmd_err_timespan(5, 300));

        IEnumerable<PollEmoji> res = await msg.DoPollAsync(
            new[] { Emojis.ArrowUp, Emojis.ArrowDown },
            PollBehaviour.DeleteEmojis,
            timespan ?? TimeSpan.FromMinutes(1)
        );
        var votes = res.ToDictionary(pe => pe.Emoji, pe => pe.Total);
        int votesFor = votes.GetValueOrDefault(Emojis.ArrowUp);
        int votesAgainst = votes.GetValueOrDefault(Emojis.ArrowDown);
        if (votesAgainst > 2 * (votesFor > 0 ? votesFor : 1)) {
            string sanitized = Formatter.Spoiler(Formatter.Strip(msg.Content));
            await msg.DeleteAsync("_gf: Flagged for deletion");
            await ctx.RespondWithLocalizedEmbedAsync(emb => {
                emb.WithColor(this.ModuleColor);
                emb.WithLocalizedDescription(TranslationKey.fmt_flag(msg.Author.Mention, votesAgainst, sanitized));
            });
        } else {
            await ctx.FailAsync(TranslationKey.cmd_err_flag);
        }
    }
    #endregion

    #region message listpinned
    [Command("listpinned")]
    [Aliases("lp", "listpins", "listpin", "pinned")]
    public async Task ListPinnedMessagesAsync(CommandContext ctx,
        [Description(TranslationKey.desc_chn_pins)] DiscordChannel? chn = null)
    {
        chn ??= ctx.Channel;
        if (!chn.PermissionsFor(ctx.Member).HasPermission(Permissions.AccessChannels))
            throw new CommandFailedException(ctx, TranslationKey.cmd_chk_perms_usr(Permissions.AccessChannels));

        IReadOnlyList<DiscordMessage> pinned = await ctx.Channel.GetPinnedMessagesAsync();
        if (!pinned.Any())
            throw new CommandFailedException(ctx, TranslationKey.cmd_err_pinned_none);

        IEnumerable<Page> pages = pinned.Select(m => new Page(
            $"{Formatter.Bold(m.Author.Username)} @ {this.Localization.GetLocalizedTimeString(ctx.Guild.Id, m.CreationTimestamp)}",
            // TODO 
            GetFirstEmbedOrDefaultAsBuilder(m).AddField("URL", Formatter.MaskedUrl(this.Localization.GetString(ctx.Guild.Id, TranslationKey.str_jumplink), m.JumpLink))
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
        [Description(TranslationKey.desc_msg)] DiscordMessage? msg = null)
    {
        msg ??= await ctx.Channel.GetLastMessageAsync();
        if (msg is null)
            throw new CommandFailedException(ctx, TranslationKey.cmd_err_msg_404);
        await msg.PinAsync();
    }
    #endregion

    #region message unpin
    [Command("unpin")][Priority(1)]
    [Aliases("up")]
    [RequirePermissions(Permissions.ManageMessages)]
    public async Task UnpinMessageAsync(CommandContext ctx,
        [Description(TranslationKey.desc_msg)] DiscordMessage message)
    {
        await message.UnpinAsync();
        await ctx.InfoAsync(this.ModuleColor);
    }

    [Command("unpin")][Priority(0)]
    public async Task UnpinMessageAsync(CommandContext ctx,
        [Description(TranslationKey.desc_index_1)] int index = 1)
    {
        IReadOnlyList<DiscordMessage> pinned = await ctx.Channel.GetPinnedMessagesAsync();
        if (index < 1 || index > pinned.Count)
            throw new CommandFailedException(ctx, TranslationKey.cmd_err_index(1, pinned.Count));

        await pinned.ElementAt(index - 1).UnpinAsync();
        await ctx.InfoAsync(this.ModuleColor);
    }
    #endregion

    #region message unpinall
    [Command("unpinall")]
    [Aliases("upa")]
    [RequirePermissions(Permissions.ManageMessages)]
    public async Task UnpinAllMessagesAsync(CommandContext ctx,
        [Description(TranslationKey.desc_chn_pins_del)] DiscordChannel? chn = null)
    {
        chn ??= ctx.Channel;
        if (!chn.PermissionsFor(ctx.Member).HasPermission(Permissions.AccessChannels))
            throw new CommandFailedException(ctx, TranslationKey.cmd_chk_perms_usr(Permissions.AccessChannels));

        if (!await ctx.WaitForBoolReplyAsync(TranslationKey.q_unpin_all(chn)))
            return;

        IReadOnlyList<DiscordMessage> pinned = await ctx.Channel.GetPinnedMessagesAsync();
        if (!pinned.Any())
            throw new CommandFailedException(ctx, TranslationKey.cmd_err_pinned_none);

        await Task.WhenAll(pinned.Select(m => m.UnpinAsync()));
        await ctx.InfoAsync(this.ModuleColor);
    }
    #endregion
}