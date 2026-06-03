using System.Text.RegularExpressions;
using Discord.Commands;
using TheGodfather.Attributes;

namespace TheGodfather.Modules.Administration;

[Group("reactionroles")]
[Alias("rr", "reactionrole", "reactroles", "reactionrl", "reactrole", "reactr", "reactrl", "rrole")]
[RequireContext(ContextType.Guild)]
[RequireUserPermission(GuildPermission.ManageGuild)]
[RequireBotPermission(GuildPermission.ManageRoles)]
[Cooldown(3, 5, CooldownBucketType.Guild)]
public sealed class ReactionRolesModule : TheGodfatherModule
{
    private static readonly Regex _messageLinkRegex = new(@"discord(?:app)?\.com/channels/\d+/(\d+)/(\d+)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private readonly ReactionRoleService reactionRoles;


    public ReactionRolesModule(LocalizationService localization, ReactionRoleService reactionRoles)
        : base(localization, new Color(0x3498DB))
    {
        this.reactionRoles = reactionRoles;
    }


    [Command("")]
    [Priority(1)]
    public Task ExecuteGroupAsync(string emoji, IRole role, string? message = null)
        => this.AddAsync(emoji, role, message);

    [Command("")]
    [Priority(0)]
    public Task ExecuteGroupAsync()
        => this.ListAsync();

    [Command("add")]
    [Priority(1)]
    [Alias("register", "reg", "a", "+", "+=", "<<", "<", "<-", "<=")]
    public Task AddAsync(IRole role, string emoji, string? message = null)
        => this.AddAsync(emoji, role, message);

    [Command("add")]
    [Priority(0)]
    public async Task AddAsync(string emoji, IRole role, string? message = null)
    {
        string emojiName = NormalizeEmoji(emoji);
        (ulong ChannelId, ulong MessageId, IUserMessage? Message) messageRef = await this.ResolveMessageReferenceAsync(message);

        ReactionRole? existing = await this.reactionRoles.GetAsync(this.Context.Guild.Id, emojiName, messageRef.ChannelId, messageRef.MessageId);
        if (existing is not null)
            throw new LocalizedCommandException(TranslationKey.cmd_err_rr);

        if (messageRef.MessageId == 0)
            await this.reactionRoles.RemoveByEmojiAsync(this.Context.Guild.Id, new[] { emojiName });

        await this.reactionRoles.AddAsync(new ReactionRole {
            GuildId = this.Context.Guild.Id,
            Emoji = emojiName,
            ChannelId = messageRef.ChannelId,
            MessageId = messageRef.MessageId,
            RoleId = role.Id
        });

        if (messageRef.Message is not null && TryParseEmote(emojiName, out IEmote? emote))
            await messageRef.Message.AddReactionAsync(emote);

        string messageText = messageRef.MessageId == 0
            ? string.Empty
            : $" on {Format.Url(messageRef.MessageId.ToString(), BuildJumpLink(messageRef.ChannelId, messageRef.MessageId))}";
        await this.InfoAsync(this.ModuleColor, $"{role.Mention} assigned by {emojiName}{messageText}.");
    }

    [Command("delete")]
    [Priority(2)]
    [Alias("unregister", "remove", "rm", "del", "d", "-", "-=", ">", ">>", "->", "=>")]
    public async Task RemoveAsync(params IRole[] roles)
    {
        if (roles is null || !roles.Any()) {
            await this.RemoveAllAsync();
            return;
        }

        IReadOnlyList<ReactionRole> all = await this.reactionRoles.GetAllAsync(this.Context.Guild.Id);
        int removed = await this.reactionRoles.RemoveAsync(all.Where(rr => roles.Any(role => role.Id == rr.RoleId)));
        await this.InfoAsync(this.ModuleColor, TranslationKey.fmt_rr_rem(removed));
    }

    [Command("delete emoji")]
    [Alias("remove emoji", "delete e", "remove e")]
    public async Task RemoveEmojiAsync(params string[] emojis)
    {
        if (emojis is null || !emojis.Any()) {
            await this.RemoveAllAsync();
            return;
        }

        int removed = await this.reactionRoles.RemoveByEmojiAsync(this.Context.Guild.Id, emojis.Select(NormalizeEmoji));
        await this.InfoAsync(this.ModuleColor, TranslationKey.fmt_rr_rem(removed));
    }

    [Command("delete channel")]
    [Alias("remove channel", "delete chn", "remove chn")]
    public async Task RemoveChannelAsync(params ITextChannel[] channels)
    {
        if (channels is null || !channels.Any()) {
            await this.RemoveAllAsync();
            return;
        }

        int removed = await this.reactionRoles.RemoveByChannelAsync(this.Context.Guild.Id, channels.Select(c => c.Id));
        await this.InfoAsync(this.ModuleColor, TranslationKey.fmt_rr_rem(removed));
    }

    [Command("delete message")]
    [Alias("remove message", "delete msg", "remove msg")]
    public async Task RemoveMessageAsync(params string[] messageRefs)
    {
        if (messageRefs is null || !messageRefs.Any()) {
            await this.RemoveAllAsync();
            return;
        }

        var ids = new List<(ulong ChannelId, ulong MessageId)>();
        foreach (string messageRef in messageRefs) {
            (ulong ChannelId, ulong MessageId, _) = await this.ResolveMessageReferenceAsync(messageRef);
            if (ChannelId != 0 && MessageId != 0)
                ids.Add((ChannelId, MessageId));
        }

        int removed = await this.reactionRoles.RemoveByMessageAsync(this.Context.Guild.Id, ids);
        await this.InfoAsync(this.ModuleColor, TranslationKey.fmt_rr_rem(removed));
    }

    [Command("deleteall")]
    [Alias("removeall", "rmrf", "rma", "clearall", "clear", "delall", "da", "cl", "-a", "--", ">>>")]
    public async Task RemoveAllAsync()
    {
        if (!await this.ConfirmAsync(TranslationKey.q_rr_rem_all))
            return;

        await this.reactionRoles.ClearAsync(this.Context.Guild.Id);
        await this.InfoAsync(this.ModuleColor, TranslationKey.evt_rr_clear);
    }

    [Command("list")]
    [Alias("print", "show", "view", "ls", "l", "p")]
    public async Task ListAsync()
    {
        IReadOnlyList<ReactionRole> rrs = await this.reactionRoles.GetAllAsync(this.Context.Guild.Id);
        if (!rrs.Any()) {
            await this.ImpInfoAsync(this.ModuleColor, TranslationKey.cmd_err_rr_none);
            return;
        }

        await this.RespondPagedAsync(
            TranslationKey.str_rr,
            rrs.OrderBy(rr => rr.RoleId).ThenBy(rr => rr.Emoji),
            rr => {
                SocketRole? role = this.Context.Guild.GetRole(rr.RoleId);
                string roleText = role?.Mention ?? rr.RoleId.ToString();
                string messageText = rr.MessageId == 0 ? "" : $" {Format.Url("msg", BuildJumpLink(rr.ChannelId, rr.MessageId))}";
                return $"{rr.Emoji} | {roleText}{messageText}";
            }
        );
    }


    internal static string NormalizeEmoji(string emoji)
        => Emote.TryParse(emoji, out Emote? emote) ? emote.ToString() : emoji;

    internal static bool TryParseEmote(string emoji, out IEmote? emote)
    {
        if (Emote.TryParse(emoji, out Emote? custom)) {
            emote = custom;
            return true;
        }

        try {
            emote = new Emoji(emoji);
            return true;
        } catch {
            emote = null;
            return false;
        }
    }

    private async Task<(ulong ChannelId, ulong MessageId, IUserMessage? Message)> ResolveMessageReferenceAsync(string? message)
    {
        if (string.IsNullOrWhiteSpace(message))
            return (0, 0, null);

        ulong cid;
        ulong mid;
        Match match = _messageLinkRegex.Match(message);
        if (match.Success) {
            cid = ulong.Parse(match.Groups[1].Value);
            mid = ulong.Parse(match.Groups[2].Value);
        } else if (ulong.TryParse(message, out mid)) {
            cid = this.Context.Channel.Id;
        } else {
            throw new LocalizedCommandException(TranslationKey.cmd_err_msg_404);
        }

        ITextChannel? channel = this.Context.Guild.GetTextChannel(cid);
        IUserMessage? msg = channel is null ? null : await channel.GetMessageAsync(mid) as IUserMessage;
        if (msg is null)
            throw new LocalizedCommandException(TranslationKey.cmd_err_msg_404);

        return (cid, mid, msg);
    }

    private string BuildJumpLink(ulong channelId, ulong messageId)
        => $"https://discord.com/channels/{this.Context.Guild.Id}/{channelId}/{messageId}";
}
