using System.Globalization;
using System.IO;
using System.Text;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.CommandsNext.Exceptions;
using DSharpPlus.Entities;
using Microsoft.Extensions.DependencyInjection;
using TheGodfather.Modules.Administration.Services;
using TheGodfather.Modules.Misc.Extensions;
using TheGodfather.Modules.Misc.Services;
using TimeZoneConverter;

namespace TheGodfather.Modules.Misc;

[Module(ModuleType.Misc)]
[Cooldown(3, 5, CooldownBucketType.Channel)][NotBlocked]
public sealed class MiscModule : TheGodfatherServiceModule<RandomService>
{
    #region 8ball
    [Command("8ball")]
    [Aliases("8b")]
    public Task EightBallAsync(CommandContext ctx,
        [RemainingText][Description(TranslationKey.desc_8b_question)] string question)
    {
        if (string.IsNullOrWhiteSpace(question))
            throw new InvalidCommandUsageException(ctx, TranslationKey.cmd_err_8b);

        TranslationKey answer = this.Service.EightBall(ctx.Channel, question);
        return ctx.ImpInfoAsync(this.ModuleColor, Emojis.EightBall, answer);
    }
    #endregion

    #region coinflip
    [Command("coinflip")]
    [Aliases("coin", "flip")]
    public Task CoinflipAsync(CommandContext ctx,
        [Description(TranslationKey.desc_coinflip_ratio)] int ratio = 1)
    {
        TranslationKey result = this.Service.Coinflip(ratio) 
            ? TranslationKey.fmt_coin_heads(ctx.User.Mention) 
            : TranslationKey.fmt_coin_tails(ctx.User.Mention);
        return ctx.ImpInfoAsync(this.ModuleColor, Emojis.NewMoon, result);
    }
    #endregion

    #region dice
    [Command("dice")]
    [Aliases("die", "roll")]
    public Task DiceAsync(CommandContext ctx,
        [Description(TranslationKey.desc_dice_sides)] int sides = 6)
        => ctx.ImpInfoAsync(this.ModuleColor, Emojis.Dice, TranslationKey.fmt_dice(ctx.User.Mention, this.Service.Dice(sides)));
    #endregion

    #region invite
    [Command("invite")]
    [Aliases("getinvite", "inv")]
    public async Task GetInstantInviteAsync(CommandContext ctx,
        [Description(TranslationKey.desc_invite_expire)] TimeSpan? expiryTime = null)
    {
        DiscordInvite? invite = null;
        if (expiryTime is null) {
            if (ctx.Guild.VanityUrlCode is { }) {
                invite = await ctx.Guild.GetVanityInviteAsync();
            } else {
                IReadOnlyList<DiscordInvite> invites = await ctx.Guild.GetInvitesAsync();
                invite = invites.FirstOrDefault(inv => !inv.IsTemporary);
            }
        }

        if (invite is null || expiryTime is { }) {
            expiryTime ??= TimeSpan.FromSeconds(86400);

            if (!ctx.Guild.CurrentMember.PermissionsIn(ctx.Channel).HasPermission(Permissions.CreateInstantInvite))
                throw new ChecksFailedException(ctx.Command!, ctx, new[] { new RequireBotPermissionsAttribute(Permissions.CreateInstantInvite) });

            // TODO check timespan because only some values are allowed - 400 is thrown
            invite = await ctx.Channel.CreateInviteAsync((int)expiryTime.Value.TotalSeconds, temporary: true, reason: ctx.BuildInvocationDetailsString());
        }

        await ctx.RespondAsync(invite.ToString());
    }
    #endregion

    #region leave
    [Command("leave")][UsesInteractivity]
    [RequireOwnerOrPermissions(Permissions.Administrator)]
    public async Task LeaveAsync(CommandContext ctx)
    {
        if (await ctx.WaitForBoolReplyAsync(TranslationKey.q_leave))
            await ctx.Guild.LeaveAsync();
    }
    #endregion

    #region leet
    [Command("leet")]
    [Aliases("l33t", "1337")]
    public Task L33tAsync(CommandContext ctx,
        [RemainingText][Description(TranslationKey.desc_say)] string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            throw new InvalidCommandUsageException(ctx, TranslationKey.cmd_err_leet_none);

        string leet = this.Service.ToLeet(text);
        return ctx.RespondAsync(new DiscordEmbedBuilder {
            Color = this.ModuleColor,
            Description = $"{Emojis.Information} {leet}"
        });
    }
    #endregion

    #region linux
    [Command("linux")]
    [Aliases("interject", "interjection")]
    public Task LinuxAsync(CommandContext ctx,
        [Description(TranslationKey.desc_replacement)] string? str1 = null,
        [Description(TranslationKey.desc_replacement)] string? str2 = null)
    {
        if (string.IsNullOrWhiteSpace(str1))
            str1 = "GNU";

        if (string.IsNullOrWhiteSpace(str2))
            str2 = "Linux";

        string interjection =
            $"I'd just like to interject for a moment. What you're referring to as {str2}, " +
            $"is in fact, {str1}/{str2}, or as I've recently taken to calling it, {str1} plus {str2}. " +
            $"{str2} is not an operating system unto itself, but rather another free component of a fully " +
            $"functioning {str1} system made useful by the {str1} corelibs, shell utilities and vital " +
            "system components comprising a full OS as defined by POSIX.\n\n" +
            $"Many computer users run a modified version of the {str1} system every day, without realizing it. " +
            $"Through a peculiar turn of events, the version of {str1} which is widely used today is often " +
            $"called {str2}, and many of its users are not aware that it is basically the {str1} system, " +
            $"developed by the {str1} Project.\n\n" +
            $"There really is a {str2}, and these people are using it, but it is just a part of the system " +
            $"they use. {str2} is the kernel: the program in the system that allocates the machine's " +
            "resources to the other programs that you run. The kernel is an essential part of an operating " +
            "system, but useless by itself; it can only function in the context of a complete operating system. " +
            $"{str2} is normally used in combination with the {str1} operating system: the whole system is " +
            $"basically {str1} with {str2} added, or {str1}/{str2}. All the so-called {str2} " +
            $"distributions are really distributions of {str1}/{str2}.";
        return ctx.Channel.EmbedAsync(interjection, color: this.ModuleColor);
    }
    #endregion

    #region penis
    [Command("penis")][Priority(1)]
    [Aliases("size", "length", "manhood", "dick", "dicksize")]
    public Task PenisAsync(CommandContext ctx,
        [Description(TranslationKey.desc_members)] params DiscordMember[] members)
        => this.InternalPenisAsync(ctx, members);

    [Command("penis")][Priority(0)]
    public Task PenisAsync(CommandContext ctx,
        [Description(TranslationKey.desc_users)] params DiscordUser[] users)
        => this.InternalPenisAsync(ctx, users);
    #endregion

    #region penisbros
    [Command("penisbros")][Priority(1)]
    [Aliases("sizebros", "lengthbros", "manhoodbros", "dickbros", "cockbros")]
    [RequireGuild]
    public Task PenisBrosAsync(CommandContext ctx,
        [Description(TranslationKey.desc_member)] DiscordMember member)
        => this.PenisBrosAsync(ctx, member as DiscordUser);

    [Command("penisbros")][Priority(0)]
    public Task PenisBrosAsync(CommandContext ctx,
        [Description(TranslationKey.desc_user)] DiscordUser? user = null)
    {
        user ??= ctx.User;
        if (user.IsCurrent)
            return ctx.InfoAsync(this.ModuleColor, Emojis.Ruler, TranslationKey.cmd_err_size_bot);

        int size = this.Service.Size(user.Id).Length;
        var cockbros = ctx.Guild.Members
                          .Select(kvp => kvp.Value)
                          .Where(m => m != user && m != ctx.Client.CurrentUser && this.Service.Size(m.Id).Length == size)
                          .ToList()
                          ;

        return cockbros.Any()
            ? ctx.PaginateAsync(TranslationKey.fmt_penisbros(user.Mention), cockbros, m => m.Mention, this.ModuleColor)
            : ctx.FailAsync(TranslationKey.cmd_err_penisbros_none(user.Mention));
    }
    #endregion

    #region ping
    [Command("ping")]
    public Task PingAsync(CommandContext ctx)
        => ctx.ImpInfoAsync(this.ModuleColor, Emojis.Heartbeat, TranslationKey.fmt_ping(ctx.Client.Ping));
    #endregion

    #region prefix
    [Command("prefix")]
    [Aliases("setprefix", "pref", "setpref")]
    [RequireGuild][RequireOwnerOrPermissions(Permissions.Administrator)]
    public async Task GetOrSetPrefixAsync(CommandContext ctx,
        [Description(TranslationKey.desc_prefix)] string? prefix = null)
    {
        GuildConfigService gcs = ctx.Services.GetRequiredService<GuildConfigService>();
        if (string.IsNullOrWhiteSpace(prefix)) {
            string p = gcs.GetGuildPrefix(ctx.Guild.Id);
            await ctx.ImpInfoAsync(this.ModuleColor, Emojis.Information, TranslationKey.fmt_prefix(p));
            return;
        }

        if (prefix.Length > GuildConfig.PrefixLimit)
            throw new CommandFailedException(ctx, TranslationKey.cmd_err_prefix(GuildConfig.PrefixLimit));

        await gcs.ModifyConfigAsync(ctx.Guild.Id, cfg => cfg.Prefix = prefix);
        await ctx.InfoAsync(this.ModuleColor);
    }
    #endregion

    #region rate
    [Command("rate")][Priority(1)]
    [Aliases("score", "graph", "rating")]
    [RequireBotPermissions(Permissions.AttachFiles)]
    public Task RateAsync(CommandContext ctx,
        [Description(TranslationKey.desc_members)] params DiscordMember[] members)
        => this.InternalRateAsync(ctx, members);

    [Command("rate")][Priority(0)]
    public Task RateAsync(CommandContext ctx,
        [Description(TranslationKey.desc_users)] params DiscordUser[] users)
        => this.InternalRateAsync(ctx, users);
    #endregion

    #region rip
    [Command("rip")][Priority(1)]
    [Aliases("restinpeace", "grave")]
    [RequireBotPermissions(Permissions.AttachFiles)]
    public Task RateAsync(CommandContext ctx,
        [Description(TranslationKey.desc_member)] DiscordMember member,
        [RemainingText][Description(TranslationKey.desc_rip)] string? desc = "RIP")
        => this.InternalRipAsync(ctx, member, desc);

    [Command("rip")][Priority(0)]
    public Task RateAsync(CommandContext ctx,
        [Description(TranslationKey.desc_user)] DiscordUser user,
        [RemainingText][Description(TranslationKey.desc_rip)] string? desc = "RIP")
        => this.InternalRipAsync(ctx, user, desc);
    #endregion

    #region report
    [Command("report")][UsesInteractivity]
    [Cooldown(1, 3600, CooldownBucketType.User)]
    public async Task SendErrorReportAsync(CommandContext ctx,
        [RemainingText][Description(TranslationKey.desc_issue)] string issue)
    {
        if (string.IsNullOrWhiteSpace(issue))
            throw new InvalidCommandUsageException(ctx, TranslationKey.cmd_err_issue_none);

        DiscordDmChannel? dm = await ctx.Client.CreateOwnerDmChannel();
        if (dm is null)
            throw new CommandFailedException(ctx, TranslationKey.cmd_err_issue_fail);

        if (await ctx.WaitForBoolReplyAsync(TranslationKey.q_issue)) {
            Log.Warning("Report from {User} ({UserId}): {UserReportString}", ctx.User.Username, ctx.User.Id, issue);
            var emb = new DiscordEmbedBuilder {
                Title = "Issue reported",
                Description = issue
            };
            emb.WithAuthor(ctx.User.ToString(), iconUrl: ctx.User.AvatarUrl ?? ctx.User.DefaultAvatarUrl);
            if (ctx.Guild is { })
                emb.AddField("Guild", $"{ctx.Guild} owned by {ctx.Guild.Owner}");

            await dm.SendMessageAsync("A new issue has been reported!", emb.Build());
            await ctx.InfoAsync(this.ModuleColor, TranslationKey.str_reported);
        }
    }
    #endregion

    #region say
    [Command("say")]
    [Aliases("repeat", "echo")]
    public Task SayAsync(CommandContext ctx,
        [RemainingText][Description(TranslationKey.desc_say)] string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            throw new InvalidCommandUsageException(ctx, TranslationKey.cmd_err_text_none);

        return ctx.Services.GetRequiredService<FilteringService>().TextContainsFilter(ctx.Guild.Id, text, out _)
            ? throw new CommandFailedException(ctx, TranslationKey.cmd_err_say)
            : ctx.RespondWithLocalizedEmbedAsync(emb => {
                emb.WithColor(this.ModuleColor);
                emb.WithDescription($"{Emojis.Loudspeaker} {Formatter.Strip(text)}");
            });
    }
    #endregion

    #region simulate
    [Command("simulate")]
    [Aliases("sim")]
    [RequireGuild][Cooldown(1, 5, CooldownBucketType.Guild)]
    public async Task SimulateAsync(CommandContext ctx,
        [Description(TranslationKey.desc_member)] DiscordMember? member = null)
    {
        member ??= ctx.Member;
        if (member == ctx.Guild.CurrentMember)
            member = ctx.Member;

        string prefix = ctx.Services.GetRequiredService<GuildConfigService>().GetGuildPrefix(ctx.Guild.Id);
        string? simulatedMessage = await SimulationService.SimulateAsync(ctx.Channel, member!, prefix);
        if (simulatedMessage is null)
            throw new CommandFailedException(ctx, TranslationKey.cmd_err_sim);
        await ctx.Channel.EmbedAsync(simulatedMessage, Emojis.Loudspeaker, this.ModuleColor);
    }
    #endregion

    #region tts
    [Command("tts")]
    [RequirePermissions(Permissions.SendTtsMessages)]
    public Task TtsAsync(CommandContext ctx,
        [RemainingText][Description(TranslationKey.desc_say)] string text)
    {

        if (string.IsNullOrWhiteSpace(text))
            throw new InvalidCommandUsageException(ctx, TranslationKey.cmd_err_text_none);

        return ctx.Services.GetRequiredService<FilteringService>().TextContainsFilter(ctx.Guild.Id, text, out _)
            ? throw new CommandFailedException(ctx, TranslationKey.cmd_err_say)
            : ctx.RespondAsync(new DiscordMessageBuilder().WithContent(Formatter.BlockCode(Formatter.Strip(text))).WithTTS(true));
    }
    #endregion

    #region time
    [Command("time")]
    [Aliases("t")]
    public Task TimeAsync(CommandContext ctx,
        [RemainingText][Description(TranslationKey.desc_tz)] string? timezone = null)
    {
        if (string.IsNullOrWhiteSpace(timezone)) {
            string time = this.Localization.GetLocalizedTimeString(ctx.Guild?.Id, DateTimeOffset.Now);
            string tz = this.Localization.GetGuildTimeZone(ctx.Guild?.Id).DisplayName;
            return ctx.ImpInfoAsync(this.ModuleColor, Emojis.Clock1, TranslationKey.fmt_time(tz, time));
        }

        if (!TZConvert.TryGetTimeZoneInfo(timezone, out TimeZoneInfo? info))
            throw new CommandFailedException(ctx, TranslationKey.cmd_err_tz);

        CultureInfo culture = this.Localization.GetGuildCulture(ctx.Guild.Id);
        string timeStr = TimeZoneInfo.ConvertTime(DateTimeOffset.Now, info).ToString("G", culture);
        return ctx.ImpInfoAsync(this.ModuleColor, Emojis.Clock1, TranslationKey.fmt_time(info.DisplayName, timeStr));
    }
    #endregion

    #region unleet
    [Command("unleet")]
    [Aliases("unl33t")]
    public Task Unl33tAsync(CommandContext ctx,
        [RemainingText][Description(TranslationKey.desc_say)] string leet)
    {
        if (string.IsNullOrWhiteSpace(leet))
            throw new InvalidCommandUsageException(ctx, TranslationKey.cmd_err_leet_none);

        string text = this.Service.FromLeet(leet);
        return ctx.RespondAsync(new DiscordEmbedBuilder {
            Color = this.ModuleColor,
            Description = $"{Emojis.Information} {text}"
        });
    }
    #endregion


    #region internals
    private Task InternalPenisAsync(CommandContext ctx, IReadOnlyList<DiscordUser> users)
    {
        users = users.Distinct().ToList();
        if (!users.Any())
            users = new[] { ctx.User };
        if (users.Count >= 10)
            throw new InvalidCommandUsageException(ctx, TranslationKey.cmd_err_size);

        var sb = new StringBuilder();
        foreach (DiscordUser user in users) {
            if (user.IsCurrent)
                return ctx.ImpInfoAsync(this.ModuleColor, Emojis.Ruler, TranslationKey.cmd_err_size_bot);
            sb.Append(Formatter.Bold(this.Service.Size(user.Id))).Append(' ').AppendLine(user.Mention);
        }

        return ctx.RespondWithLocalizedEmbedAsync(emb => {
            emb.WithColor(this.ModuleColor);
            emb.WithLocalizedDescription(TranslationKey.fmt_size(Emojis.Ruler, sb.ToString()));
        });
    }

    private async Task InternalRateAsync(CommandContext ctx, IReadOnlyList<DiscordUser> users)
    {
        users = users.Distinct().ToList();
        if (!users.Any())
            users = new[] { ctx.User };
        if (users.Count > 8)
            throw new InvalidCommandUsageException(ctx, TranslationKey.cmd_err_rate);

        if (users.Any(u => u.IsCurrent)) {
            await ctx.ImpInfoAsync(this.ModuleColor, Emojis.Ruler, TranslationKey.cmd_err_size_bot);
            return;
        }

        await using Stream ms = await ctx.Services.GetRequiredService<ImagingService>().RateAsync(users.Select(u => (u.ToDiscriminatorString(), u.Id)));
        await ctx.RespondAsync(new DiscordMessageBuilder()
            .AddFile("Rating.jpg", ms)
            .WithEmbed(new DiscordEmbedBuilder {
                Description = this.Localization.GetString(ctx.Guild?.Id, TranslationKey.fmt_rating(Emojis.Ruler, users.Select(u => u.Mention).JoinWith(", "))),
                Color = this.ModuleColor
            })
        );
    }

    private async Task InternalRipAsync(CommandContext ctx, DiscordUser user, string? desc)
    {
        if (user.IsCurrent) {
            await ctx.ImpInfoAsync(this.ModuleColor, Emojis.Ruler, TranslationKey.cmd_err_size_bot);
            return;
        }

        ImagingService ims = ctx.Services.GetRequiredService<ImagingService>();
        CultureInfo culture = this.Localization.GetGuildCulture(ctx.Guild.Id);
        await using Stream ms = await ims.RipAsync(user.ToDiscriminatorString(), user.AvatarUrl, user.CreationTimestamp, desc, culture);
        await ctx.RespondAsync(new DiscordMessageBuilder().AddFile("Grave.jpg", ms));
    }
    #endregion
}