using System.Diagnostics;
using System.Net;
using System.Text;
using Discord.Commands;
using TheGodfather.Attributes;

namespace TheGodfather.Modules.Owner;

[Group("owner")]
[Alias("admin", "o")]
[RequireOwner]
public sealed class OwnerModule : TheGodfatherModule
{
    private readonly BotActivityService activity;


    public OwnerModule(LocalizationService localization, BotActivityService activity)
        : base(localization, new Color(0xE74C3C))
    {
        this.activity = activity;
    }


    [Command("announce")]
    [Alias("ann")]
    public async Task AnnounceAsync([Remainder] string message)
    {
        if (!await this.ConfirmAsync(TranslationKey.q_announcement(Format.Sanitize(message))))
            return;

        var embed = new LocalizedEmbedBuilder(this.Localization, this.Context.Guild?.Id)
            .WithLocalizedTitle(TranslationKey.str_announcement)
            .WithDescription(message)
            .WithColor(Color.Red)
            .Build();

        var errors = new StringBuilder();
        foreach (SocketGuild guild in this.Context.Client.Guilds) {
            IMessageChannel? channel = guild.SystemChannel ?? guild.DefaultChannel ?? guild.TextChannels.FirstOrDefault();
            if (channel is null)
                continue;

            try {
                await channel.SendMessageAsync(embed: embed);
            } catch {
                errors.AppendLine(this.Localization.GetString(this.Context.Guild?.Id, TranslationKey.cmd_err_announce(0, guild.Name, guild.Id)));
            }
        }

        if (errors.Length > 0)
            await this.ImpInfoAsync(this.ModuleColor, TranslationKey.fmt_err(errors.ToString()));
        else
            await this.InfoAsync(this.ModuleColor);
    }

    [Command("avatar")]
    [Alias("setavatar", "setbotavatar", "profilepic", "a")]
    public async Task SetBotAvatarAsync(string url)
    {
        if (!Uri.TryCreate(url, UriKind.Absolute, out Uri? uri))
            throw new LocalizedCommandException(TranslationKey.cmd_err_image_url);

        if (!await uri.ContentTypeHeaderIsImageAsync(DiscordLimits.AvatarSizeLimit))
            throw new LocalizedCommandException(TranslationKey.cmd_err_image_url_fail(DiscordLimits.AvatarSizeLimit));

        try {
            await using MemoryStream ms = await HttpService.GetMemoryStreamAsync(uri);
            await this.Context.Client.CurrentUser.ModifyAsync(props => props.Avatar = new Image(ms));
        } catch (WebException) {
            throw new LocalizedCommandException(TranslationKey.err_url_image_fail);
        }

        await this.InfoAsync(this.ModuleColor);
    }

    [Command("name")]
    [Alias("botname", "setbotname", "setname")]
    public async Task SetBotNameAsync([Remainder] string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new LocalizedCommandException(TranslationKey.cmd_err_missing_name);

        if (name.Length > DiscordLimits.NameLimit)
            throw new LocalizedCommandException(TranslationKey.cmd_err_name(DiscordLimits.NameLimit));

        await this.Context.Client.CurrentUser.ModifyAsync(props => props.Username = name);
        await this.InfoAsync(this.ModuleColor);
    }

    [Command("leaveguilds")]
    [Priority(1)]
    public Task LeaveGuildsAsync(params ulong[] guildIds)
        => this.LeaveGuildsAsync(false, guildIds);

    [Command("leaveguilds")]
    [Priority(0)]
    public async Task LeaveGuildsAsync(bool quiet = false, params ulong[] guildIds)
    {
        var errors = new StringBuilder();
        foreach (ulong guildId in guildIds.Distinct()) {
            SocketGuild? guild = this.Context.Client.GetGuild(guildId);
            if (guild is null) {
                if (!quiet)
                    errors.AppendLine(this.Localization.GetString(this.Context.Guild?.Id, TranslationKey.cmd_err_guild_leave_fail(guildId)));
                continue;
            }

            try {
                await guild.LeaveAsync();
            } catch {
                errors.AppendLine(this.Localization.GetString(this.Context.Guild?.Id, TranslationKey.cmd_err_guild_leave(guildId)));
            }
        }

        if (errors.Length > 0)
            await this.FailAsync(TranslationKey.fmt_err(errors.ToString()));
        else
            await this.InfoAsync(this.ModuleColor);
    }

    [Command("sendmessage")]
    [Alias("send")]
    public Task SendAsync(IMessageChannel channel, [Remainder] string message)
        => channel.SendMessageAsync(message);

    [Command("shutdown")]
    [Priority(1)]
    [Alias("exit", "die", "kill")]
    public Task ExitAsync(int exitCode, int delaySeconds = 0)
        => TheGodfather.Stop(exitCode, TimeSpan.FromSeconds(delaySeconds));

    [Command("shutdown")]
    [Priority(0)]
    public Task ExitAsync(int delaySeconds = 0)
        => this.ExitAsync(0, delaySeconds);

    [Command("sudo")]
    public async Task SudoAsync(IUser user, [Remainder] string command)
    {
        await this.Context.Channel.SendMessageAsync($"{Format.Bold(UserDisplayName(user))}: {command}");
    }

    [Command("toggleignore")]
    [Alias("ti")]
    public Task ToggleIgnoreAsync()
    {
        bool listening = this.activity.ToggleListeningStatus();
        return this.InfoAsync(this.ModuleColor, listening ? TranslationKey.str_off : TranslationKey.str_on);
    }

    [Command("restart")]
    [Alias("reboot")]
    public Task RestartAsync()
        => TheGodfather.Stop(100);

    [Command("update")]
    public Task UpdateAsync()
        => TheGodfather.Stop(101);

    [Command("uptime")]
    public Task UptimeAsync()
    {
        TimeSpan processUptime = this.activity.UptimeInformation.ProgramUptime;
        TimeSpan socketUptime = this.activity.UptimeInformation.SocketUptime;

        return this.RespondWithLocalizedEmbedAsync(emb => {
            emb.WithLocalizedTitle(TranslationKey.str_uptime_info);
            emb.WithDescription($"{TheGodfather.ApplicationName} {TheGodfather.ApplicationVersion}");
            emb.AddLocalizedField(TranslationKey.str_uptime_bot, processUptime.ToString(@"dd\.hh\:mm\:ss"), true);
            emb.AddLocalizedField(TranslationKey.str_uptime_socket, socketUptime.ToString(@"dd\.hh\:mm\:ss"), true);
            emb.WithColor(this.ModuleColor);
        });
    }
}
