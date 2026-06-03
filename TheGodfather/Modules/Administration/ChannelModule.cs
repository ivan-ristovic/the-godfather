using Discord.Commands;
using TheGodfather.Attributes;

namespace TheGodfather.Modules.Administration;

[Group("channel")]
[Alias("channels", "chn", "chns", "ch")]
[RequireContext(ContextType.Guild)]
[Cooldown(3, 5, CooldownBucketType.Guild)]
public sealed class ChannelModule : TheGodfatherModule
{
    public ChannelModule(LocalizationService localization)
        : base(localization, new Color(0x3498DB)) { }


    [Command("")]
    public Task ExecuteGroupAsync()
        => this.InfoAsync(this.Context.Channel as IGuildChannel);

    [Command("category")]
    [RequireUserPermission(GuildPermission.ManageChannels)]
    [RequireBotPermission(GuildPermission.ManageChannels)]
    public async Task CreateCategoryAsync([Remainder] string name)
    {
        await this.Context.Guild.CreateCategoryChannelAsync(name, props => props.Position = 0, new RequestOptions { AuditLogReason = InvocationReason() });
        await this.InfoAsync(this.ModuleColor);
    }

    [Command("text")]
    [Priority(1)]
    [RequireUserPermission(GuildPermission.ManageChannels)]
    [RequireBotPermission(GuildPermission.ManageChannels)]
    public async Task CreateTextAsync(string name, [Remainder] string? topic = null)
    {
        await this.Context.Guild.CreateTextChannelAsync(name, props => {
            if (!string.IsNullOrWhiteSpace(topic))
                props.Topic = topic;
        }, new RequestOptions { AuditLogReason = InvocationReason() });
        await this.InfoAsync(this.ModuleColor);
    }

    [Command("voice")]
    [Priority(1)]
    [RequireUserPermission(GuildPermission.ManageChannels)]
    [RequireBotPermission(GuildPermission.ManageChannels)]
    public async Task CreateVoiceAsync([Remainder] string name)
    {
        await this.Context.Guild.CreateVoiceChannelAsync(name, options: new RequestOptions { AuditLogReason = InvocationReason() });
        await this.InfoAsync(this.ModuleColor);
    }

    [Command("bitrate")]
    [Priority(2)]
    [RequireUserPermission(GuildPermission.ManageChannels)]
    [RequireBotPermission(GuildPermission.ManageChannels)]
    public async Task SetBitrateAsync(IVoiceChannel channel, int bitrate)
    {
        await channel.ModifyAsync(props => props.Bitrate = bitrate, new RequestOptions { AuditLogReason = InvocationReason() });
        await this.InfoAsync(this.ModuleColor);
    }

    [Command("bitrate")]
    [Priority(1)]
    public Task SetBitrateAsync(int bitrate)
        => this.Context.Channel is IVoiceChannel voice
            ? this.SetBitrateAsync(voice, bitrate)
            : this.FailAsync(TranslationKey.cmd_err_chn_type_voice);

    [Command("bitrate")]
    [Priority(0)]
    public Task GetBitrateAsync(IVoiceChannel? channel = null)
    {
        channel ??= this.Context.Channel as IVoiceChannel;
        return channel is null
            ? this.FailAsync(TranslationKey.cmd_err_chn_type_voice)
            : this.InfoAsync(this.ModuleColor, $"{channel.Name}: {channel.Bitrate / 1000} kbps");
    }

    [Command("clone")]
    [Alias("copy", "cp")]
    [RequireUserPermission(GuildPermission.ManageChannels)]
    [RequireBotPermission(GuildPermission.ManageChannels)]
    public async Task CloneAsync(IGuildChannel? channel = null)
    {
        channel ??= this.Context.Channel as IGuildChannel;
        if (channel is null) {
            await this.FailAsync("Channel not found.");
            return;
        }

        switch (channel) {
            case ICategoryChannel:
                await this.Context.Guild.CreateCategoryChannelAsync($"{channel.Name}-copy", options: new RequestOptions { AuditLogReason = InvocationReason("Clone channel") });
                break;
            case IVoiceChannel voice:
                await this.Context.Guild.CreateVoiceChannelAsync($"{voice.Name}-copy", props => {
                    props.CategoryId = voice.CategoryId;
                    props.Bitrate = voice.Bitrate;
                    props.UserLimit = voice.UserLimit;
                }, new RequestOptions { AuditLogReason = InvocationReason("Clone channel") });
                break;
            case ITextChannel text:
                await this.Context.Guild.CreateTextChannelAsync($"{text.Name}-copy", props => {
                    props.Topic = text.Topic;
                    props.IsNsfw = text.IsNsfw;
                    props.CategoryId = text.CategoryId;
                    props.SlowModeInterval = text.SlowModeInterval;
                }, new RequestOptions { AuditLogReason = InvocationReason("Clone channel") });
                break;
        }

        await this.InfoAsync(this.ModuleColor);
    }

    [Command("delete")]
    [Priority(1)]
    [Alias("remove", "rm", "del", "d", "-", "-=", ">", ">>")]
    [RequireUserPermission(GuildPermission.ManageChannels)]
    [RequireBotPermission(GuildPermission.ManageChannels)]
    public async Task DeleteAsync(IGuildChannel? channel = null)
    {
        channel ??= this.Context.Channel as IGuildChannel;
        if (channel is null) {
            await this.FailAsync("Channel not found.");
            return;
        }

        if (!await this.ConfirmAsync(TranslationKey.q_chn_del(channel.Name, channel.Id)))
            return;

        await channel.DeleteAsync(new RequestOptions { AuditLogReason = InvocationReason() });
    }

    [Command("info")]
    [Alias("i")]
    public Task InfoAsync(IGuildChannel? channel = null)
    {
        channel ??= this.Context.Channel as IGuildChannel;
        if (channel is null)
            return this.FailAsync("Channel not found.");

        return this.RespondWithLocalizedEmbedAsync(emb => {
            emb.WithTitle(channel.Name);
            emb.WithColor(this.ModuleColor);
            emb.AddLocalizedField(TranslationKey.str_id, channel.Id, true);
            emb.AddLocalizedTimestampField(TranslationKey.str_created_at, channel.CreatedAt, true);
            emb.AddLocalizedField(TranslationKey.str_pos, channel.Position, true);
            emb.AddLocalizedField(TranslationKey.str_type, channel.GetChannelType(), true);
            if (channel is ITextChannel text) {
                emb.AddLocalizedField(TranslationKey.str_topic, text.Topic, false, false);
                emb.AddLocalizedField(TranslationKey.str_nsfw, text.IsNsfw, true);
                emb.AddField("Slowmode", text.SlowModeInterval, true);
            }
            if (channel is IVoiceChannel voice) {
                emb.AddLocalizedField(TranslationKey.str_bitrate, voice.Bitrate / 1000, true);
                emb.AddLocalizedField(TranslationKey.str_user_limit, voice.UserLimit, true);
            }
        });
    }

    [Command("name")]
    [Alias("rename", "n", "mv", "setname")]
    [RequireUserPermission(GuildPermission.ManageChannels)]
    [RequireBotPermission(GuildPermission.ManageChannels)]
    public async Task RenameAsync(IGuildChannel channel, [Remainder] string name)
    {
        await channel.ModifyAsync(props => props.Name = name, new RequestOptions { AuditLogReason = InvocationReason() });
        await this.InfoAsync(this.ModuleColor);
    }

    [Command("rename")]
    public Task RenameCurrentAsync([Remainder] string name)
        => this.Context.Channel is IGuildChannel channel ? this.RenameAsync(channel, name) : this.FailAsync("Channel not found.");

    [Command("nsfw")]
    [Priority(2)]
    [RequireUserPermission(GuildPermission.ManageChannels)]
    [RequireBotPermission(GuildPermission.ManageChannels)]
    public async Task SetNsfwAsync(ITextChannel channel, bool nsfw = true)
    {
        await channel.ModifyAsync(props => props.IsNsfw = nsfw, new RequestOptions { AuditLogReason = InvocationReason() });
        await this.InfoAsync(this.ModuleColor);
    }

    [Command("nsfw")]
    [Priority(1)]
    public Task SetCurrentNsfwAsync(bool nsfw = true)
        => this.Context.Channel is ITextChannel text ? this.SetNsfwAsync(text, nsfw) : this.FailAsync(TranslationKey.cmd_err_chn_type_text);

    [Command("parent")]
    [Alias("setparent")]
    [RequireUserPermission(GuildPermission.ManageChannels)]
    [RequireBotPermission(GuildPermission.ManageChannels)]
    public async Task SetParentAsync(IGuildChannel channel, ICategoryChannel? category = null)
    {
        await channel.ModifyAsync(props => props.CategoryId = category?.Id, new RequestOptions { AuditLogReason = InvocationReason() });
        await this.InfoAsync(this.ModuleColor);
    }

    [Command("position")]
    [Alias("setposition")]
    [RequireUserPermission(GuildPermission.ManageChannels)]
    [RequireBotPermission(GuildPermission.ManageChannels)]
    public async Task SetPositionAsync(IGuildChannel channel, int position)
    {
        await channel.ModifyAsync(props => props.Position = position, new RequestOptions { AuditLogReason = InvocationReason() });
        await this.InfoAsync(this.ModuleColor);
    }

    [Command("slowmode")]
    [Priority(2)]
    [RequireUserPermission(GuildPermission.ManageChannels)]
    [RequireBotPermission(GuildPermission.ManageChannels)]
    public async Task SetSlowmodeAsync(ITextChannel channel, int seconds)
    {
        await channel.ModifyAsync(props => props.SlowModeInterval = seconds, new RequestOptions { AuditLogReason = InvocationReason() });
        await this.InfoAsync(this.ModuleColor);
    }

    [Command("slowmode")]
    [Priority(1)]
    public Task SetCurrentSlowmodeAsync(int seconds)
        => this.Context.Channel is ITextChannel text ? this.SetSlowmodeAsync(text, seconds) : this.FailAsync(TranslationKey.cmd_err_chn_type_text);

    [Command("topic")]
    [Priority(2)]
    [RequireUserPermission(GuildPermission.ManageChannels)]
    [RequireBotPermission(GuildPermission.ManageChannels)]
    public async Task SetTopicAsync(ITextChannel channel, [Remainder] string topic)
    {
        await channel.ModifyAsync(props => props.Topic = topic, new RequestOptions { AuditLogReason = InvocationReason() });
        await this.InfoAsync(this.ModuleColor);
    }

    [Command("topic")]
    [Priority(1)]
    public Task SetCurrentTopicAsync([Remainder] string topic)
        => this.Context.Channel is ITextChannel text ? this.SetTopicAsync(text, topic) : this.FailAsync(TranslationKey.cmd_err_chn_type_text);

    [Command("userlimit")]
    [Priority(2)]
    [RequireUserPermission(GuildPermission.ManageChannels)]
    [RequireBotPermission(GuildPermission.ManageChannels)]
    public async Task SetUserLimitAsync(IVoiceChannel channel, int limit)
    {
        await channel.ModifyAsync(props => props.UserLimit = limit, new RequestOptions { AuditLogReason = InvocationReason() });
        await this.InfoAsync(this.ModuleColor);
    }

    [Command("userlimit")]
    [Priority(1)]
    public Task SetCurrentUserLimitAsync(int limit)
        => this.Context.Channel is IVoiceChannel voice ? this.SetUserLimitAsync(voice, limit) : this.FailAsync(TranslationKey.cmd_err_chn_type_voice);

    [Command("viewperms")]
    [Priority(1)]
    public Task ViewPermsAsync(IGuildChannel? channel = null)
    {
        channel ??= this.Context.Channel as IGuildChannel;
        if (channel is null)
            return this.FailAsync("Channel not found.");

        SocketGuildUser self = this.Context.Guild.CurrentUser;
        ChannelPermissions perms = self.GetPermissions(channel);
        return this.RespondWithLocalizedEmbedAsync(emb => {
            emb.WithTitle(channel.Name);
            emb.WithColor(this.ModuleColor);
            emb.WithDescription(perms.ToList().Select(p => p.ToString()).JoinWith(", "));
        });
    }


    private string InvocationReason(string? reason = null)
        => string.IsNullOrWhiteSpace(reason)
            ? $"Requested by {this.Context.User} ({this.Context.User.Id})"
            : $"{reason} | Requested by {this.Context.User} ({this.Context.User.Id})";
}

internal static class ChannelModuleExtensions
{
    public static string GetChannelType(this IGuildChannel channel)
        => channel switch {
            ICategoryChannel => "Category",
            IVoiceChannel => "Voice",
            ITextChannel => "Text",
            _ => channel.GetType().Name
        };
}
