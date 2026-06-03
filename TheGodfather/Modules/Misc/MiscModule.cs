using Discord.Commands;
using TheGodfather.Modules.Misc.Services;
using TimeZoneConverter;

namespace TheGodfather.Modules.Misc;

public sealed class MiscModule : TheGodfatherModule
{
    private readonly RandomService random;
    private readonly GuildConfigService guildConfig;


    public MiscModule(LocalizationService localization, RandomService random, GuildConfigService guildConfig)
        : base(localization, new Color(0x007FFF))
    {
        this.random = random;
        this.guildConfig = guildConfig;
    }


    [Command("8ball")]
    [Alias("8b")]
    public Task EightBallAsync([Remainder] string question)
    {
        if (string.IsNullOrWhiteSpace(question))
            return this.FailAsync(TranslationKey.cmd_err_8b);

        TranslationKey answer = this.GetEightBallAnswer(question);
        return this.ImpInfoAsync(this.ModuleColor, Emojis.EightBall, answer);
    }

    [Command("coinflip")]
    [Alias("coin", "flip")]
    public Task CoinflipAsync(int ratio = 1)
    {
        TranslationKey result = this.random.Coinflip(ratio)
            ? TranslationKey.fmt_coin_heads(this.Context.User.Mention)
            : TranslationKey.fmt_coin_tails(this.Context.User.Mention);
        return this.ImpInfoAsync(this.ModuleColor, Emojis.NewMoon, result);
    }

    [Command("dice")]
    [Alias("die", "roll")]
    public Task DiceAsync(int sides = 6)
        => this.ImpInfoAsync(this.ModuleColor, Emojis.Dice, TranslationKey.fmt_dice(this.Context.User.Mention, this.random.Dice(sides)));

    [Command("leet")]
    [Alias("l33t", "1337")]
    public Task L33tAsync([Remainder] string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return this.FailAsync(TranslationKey.cmd_err_leet_none);

        string leet = this.random.ToLeet(text);
        return this.ReplyAsync(embed: new EmbedBuilder {
            Color = this.ModuleColor,
            Description = $"{Emojis.Information} {leet}"
        }.Build());
    }

    [Command("linux")]
    [Alias("interject", "interjection")]
    public Task LinuxAsync(string? str1 = null, string? str2 = null)
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

        return this.ReplyAsync(embed: new EmbedBuilder {
            Color = this.ModuleColor,
            Description = interjection
        }.Build());
    }

    [Command("ping")]
    [Summary("Pings the bot.")]
    public Task PingAsync()
        => this.ImpInfoAsync(this.ModuleColor, Emojis.Heartbeat, TranslationKey.fmt_ping(this.Context.Client.Latency));

    [Command("prefix")]
    [Alias("setprefix", "pref", "setpref")]
    [RequireContext(ContextType.Guild)]
    [RequireUserPermission(GuildPermission.Administrator)]
    public async Task GetOrSetPrefixAsync(string? prefix = null)
    {
        if (string.IsNullOrWhiteSpace(prefix)) {
            string currentPrefix = this.guildConfig.GetGuildPrefix(this.Context.Guild.Id);
            await this.ImpInfoAsync(this.ModuleColor, Emojis.Information, TranslationKey.fmt_prefix(currentPrefix));
            return;
        }

        if (prefix.Length > GuildConfig.PrefixLimit) {
            await this.FailAsync(TranslationKey.cmd_err_prefix(GuildConfig.PrefixLimit));
            return;
        }

        if (!this.guildConfig.IsGuildRegistered(this.Context.Guild.Id))
            await this.guildConfig.RegisterGuildAsync(this.Context.Guild.Id);
        await this.guildConfig.ModifyConfigAsync(this.Context.Guild.Id, cfg => cfg.Prefix = prefix);
        await this.ImpInfoAsync(this.ModuleColor, Emojis.CheckMarkSuccess, TranslationKey.fmt_prefix(prefix));
    }

    [Command("time")]
    [Alias("t")]
    public Task TimeAsync([Remainder] string? timezone = null)
    {
        if (string.IsNullOrWhiteSpace(timezone)) {
            string time = this.Localization.GetLocalizedTimeString(this.Context.Guild?.Id, DateTimeOffset.Now);
            string tz = this.Localization.GetGuildTimeZone(this.Context.Guild?.Id).DisplayName;
            return this.ImpInfoAsync(this.ModuleColor, Emojis.Clock1, TranslationKey.fmt_time(tz, time));
        }

        if (!TZConvert.TryGetTimeZoneInfo(timezone, out TimeZoneInfo? info))
            return this.FailAsync(TranslationKey.cmd_err_tz);

        string timeStr = TimeZoneInfo.ConvertTime(DateTimeOffset.Now, info).ToString("G", this.Localization.GetGuildCulture(this.Context.Guild?.Id));
        return this.ImpInfoAsync(this.ModuleColor, Emojis.Clock1, TranslationKey.fmt_time(info.DisplayName, timeStr));
    }

    [Command("unleet")]
    [Alias("unl33t")]
    public Task Unl33tAsync([Remainder] string leet)
    {
        if (string.IsNullOrWhiteSpace(leet))
            return this.FailAsync(TranslationKey.cmd_err_leet_none);

        string text = this.random.FromLeet(leet);
        return this.ReplyAsync(embed: new EmbedBuilder {
            Color = this.ModuleColor,
            Description = $"{Emojis.Information} {text}"
        }.Build());
    }


    private TranslationKey GetEightBallAnswer(string question)
    {
        if (question.StartsWith("when", StringComparison.InvariantCultureIgnoreCase) ||
            question.StartsWith("how long", StringComparison.InvariantCultureIgnoreCase))
            return this.random.GetRandomTimeAnswer();

        if ((question.StartsWith("how much", StringComparison.InvariantCultureIgnoreCase) ||
             question.StartsWith("how many", StringComparison.InvariantCultureIgnoreCase)))
            return this.random.GetRandomQuantityAnswer();

        if (question.StartsWith("who", StringComparison.InvariantCultureIgnoreCase) && this.Context.Guild is not null) {
            var rng = new SecureRandom();
            IReadOnlyCollection<SocketGuildUser> users = this.Context.Guild.Users
                .Where(user => !user.IsBot)
                .ToList();
            if (users.Any())
                return TranslationKey.wrap_1(rng.ChooseRandomElement(users).Mention);
        }

        return this.random.GetRandomYesNoAnswer();
    }
}
