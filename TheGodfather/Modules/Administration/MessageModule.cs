using System.Text.RegularExpressions;
using Discord.Commands;
using TheGodfather.Attributes;

namespace TheGodfather.Modules.Administration;

[Group("message")]
[Alias("m", "msg", "msgs", "messages")]
[Cooldown(3, 5, CooldownBucketType.Channel)]
public sealed class MessageModule : TheGodfatherModule
{
    private static readonly Regex _messageLinkRegex = new(@"discord(?:app)?\.com/channels/\d+/(\d+)/(\d+)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public MessageModule(LocalizationService localization)
        : base(localization, new Color(0x3498DB)) { }


    [Command("attachments")]
    [Alias("a", "files", "la")]
    [RequireUserPermission(GuildPermission.ViewChannel)]
    public async Task ListAttachmentsAsync(string? message = null)
    {
        IUserMessage? msg = await this.ResolveMessageAsync(message) ?? await this.GetLastMessageAsync();
        if (msg is null || msg.Attachments.Count > DiscordLimits.EmbedFieldLimit)
            throw new LocalizedCommandException(TranslationKey.cmd_err_msg_404);

        await this.RespondWithLocalizedEmbedAsync(emb => {
            emb.WithLocalizedTitle(TranslationKey.str_attachments);
            emb.WithColor(this.ModuleColor);
            foreach (Attachment attachment in msg.Attachments)
                emb.AddField($"{attachment.Filename} ({attachment.Size.ToMetric()})", attachment.Url);
        });
    }

    [Command("listpinned")]
    [Alias("lp", "listpins", "listpin", "pinned")]
    public async Task ListPinnedMessagesAsync(ITextChannel? channel = null)
    {
        channel ??= this.Context.Channel as ITextChannel;
        if (channel is null)
            throw new LocalizedCommandException(TranslationKey.cmd_err_chn_type_text);

        IReadOnlyCollection<IMessage> pinned = await channel.GetPinnedMessagesAsync();
        if (!pinned.Any())
            throw new LocalizedCommandException(TranslationKey.cmd_err_pinned_none);

        await this.RespondPagedAsync(
            TranslationKey.str_pinned,
            pinned.OrderByDescending(m => m.Timestamp),
            msg => $"{Format.Bold(msg.Author.Username)} @ {this.Localization.GetLocalizedTimeString(this.Context.Guild?.Id, msg.Timestamp)} | {Format.Url(this.Localization.GetString(this.Context.Guild?.Id, TranslationKey.str_jumplink), msg.GetJumpUrl())}"
        );
    }

    [Command("pin")]
    [Alias("p")]
    [RequireUserPermission(GuildPermission.ManageMessages)]
    [RequireBotPermission(ChannelPermission.ManageMessages)]
    public async Task PinMessageAsync(string? message = null)
    {
        IUserMessage? msg = await this.ResolveMessageAsync(message) ?? await this.GetLastMessageAsync();
        if (msg is null)
            throw new LocalizedCommandException(TranslationKey.cmd_err_msg_404);

        await msg.PinAsync(new RequestOptions { AuditLogReason = InvocationReason() });
        await this.InfoAsync(this.ModuleColor);
    }

    [Command("unpin")]
    [Priority(1)]
    [Alias("up")]
    [RequireUserPermission(GuildPermission.ManageMessages)]
    [RequireBotPermission(ChannelPermission.ManageMessages)]
    public async Task UnpinMessageAsync(string message)
    {
        IUserMessage? msg = await this.ResolveMessageAsync(message);
        if (msg is null)
            throw new LocalizedCommandException(TranslationKey.cmd_err_msg_404);

        await msg.UnpinAsync(new RequestOptions { AuditLogReason = InvocationReason() });
        await this.InfoAsync(this.ModuleColor);
    }

    [Command("unpin")]
    [Priority(0)]
    public async Task UnpinMessageAtIndexAsync(int index = 1)
    {
        if (this.Context.Channel is not ITextChannel channel)
            throw new LocalizedCommandException(TranslationKey.cmd_err_chn_type_text);

        IReadOnlyCollection<IMessage> pinned = await channel.GetPinnedMessagesAsync();
        if (index < 1 || index > pinned.Count)
            throw new LocalizedCommandException(TranslationKey.cmd_err_index(1, pinned.Count));

        if (pinned.ElementAt(index - 1) is IUserMessage pinnedMessage)
            await pinnedMessage.UnpinAsync(new RequestOptions { AuditLogReason = InvocationReason() });
        await this.InfoAsync(this.ModuleColor);
    }

    [Command("unpinall")]
    [Alias("upa")]
    [RequireUserPermission(GuildPermission.ManageMessages)]
    [RequireBotPermission(ChannelPermission.ManageMessages)]
    public async Task UnpinAllMessagesAsync(ITextChannel? channel = null)
    {
        channel ??= this.Context.Channel as ITextChannel;
        if (channel is null)
            throw new LocalizedCommandException(TranslationKey.cmd_err_chn_type_text);

        if (!await this.ConfirmAsync(TranslationKey.q_unpin_all(channel.Mention)))
            return;

        IReadOnlyCollection<IMessage> pinned = await channel.GetPinnedMessagesAsync();
        if (!pinned.Any())
            throw new LocalizedCommandException(TranslationKey.cmd_err_pinned_none);

        foreach (IUserMessage msg in pinned.OfType<IUserMessage>())
            await msg.UnpinAsync(new RequestOptions { AuditLogReason = InvocationReason() });
        await this.InfoAsync(this.ModuleColor);
    }

    [Command("delete")]
    [Priority(1)]
    [Alias("-", "prune", "del", "d")]
    [RequireUserPermission(GuildPermission.Administrator)]
    [RequireBotPermission(ChannelPermission.ManageMessages)]
    public async Task DeleteAsync(int amount = 1, [Remainder] string? reason = null)
    {
        if (amount is < 1 or > 10000)
            throw new LocalizedCommandException(TranslationKey.cmd_err_msg_del_range(1, 10000));

        if (amount > 10 && !await this.ConfirmAsync(TranslationKey.q_msg_del(amount)))
            return;

        if (this.Context.Channel is not ITextChannel channel)
            throw new LocalizedCommandException(TranslationKey.cmd_err_chn_type_text);

        IMessage[] messages = await FetchMessagesAsync(channel, amount);
        if (!messages.Any())
            throw new LocalizedCommandException(TranslationKey.cmd_err_msg_del_none);

        await DeleteMessagesBatchedAsync(channel, messages, InvocationReason(reason));
    }

    [Command("delete after")]
    [Alias("delete aft", "delete af")]
    [RequireUserPermission(GuildPermission.Administrator)]
    [RequireBotPermission(ChannelPermission.ManageMessages)]
    public Task DeleteMessagesAfterAsync(string message, int amount = 1)
        => this.DeleteRelativeAsync(message, amount, after: true);

    [Command("delete before")]
    [Alias("delete bef", "delete bf")]
    [RequireUserPermission(GuildPermission.Administrator)]
    [RequireBotPermission(ChannelPermission.ManageMessages)]
    public Task DeleteMessagesBeforeAsync(string message, int amount = 1)
        => this.DeleteRelativeAsync(message, amount, after: false);

    [Command("delete from")]
    [Alias("delete f", "delete frm")]
    [RequireUserPermission(GuildPermission.Administrator)]
    [RequireBotPermission(ChannelPermission.ManageMessages)]
    public async Task DeleteMessagesFromUserAsync(IGuildUser member, int amount = 100)
    {
        if (this.Context.Channel is not ITextChannel channel)
            throw new LocalizedCommandException(TranslationKey.cmd_err_chn_type_text);

        IMessage[] messages = await FetchMessagesAsync(channel, Math.Clamp(amount, 1, 10000));
        await DeleteMessagesBatchedAsync(channel, messages.Where(m => m.Author.Id == member.Id), InvocationReason());
    }

    [Command("delete reactions")]
    [Alias("delete react", "delete re")]
    [RequireBotPermission(ChannelPermission.ManageMessages)]
    public async Task DeleteReactionsAsync(string? message = null)
    {
        IUserMessage? msg = await this.ResolveMessageAsync(message) ?? await this.GetLastMessageAsync();
        if (msg is null)
            throw new LocalizedCommandException(TranslationKey.cmd_err_msg_404);

        await msg.RemoveAllReactionsAsync(new RequestOptions { AuditLogReason = InvocationReason() });
        await this.InfoAsync(this.ModuleColor);
    }

    [Command("delete regex")]
    [Alias("delete r", "delete rgx", "delete regexp", "delete reg")]
    [RequireUserPermission(GuildPermission.Administrator)]
    [RequireBotPermission(ChannelPermission.ManageMessages)]
    public async Task DeleteMessagesFromRegexAsync(string pattern, int amount = 5)
    {
        if (!pattern.TryParseRegex(out Regex? regex) || regex is null)
            throw new LocalizedCommandException(TranslationKey.cmd_err_regex);

        if (this.Context.Channel is not ITextChannel channel)
            throw new LocalizedCommandException(TranslationKey.cmd_err_chn_type_text);

        IMessage[] messages = await FetchMessagesAsync(channel, Math.Clamp(amount, 1, 10000));
        await DeleteMessagesBatchedAsync(channel, messages.Where(m => !string.IsNullOrWhiteSpace(m.Content) && regex.IsMatch(m.Content)), InvocationReason());
    }


    private async Task DeleteRelativeAsync(string message, int amount, bool after)
    {
        if (amount is < 1 or > 10000)
            throw new LocalizedCommandException(TranslationKey.cmd_err_msg_del_range(1, 10000));

        if (this.Context.Channel is not ITextChannel channel)
            throw new LocalizedCommandException(TranslationKey.cmd_err_chn_type_text);

        IUserMessage? msg = await this.ResolveMessageAsync(message);
        if (msg is null)
            throw new LocalizedCommandException(TranslationKey.cmd_err_msg_404);

        IMessage[] messages = (await (after
            ? channel.GetMessagesAsync(msg.Id, Direction.After, Math.Min(amount, 100)).FlattenAsync()
            : channel.GetMessagesAsync(msg.Id, Direction.Before, Math.Min(amount, 100)).FlattenAsync())).ToArray();
        await DeleteMessagesBatchedAsync(channel, messages, InvocationReason());
    }

    private static async Task<IMessage[]> FetchMessagesAsync(IMessageChannel channel, int amount)
    {
        var messages = new List<IMessage>();
        ulong? before = null;
        while (messages.Count < amount) {
            int take = Math.Min(100, amount - messages.Count);
            IEnumerable<IMessage> batch = before is null
                ? await channel.GetMessagesAsync(take).FlattenAsync()
                : await channel.GetMessagesAsync(before.Value, Direction.Before, take).FlattenAsync();

            IMessage[] current = batch.ToArray();
            if (!current.Any())
                break;

            messages.AddRange(current);
            before = current.Min(m => m.Id);
        }

        return messages.Take(amount).ToArray();
    }

    private static async Task DeleteMessagesBatchedAsync(ITextChannel channel, IEnumerable<IMessage> messages, string reason)
    {
        foreach (IMessage[] batch in messages.Chunk(100)) {
            if (batch.Length == 1 && batch[0] is IUserMessage single)
                await single.DeleteAsync(new RequestOptions { AuditLogReason = reason });
            else if (batch.Length > 1)
                await channel.DeleteMessagesAsync(batch, new RequestOptions { AuditLogReason = reason });
        }
    }

    private async Task<IUserMessage?> ResolveMessageAsync(string? message)
    {
        if (string.IsNullOrWhiteSpace(message))
            return null;

        ulong cid;
        ulong mid;
        Match match = _messageLinkRegex.Match(message);
        if (match.Success) {
            cid = ulong.Parse(match.Groups[1].Value);
            mid = ulong.Parse(match.Groups[2].Value);
        } else if (ulong.TryParse(message, out mid)) {
            cid = this.Context.Channel.Id;
        } else {
            return null;
        }

        IMessageChannel? channel = this.Context.Client.GetChannel(cid) as IMessageChannel;
        return channel is null ? null : await channel.GetMessageAsync(mid) as IUserMessage;
    }

    private async Task<IUserMessage?> GetLastMessageAsync()
    {
        IMessage[] messages = (await this.Context.Channel.GetMessagesAsync(2).FlattenAsync()).ToArray();
        return messages.FirstOrDefault(m => m.Id != this.Context.Message.Id) as IUserMessage;
    }

    private string InvocationReason(string? reason = null)
        => string.IsNullOrWhiteSpace(reason)
            ? $"Requested by {this.Context.User} ({this.Context.User.Id})"
            : $"{reason} | Requested by {this.Context.User} ({this.Context.User.Id})";
}
