using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.CommandsNext.Exceptions;
using DSharpPlus.Entities;
using DSharpPlus.VoiceNext;

namespace TheGodfather.Modules.Music;

[Group("voice")][Module(ModuleType.Music)][Hidden]
[Aliases("v")]
[RequireGuild][RequirePrivilegedUser]
[Cooldown(3, 5, CooldownBucketType.Guild)]
public sealed class VoiceModule : TheGodfatherModule
{
    #region voice connect
    [Command("connect")]
    [Aliases("c", "con", "conn")]
    public Task ConnectAsync(CommandContext ctx,
        [RemainingText][Description(TranslationKey.desc_chn_voice)] DiscordChannel? channel = null)
    {
        channel ??= ctx.Member.VoiceState?.Channel;
        if (channel is null)
            throw new CommandFailedException(ctx, TranslationKey.cmd_err_music_vc);

        if (channel.Type != ChannelType.Voice)
            throw new CommandFailedException(ctx, TranslationKey.cmd_err_chn_type_voice);

        if (!channel.PermissionsFor(ctx.Guild.CurrentMember).HasPermission(Permissions.AccessChannels))
            throw new ChecksFailedException(ctx.Command, ctx, new[] { new RequireBotPermissionsAttribute(Permissions.AccessChannels) });

        return ctx.Client.GetVoiceNext().ConnectAsync(channel);
    }
    #endregion

    #region voice disconnect
    [Command("disconnect")]
    [Aliases("d", "disconn", "dc")]
    public Task DisonnectAsync(CommandContext ctx)
    {
        ctx.Client.GetVoiceNext().GetConnection(ctx.Guild)?.Disconnect();
        return Task.CompletedTask;
    }
    #endregion
}