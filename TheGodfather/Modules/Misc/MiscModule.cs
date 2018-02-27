#region USING_DIRECTIVES
using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TheGodfather.Attributes;
using TheGodfather.Exceptions;
using TheGodfather.Extensions;
using TheGodfather.Modules.Misc.Common;
using TheGodfather.Services;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
#endregion

namespace TheGodfather.Modules.Misc
{
    [Cooldown(2, 3, CooldownBucketType.User), Cooldown(5, 3, CooldownBucketType.Channel)]
    [ListeningCheck]
    public class MiscModule : TheGodfatherBaseModule
    {

        public MiscModule(SharedData shared, DatabaseService db) : base(shared, db) { }


        #region COMMAND_8BALL
        [Command("8ball")]
        [Description("An almighty ball which knows the answer to any question you ask. Alright, it's random answer, so what?")]
        [Aliases("8b")]
        [UsageExample("!8ball Am I gay?")]
        public async Task EightBallAsync(CommandContext ctx,
                                        [RemainingText, Description("A question for the almighty ball.")] string question)
        {
            if (string.IsNullOrWhiteSpace(question))
                throw new InvalidCommandUsageException("The almighty ball requires a question.");

            await ReplyWithEmbedAsync(ctx, EightBall.Answer, ":8ball:")
                .ConfigureAwait(false);
        }
        #endregion

        #region COMMAND_COINFLIP
        [Command("coinflip")]
        [Description("Flip a coin.")]
        [Aliases("coin", "flip")]
        [UsageExample("!coinflip")]
        public async Task CoinflipAsync(CommandContext ctx)
        {
            string flip = new Random().Next(2) == 0 ? "Heads" : "Tails";
            await ReplyWithEmbedAsync(ctx, $"{ctx.User.Mention} flipped {Formatter.Bold(flip)}", flip == "Heads" ? ":full_moon_with_face:" : ":new_moon_with_face:")
                .ConfigureAwait(false);
        }
        #endregion

        #region COMMAND_DICE
        [Command("dice")]
        [Description("Roll a dice.")]
        [Aliases("die", "roll")]
        [UsageExample("!dice")]
        public async Task DiceAsync(CommandContext ctx)
        {
            string roll = new Random().Next(1, 7).ToString();
            await ReplyWithEmbedAsync(ctx, $"{ctx.User.Mention} rolled a {Formatter.Bold(roll)}", ":game_die:")
                .ConfigureAwait(false);
        }
        #endregion

        #region COMMAND_GIVEME
        [Command("giveme")]
        [Description("Grants you a role from this guild's self-assignable roles list.")]
        [Aliases("giverole", "gimme", "grantme")]
        [UsageExample("!giveme @Announcements")]
        [RequireBotPermissions(Permissions.ManageRoles)]
        public async Task GiveRoleAsync(CommandContext ctx,
                                       [Description("Role to grant.")] DiscordRole role)
        {
            if (!await DatabaseService.SelfAssignableRoleExistsAsync(ctx.Guild.Id, role.Id))
                throw new CommandFailedException("That role is not in this guild's self-assignable roles list.");

            await ctx.Member.GrantRoleAsync(role, GetReasonString(ctx, "Granted self-assignable role."))
                .ConfigureAwait(false);
            await ReplyWithEmbedAsync(ctx)
                .ConfigureAwait(false);
        }
        #endregion

        #region COMMAND_INVITE
        [Command("invite")]
        [Description("Get an instant invite link for the current guild.")]
        [Aliases("getinvite")]
        [UsageExample("!invite")]
        [RequirePermissions(Permissions.CreateInstantInvite)]
        public async Task GetInstantInviteAsync(CommandContext ctx)
        {
            var invites = await ctx.Guild.GetInvitesAsync()
                .ConfigureAwait(false);

            var permanent = invites.Where(inv => !inv.IsTemporary);
            if (permanent.Any()) {
                await ctx.RespondAsync(permanent.First().ToString())
                    .ConfigureAwait(false);
            } else {
                var invite = await ctx.Channel.CreateInviteAsync(max_age: 3600, temporary: true, reason: GetReasonString(ctx))
                    .ConfigureAwait(false);
                await ctx.RespondAsync($"{invite} {Formatter.Italic("(This invite will expire in one hour!)")}")
                    .ConfigureAwait(false);
            }
        }
        #endregion

        #region COMMAND_LEAVE
        [Command("leave")]
        [Description("Makes Godfather leave the guild.")]
        [UsageExample("!leave")]
        [RequireUserPermissions(Permissions.Administrator)]
        public async Task LeaveAsync(CommandContext ctx)
        {
            if (await AskYesNoQuestionAsync(ctx, "Are you sure you want me to leave this guild?").ConfigureAwait(false)) {
                await ReplyWithEmbedAsync(ctx, "Go find a new bot, since this one is leaving!", ":wave:")
                    .ConfigureAwait(false);
                await ctx.Guild.LeaveAsync()
                    .ConfigureAwait(false);
            } else {
                await ReplyWithEmbedAsync(ctx, "Guess I'll stay then.", ":no_mouth:")
                    .ConfigureAwait(false);
            }
        }
        #endregion

        #region COMMAND_LEET
        [Command("leet")]
        [Description("Wr1t3s m3ss@g3 1n 1337sp34k.")]
        [Aliases("l33t")]
        [UsageExample("!leet Some sentence")]
        public async Task L33tAsync(CommandContext ctx,
                                   [RemainingText, Description("Text.")] string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                throw new InvalidCommandUsageException("Y0u d1dn'7 g1v3 m3 @ny 73x7...");

            var rnd = new Random();
            var sb = new StringBuilder();
            foreach (char c in text) {
                char add;
                bool r = rnd.Next() % 2 == 0;
                switch (c) {
                    case 'i': add = r ? 'i' : '1'; break;
                    case 'l': add = r ? 'l' : '1'; break;
                    case 'e': add = r ? 'e' : '3'; break;
                    case 'a': add = r ? '@' : '4'; break;
                    case 't': add = r ? 't' : '7'; break;
                    case 'o': add = r ? 'o' : '0'; break;
                    case 's': add = r ? 's' : '5'; break;
                    default: add = c; break;
                }
                sb.Append((rnd.Next() % 2 == 0) ? Char.ToUpperInvariant(add) : Char.ToLowerInvariant(add));
            }

            await ctx.RespondAsync(sb.ToString())
                .ConfigureAwait(false);
        }
        #endregion

        #region COMMAND_PENIS
        [Command("penis")]
        [Description("An accurate measurement.")]
        [Aliases("size", "length", "manhood", "dick")]
        [UsageExample("!penis @Someone")]
        public async Task PenisAsync(CommandContext ctx,
                                    [Description("Who to measure.")] DiscordUser user)
        {
            if (user.Id == ctx.Client.CurrentUser.Id) {
                await ReplyWithEmbedAsync(ctx, $"{user.Mention}'s size:\n\n{Formatter.Bold("8===============================================")}\n{Formatter.Italic("(Please plug in a second monitor for the entire display)")}", ":straight_ruler:")
                    .ConfigureAwait(false);
                return;
            }

            var sb = new StringBuilder();
            sb.Append('8').Append('=', (int)(user.Id % 40)).Append('D');
            await ReplyWithEmbedAsync(ctx, $"{user.Mention}'s size:\n\n{Formatter.Bold(sb.ToString())}", ":straight_ruler:")
                .ConfigureAwait(false);
        }
        #endregion

        #region COMMAND_PING
        [Command("ping")]
        [Description("Ping the bot.")]
        [UsageExample("!ping")]
        public async Task PingAsync(CommandContext ctx)
        {
            await ReplyWithEmbedAsync(ctx, $"Pong! {ctx.Client.Ping}ms", ":heartbeat:")
                .ConfigureAwait(false);
        }
        #endregion

        #region COMMAND_PREFIX
        [Command("prefix")]
        [Description("Get current guild prefix, or change it.")]
        [Aliases("setprefix", "pref", "setpref")]
        [UsageExample("!prefix")]
        [UsageExample("!prefix ;")]
        [RequireUserPermissions(Permissions.Administrator)]
        public async Task GetOrSetPrefixAsync(CommandContext ctx,
                                             [Description("Prefix to set.")] string prefix = null)
        {
            if (string.IsNullOrWhiteSpace(prefix)) {
                string p = SharedData.GetGuildPrefix(ctx.Guild.Id);
                await ReplyWithEmbedAsync(ctx, $"Current prefix for this guild: {Formatter.Bold(p)}", ":information_source:")
                    .ConfigureAwait(false);
                return;
            }

            if (prefix.Length > 12)
                throw new CommandFailedException("Prefix length cannot be longer than 12 characters.");

            SharedData.GuildPrefixes.AddOrUpdate(ctx.Guild.Id, prefix, (id, oldp) => prefix);
            await ReplyWithEmbedAsync(ctx, $"Successfully changed the prefix for this guild to: {Formatter.Bold(prefix)}")
                .ConfigureAwait(false);
            try {
                await DatabaseService.SetGuildPrefixAsync(ctx.Guild.Id, prefix)
                    .ConfigureAwait(false);
            } catch {
                throw new CommandFailedException("Warning: Failed to add prefix to the database.");
            }
        }
        #endregion

        #region COMMAND_RATE
        [Command("rate")]
        [Description("Gives a rating chart for the user. If the user is not provided, rates sender.")]
        [Aliases("score", "graph")]
        [UsageExample("!rate @Someone")]
        [RequireBotPermissions(Permissions.AttachFiles)]
        public async Task RateAsync(CommandContext ctx,
                                   [Description("Who to measure.")] DiscordUser user = null)
        {
            if (user == null)
                user = ctx.User;

            try {
                using (var chart = new Bitmap("Resources/graph.png"))
                using (var g = Graphics.FromImage(chart)) {
                    int start_x, start_y;
                    if (user.Id == ctx.Client.CurrentUser.Id) {
                        start_x = chart.Width - 10;
                        start_y = 0;
                    } else {
                        start_x = (int)(user.Id % (ulong)(chart.Width - 133)) + 110;
                        start_y = (int)(user.Id % (ulong)(chart.Height - 45)) + 30;
                    }
                    g.FillEllipse(Brushes.Red, start_x, start_y, 10, 10);
                    g.Flush();

                    using (var ms = new MemoryStream()) {
                        chart.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
                        ms.Position = 0;
                        await ctx.RespondWithFileAsync("Rating.jpg", ms, embed: new DiscordEmbedBuilder() {
                            Description = Formatter.Bold($"{user.Mention}'s rating"),
                            Color = DiscordColor.Cyan
                        }).ConfigureAwait(false);
                    }
                }
            } catch (FileNotFoundException e) {
                ctx.Client.DebugLogger.LogMessage(LogLevel.Error, "TheGodfather", $"graph.png load failed! Details: {e.ToString()}", DateTime.Now);
                throw new CommandFailedException("I can't find the graph image on server machine, please contact owner and tell him.");
            }
        }
        #endregion

        /* TODO remind timer
        #region COMMAND_REMIND
        [Command("remind")]
        [Description("Resend a message after some time.")]
        public async Task RemindAsync(CommandContext ctx,
                                     [Description("Time to wait before repeat (in seconds).")] int time,
                                     [RemainingText, Description("What to repeat.")] string s)
        {
             
            if (time == 0 || string.IsNullOrWhiteSpace(s))
                throw new InvalidCommandUsageException("Missing time or repeat string.");

            if (time < 0 || time > 604800)
                throw new CommandFailedException("Time cannot be less than 0 or greater than 1 week.", new ArgumentOutOfRangeException());

            await ctx.RespondAsync($"I will remind you to: \"{s}\" in {Formatter.Bold(time.ToString())} seconds.")
                .ConfigureAwait(false);
            await Task.Delay(TimeSpan.FromSeconds(time))
                .ConfigureAwait(false);
            await ctx.RespondAsync($"I was told to remind you to: \"{s}\".")
                .ConfigureAwait(false);
        }
        #endregion
        */

        #region COMMAND_REPORT
        [Command("report")]
        [Description("Send a report message to owner about a bug (please don't abuse... please).")]
        [UsageExample("!report Your bot sucks!")]
        public async Task SendErrorReportAsync(CommandContext ctx,
                                              [RemainingText, Description("Issue text.")] string issue)
        {
            if (string.IsNullOrWhiteSpace(issue))
                throw new InvalidCommandUsageException("Text missing.");

            if (await AskYesNoQuestionAsync(ctx, "Are you okay with your user and guild info being sent for further inspection?").ConfigureAwait(false)) {
                ctx.Client.DebugLogger.LogMessage(LogLevel.Info, "TheGodfather", $"Report from {ctx.User.Username} ({ctx.User.Id}): {issue}", DateTime.Now);
                var dm = await InteractivityUtil.CreateDmChannelAsync(ctx.Client, ctx.Client.CurrentApplication.Owner.Id)
                    .ConfigureAwait(false);
                if (dm == null)
                    throw new CommandFailedException("Owner has disabled DMs.");
                var emb = new DiscordEmbedBuilder() {
                    Title = "Issue",
                    Description = issue
                };
                emb.WithAuthor(ctx.User.ToString(), icon_url: ctx.User.AvatarUrl ?? ctx.User.DefaultAvatarUrl)
                   .AddField("Guild", $"{ctx.Guild.ToString()} owned by {ctx.Guild.Owner.ToString()}");

                await dm.SendMessageAsync("A new issue has been reported!", embed: emb.Build())
                    .ConfigureAwait(false);
                await ReplyWithEmbedAsync(ctx, "Your issue has been reported.")
                    .ConfigureAwait(false);
            } else {
                await ReplyWithFailedEmbedAsync(ctx, "Your issue has not been reported.")
                    .ConfigureAwait(false);
            }
        }
        #endregion

        #region COMMAND_SAY
        [Command("say")]
        [Description("Echo echo echo.")]
        [Aliases("repeat")]
        [UsageExample("!say I am gay.")]
        public async Task SayAsync(CommandContext ctx,
                                  [RemainingText, Description("Text.")] string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                throw new InvalidCommandUsageException("Text missing.");

            if (SharedData.MessageContainsFilter(ctx.Guild.Id, text))
                throw new CommandFailedException("You can't make me say something that contains filtered content for this guild.");

            await ctx.RespondAsync(text)
                .ConfigureAwait(false);
        }
        #endregion

        #region COMMAND_TTS
        [Command("tts")]
        [Description("Sends a tts message.")]
        [UsageExample("!tts I am gay.")]
        public async Task TTSAsync(CommandContext ctx,
                                  [RemainingText, Description("Text.")] string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                throw new InvalidCommandUsageException("Text missing.");

            if (SharedData.MessageContainsFilter(ctx.Guild.Id, text))
                throw new CommandFailedException("You can't make me say something that contains filtered content for this guild.");

            await ctx.RespondAsync(text, isTTS: true)
                .ConfigureAwait(false);
        }
        #endregion

        #region COMMAND_ZUGIFY
        [Command("zugify")]
        [Description("I don't even...")]
        [Aliases("z")]
        [UsageExample("!zugify Some random text")]
        public async Task ZugifyAsync(CommandContext ctx,
                                     [RemainingText, Description("Text.")] string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                throw new InvalidCommandUsageException("Text missing.");

            text = text.ToLowerInvariant();
            var sb = new StringBuilder();
            foreach (char c in text) {
                if (c >= 'a' && c <= 'z') {
                    sb.Append(DiscordEmoji.FromName(ctx.Client, $":regional_indicator_{c}:"));
                } else if (char.IsDigit(c)) {
                    switch (c) {
                        case '0': sb.Append(DiscordEmoji.FromName(ctx.Client, ":zero:")); break;
                        case '1': sb.Append(DiscordEmoji.FromName(ctx.Client, ":one:")); break;
                        case '2': sb.Append(DiscordEmoji.FromName(ctx.Client, ":two:")); break;
                        case '3': sb.Append(DiscordEmoji.FromName(ctx.Client, ":three:")); break;
                        case '4': sb.Append(DiscordEmoji.FromName(ctx.Client, ":four:")); break;
                        case '5': sb.Append(DiscordEmoji.FromName(ctx.Client, ":five:")); break;
                        case '6': sb.Append(DiscordEmoji.FromName(ctx.Client, ":six:")); break;
                        case '7': sb.Append(DiscordEmoji.FromName(ctx.Client, ":seven:")); break;
                        case '8': sb.Append(DiscordEmoji.FromName(ctx.Client, ":eight:")); break;
                        case '9': sb.Append(DiscordEmoji.FromName(ctx.Client, ":nine:")); break;
                    }
                } else if (c == ' ') {
                    sb.Append(DiscordEmoji.FromName(ctx.Client, ":large_blue_circle:"));
                } else if (c == '?')
                    sb.Append(DiscordEmoji.FromName(ctx.Client, ":question:"));
                else if (c == '!')
                    sb.Append(DiscordEmoji.FromName(ctx.Client, ":exclamation:"));
                else if (c == '.')
                    sb.Append(DiscordEmoji.FromName(ctx.Client, ":stop_button:"));
                else
                    sb.Append(c);
                sb.Append(' ');
            }

            await ctx.RespondAsync(sb.ToString())
                .ConfigureAwait(false);
        }
        #endregion
    }
}
