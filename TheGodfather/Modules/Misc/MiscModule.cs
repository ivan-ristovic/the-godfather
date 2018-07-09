#region USING_DIRECTIVES
using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TheGodfather.Common;
using TheGodfather.Common.Attributes;
using TheGodfather.Exceptions;
using TheGodfather.Extensions;
using TheGodfather.Modules.Misc.Common;
using TheGodfather.Services;
using TheGodfather.Services.Common;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;

using Humanizer;
#endregion

namespace TheGodfather.Modules.Misc
{
    [Cooldown(3, 5, CooldownBucketType.Channel)]
    [NotBlocked]
    public class MiscModule : TheGodfatherBaseModule
    {

        public MiscModule(SharedData shared, DBService db) : base(shared, db) { }


        #region COMMAND_8BALL
        [Command("8ball"), Module(ModuleType.Miscellaneous)]
        [Description("An almighty ball which knows the answer to any question you ask. Alright, it's random answer, so what?")]
        [Aliases("8b")]
        [UsageExamples("!8ball Am I gay?")]
        public async Task EightBallAsync(CommandContext ctx,
                                        [RemainingText, Description("A question for the almighty ball.")] string question)
        {
            if (string.IsNullOrWhiteSpace(question))
                throw new InvalidCommandUsageException("The almighty ball requires a question.");

            await ctx.InformSuccessAsync($"{ctx.User.Mention}: {EightBall.GenerateRandomAnswer}", ":8ball:")
                .ConfigureAwait(false);
        }
        #endregion

        #region COMMAND_COINFLIP
        [Command("coinflip"), Module(ModuleType.Miscellaneous)]
        [Description("Flip a coin.")]
        [Aliases("coin", "flip")]
        [UsageExamples("!coinflip")]
        public async Task CoinflipAsync(CommandContext ctx)
        {
            if (GFRandom.Generator.GetBool())
                await ctx.InformSuccessAsync($"{ctx.User.Mention} flipped {Formatter.Bold("Heads")}", ":full_moon_with_face:")
                    .ConfigureAwait(false);
            else
                await ctx.InformSuccessAsync($"{ctx.User.Mention} flipped {Formatter.Bold("Tails")}", ":new_moon_with_face:")
                    .ConfigureAwait(false);
        }
        #endregion

        #region COMMAND_DICE
        [Command("dice"), Module(ModuleType.Miscellaneous)]
        [Description("Roll a dice.")]
        [Aliases("die", "roll")]
        [UsageExamples("!dice")]
        public async Task DiceAsync(CommandContext ctx)
        {
            await ctx.InformSuccessAsync($"{ctx.User.Mention} rolled a {Formatter.Bold(GFRandom.Generator.Next(1, 7).ToString())}", ":game_die:")
                .ConfigureAwait(false);
        }
        #endregion

        #region COMMAND_GIVEME
        [Command("giveme"), Module(ModuleType.Miscellaneous)]
        [Description("Grants you a role from this guild's self-assignable roles list.")]
        [Aliases("giverole", "gimme", "grantme")]
        [UsageExamples("!giveme @Announcements")]
        [RequireBotPermissions(Permissions.ManageRoles)]
        public async Task GiveRoleAsync(CommandContext ctx,
                                       [Description("Role to grant.")] DiscordRole role)
        {
            if (!await Database.SelfAssignableRoleExistsForGuildAsync(ctx.Guild.Id, role.Id))
                throw new CommandFailedException("That role is not in this guild's self-assignable roles list.");

            await ctx.Member.GrantRoleAsync(role, ctx.BuildReasonString("Granted self-assignable role."))
                .ConfigureAwait(false);
            await ctx.InformSuccessAsync()
                .ConfigureAwait(false);
        }
        #endregion

        #region COMMAND_INVITE
        [Command("invite"), Module(ModuleType.Miscellaneous)]
        [Description("Get an instant invite link for the current guild.")]
        [Aliases("getinvite")]
        [UsageExamples("!invite")]
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
                var invite = await ctx.Channel.CreateInviteAsync(max_age: 3600, temporary: true, reason: ctx.BuildReasonString())
                    .ConfigureAwait(false);
                await ctx.RespondAsync($"{invite} {Formatter.Italic("(This invite will expire in one hour!)")}")
                    .ConfigureAwait(false);
            }
        }
        #endregion

        #region COMMAND_ITEMS
        [Command("items"), Module(ModuleType.Miscellaneous)]
        [Description("View user's purchased items (see ``bank`` and ``shop``).")]
        [Aliases("myitems", "purchases")]
        [UsageExamples("!items",
                       "!items @Someone")]
        [RequirePermissions(Permissions.CreateInstantInvite)]
        public async Task GetPurchasedItemsAsync(CommandContext ctx,
                                                [Description("User.")] DiscordUser user = null)
        {
            if (user == null)
                user = ctx.User;

            var items = await Database.GetPurchasedItemsForUserAsync(user.Id)
                .ConfigureAwait(false);

            if (!items.Any())
                throw new CommandFailedException("No items purchased!");

            await ctx.SendCollectionInPagesAsync(
                $"{user.Username}'s purchased items:",
                items,
                item => $"{Formatter.Bold(item.Name)} | {item.Price}",
                DiscordColor.Azure,
                5
            ).ConfigureAwait(false);
        }
        #endregion

        #region COMMAND_LEAVE
        [Command("leave"), Module(ModuleType.Miscellaneous)]
        [Description("Makes Godfather leave the guild.")]
        [UsageExamples("!leave")]
        [RequireUserPermissions(Permissions.Administrator)]
        [UsesInteractivity]
        public async Task LeaveAsync(CommandContext ctx)
        {
            if (await ctx.WaitForBoolReplyAsync("Are you sure you want me to leave this guild?").ConfigureAwait(false)) {
                await ctx.InformSuccessAsync("Go find a new bot, since this one is leaving!", ":wave:")
                    .ConfigureAwait(false);
                await ctx.Guild.LeaveAsync()
                    .ConfigureAwait(false);
            } else {
                await ctx.InformSuccessAsync("Guess I'll stay then.", ":no_mouth:")
                    .ConfigureAwait(false);
            }
        }
        #endregion

        #region COMMAND_LEET
        [Command("leet"), Module(ModuleType.Miscellaneous)]
        [Description("Wr1t3s m3ss@g3 1n 1337sp34k.")]
        [Aliases("l33t")]
        [UsageExamples("!leet Some sentence")]
        public async Task L33tAsync(CommandContext ctx,
                                   [RemainingText, Description("Text.")] string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                throw new InvalidCommandUsageException("Y0u d1dn'7 g1v3 m3 @ny 73x7...");

            var sb = new StringBuilder();
            foreach (char c in text) {
                char add;
                bool r = GFRandom.Generator.GetBool();
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
                sb.Append(GFRandom.Generator.GetBool() ? Char.ToUpperInvariant(add) : Char.ToLowerInvariant(add));
            }

            await ctx.RespondAsync(sb.ToString())
                .ConfigureAwait(false);
        }
        #endregion

        #region COMMAND_NEWS
        [Command("news"), Module(ModuleType.Miscellaneous)]
        [Description("Get newest world news.")]
        [Aliases("worldnews")]
        [UsageExamples("!news")]
        public async Task NewsRssAsync(CommandContext ctx)
        {
            var res = RSSService.GetFeedResults("https://news.google.com/news/rss/headlines/section/topic/WORLD?ned=us&hl=en");
            if (res == null)
                throw new CommandFailedException("Error getting world news.");
            await RSSService.SendFeedResultsAsync(ctx.Channel, res)
                .ConfigureAwait(false);
        }
        #endregion

        #region COMMAND_PENIS
        [Command("penis"), Module(ModuleType.Miscellaneous)]
        [Description("An accurate measurement.")]
        [Aliases("size", "length", "manhood", "dick")]
        [UsageExamples("!penis @Someone")]
        public async Task PenisAsync(CommandContext ctx,
                                    [Description("Who to measure.")] DiscordUser user = null)
        {
            if (user == null)
                user = ctx.User;

            if (user.Id == ctx.Client.CurrentUser.Id) {
                await ctx.InformSuccessAsync($"{user.Mention}'s size:\n\n{Formatter.Bold("8===============================================")}\n{Formatter.Italic("(Please plug in a second monitor for the entire display)")}", ":straight_ruler:")
                    .ConfigureAwait(false);
                return;
            }

            var sb = new StringBuilder();
            sb.Append('8').Append('=', (int)(user.Id % 40)).Append('D');
            await ctx.InformSuccessAsync(StaticDiscordEmoji.Ruler, $"{user.Mention}'s size:\n\n{Formatter.Bold(sb.ToString())}")
                .ConfigureAwait(false);
        }
        #endregion

        #region COMMAND_PENISCOMPARE
        [Command("peniscompare"), Module(ModuleType.Miscellaneous)]
        [Description("Comparison of the results given by ``penis`` command.")]
        [Aliases("sizecompare", "comparesize", "comparepenis", "cmppenis", "peniscmp", "comppenis")]
        [UsageExamples("!peniscompare @Someone",
                       "!peniscompare @Someone @SomeoneElse")]
        public async Task PenisCompareAsync(CommandContext ctx,
                                           [Description("User1.")] DiscordUser user1,
                                           [Description("User2 (def. sender).")] DiscordUser user2 = null)
        {
            if (user2 == null)
                user2 = ctx.User;

            if (user1.Id == ctx.Client.CurrentUser.Id || user2.Id == ctx.Client.CurrentUser.Id) {
                await ctx.InformSuccessAsync("Please, I do not want to make everyone laugh at you...", ":straight_ruler:")
                    .ConfigureAwait(false);
                return;
            }

            var sb = new StringBuilder();
            sb.Append('8').Append('=', (int)(user1.Id % 40)).Append("D ").AppendLine(user1.Mention);
            sb.Append('8').Append('=', (int)(user2.Id % 40)).Append("D ").AppendLine(user2.Mention);
            await ctx.InformSuccessAsync($"Comparing...\n\n{Formatter.Bold(sb.ToString())}", ":straight_ruler:")
                .ConfigureAwait(false);
        }
        #endregion

        #region COMMAND_PING
        [Command("ping"), Module(ModuleType.Miscellaneous)]
        [Description("Ping the bot.")]
        [UsageExamples("!ping")]
        public async Task PingAsync(CommandContext ctx)
        {
            await ctx.InformSuccessAsync($"Pong! {ctx.Client.Ping}ms", ":heartbeat:")
                .ConfigureAwait(false);
        }
        #endregion

        #region COMMAND_PREFIX
        [Command("prefix"), Module(ModuleType.Miscellaneous)]
        [Description("Get current guild prefix, or change it.")]
        [Aliases("setprefix", "pref", "setpref")]
        [UsageExamples("!prefix",
                       "!prefix ;")]
        [RequireUserPermissions(Permissions.Administrator)]
        public async Task GetOrSetPrefixAsync(CommandContext ctx,
                                             [Description("Prefix to set.")] string prefix = null)
        {
            if (string.IsNullOrWhiteSpace(prefix)) {
                string p = Shared.GetGuildPrefix(ctx.Guild.Id);
                await ctx.InformSuccessAsync($"Current prefix for this guild: {Formatter.Bold(p)}", ":information_source:")
                    .ConfigureAwait(false);
                return;
            }

            if (prefix.Length > 12)
                throw new CommandFailedException("Prefix length cannot be longer than 12 characters.");

            try {
                Shared.GuildConfigurations[ctx.Guild.Id].Prefix = prefix;
                if (prefix == Shared.BotConfiguration.DefaultPrefix) {
                    await Database.ResetPrefixAsync(ctx.Guild.Id)
                        .ConfigureAwait(false);
                } else {
                    await Database.SetPrefixAsync(ctx.Guild.Id, prefix)
                        .ConfigureAwait(false);
                }
            } catch (Exception e) {
                Shared.LogProvider.LogException(LogLevel.Warning, e);
                throw new CommandFailedException("Failed to add prefix. Please try again.");
            }

            await ctx.InformSuccessAsync($"Successfully changed the prefix for this guild to: {Formatter.Bold(prefix)}")
                .ConfigureAwait(false);
        }
        #endregion

        #region COMMAND_QUOTEOFTHEDAY
        [Command("quoteoftheday"), Module(ModuleType.Miscellaneous)]
        [Description("Get quote of the day. You can also specify a category from the list: inspire, management, sports, life, funny, love, art, students.")]
        [Aliases("qotd", "qod", "quote", "q")]
        [UsageExamples("!quoteoftheday",
                       "!quoteoftheday life")]
        public async Task QotdAsync(CommandContext ctx,
                                   [Description("Category.")] string category = null)
        {
            var quote = await QuoteService.GetQuoteOfTheDayAsync(category)
                .ConfigureAwait(false);
            if (quote == null)
                throw new CommandFailedException("Failed to retrieve quote! Possibly the given quote category does not exist.");
            
            await ctx.RespondAsync(embed: quote.Embed($"Quote of the day{(string.IsNullOrWhiteSpace(category) ? "" : $" in category {category}")}:"))
                .ConfigureAwait(false);
        }
        #endregion

        #region COMMAND_RATE
        [Command("rate"), Module(ModuleType.Miscellaneous)]
        [Description("Gives a rating chart for the user. If the user is not provided, rates sender.")]
        [Aliases("score", "graph")]
        [UsageExamples("!rate @Someone")]
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
                        start_x = (int)(user.Id % (ulong)(chart.Width - 143)) + 110;
                        start_y = (int)(user.Id % (ulong)(chart.Height - 55)) + 15;
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

        #region COMMAND_REMIND
        [Command("remind"), Priority(2)]
        [Module(ModuleType.Miscellaneous)]
        [Description("Resend a message after some time.")]
        [UsageExamples("!remind 1h Drink water!")]
        [RequireUserPermissions(Permissions.Administrator)]
        public async Task RemindAsync(CommandContext ctx,
                                     [Description("Time span until reminder.")] TimeSpan timespan,
                                     [Description("Channel to send message to.")] DiscordChannel channel,
                                     [RemainingText, Description("What to send?")] string message)
        {
            if (string.IsNullOrWhiteSpace(message))
                throw new InvalidCommandUsageException("Missing time or repeat string.");

            if (message.Length > 120)
                throw new InvalidCommandUsageException("Message must be shorter than 120 characters.");

            if (channel == null)
                channel = ctx.Channel;

            if (timespan.TotalMinutes < 1 || timespan.TotalDays > 7)
                throw new InvalidCommandUsageException("Time span cannot be less than 1 minute or greater than 1 week.");

            DateTime when = DateTime.UtcNow + timespan;

            var task = new SavedTask() {
                ChannelId = ctx.Channel.Id,
                Comment = message,
                ExecutionTime = when,
                GuildId = ctx.Guild.Id,
                Type = SavedTaskType.SendMessage,
                UserId = ctx.User.Id
            };
            if (!await SavedTaskExecuter.TryScheduleAsync(ctx, task).ConfigureAwait(false))
                throw new DatabaseOperationException("Failed to set a reminder in the database!");

            await ctx.InformSuccessAsync($"I will remind {channel.Mention} in {Formatter.Bold(timespan.Humanize(5))} (at {when.ToUniversalTime().ToString()} UTC) to:\n\n{Formatter.Italic(message)}", ":alarm_clock:")
                .ConfigureAwait(false);
        }

        [Command("remind"), Priority(1)]
        public Task RemindAsync(CommandContext ctx,
                               [Description("Channel to send message to.")] DiscordChannel channel,
                               [Description("Time span until reminder.")] TimeSpan timespan,
                               [RemainingText, Description("What to send?")] string message)
            => RemindAsync(ctx, timespan, channel, message);

        [Command("remind"), Priority(0)]
        public Task RemindAsync(CommandContext ctx,
                               [Description("Time span until reminder.")] TimeSpan timespan,
                               [RemainingText, Description("What to send?")] string message)
            => RemindAsync(ctx, timespan, null, message);
        #endregion

        #region COMMAND_REPORT
        [Command("report"), Module(ModuleType.Miscellaneous)]
        [Description("Send a report message to owner about a bug (please don't abuse... please).")]
        [UsageExamples("!report Your bot sucks!")]
        [UsesInteractivity]
        public async Task SendErrorReportAsync(CommandContext ctx,
                                              [RemainingText, Description("Issue text.")] string issue)
        {
            if (string.IsNullOrWhiteSpace(issue))
                throw new InvalidCommandUsageException("Text missing.");

            if (await ctx.WaitForBoolReplyAsync("Are you okay with your user and guild info being sent for further inspection?").ConfigureAwait(false)) {
                ctx.Client.DebugLogger.LogMessage(LogLevel.Info, "TheGodfather", $"Report from {ctx.User.Username} ({ctx.User.Id}): {issue}", DateTime.Now);
                var dm = await ctx.Client.CreateDmChannelAsync(ctx.Client.CurrentApplication.Owner.Id)
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
                await ctx.InformSuccessAsync("Your issue has been reported.")
                    .ConfigureAwait(false);
            }
        }
        #endregion

        #region COMMAND_SAY
        [Command("say"), Module(ModuleType.Miscellaneous)]
        [Description("Echo echo echo.")]
        [Aliases("repeat")]
        [UsageExamples("!say I am gay.")]
        public async Task SayAsync(CommandContext ctx,
                                  [RemainingText, Description("Text.")] string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                throw new InvalidCommandUsageException("Text missing.");

            if (Shared.MessageContainsFilter(ctx.Guild.Id, text))
                throw new CommandFailedException("You can't make me say something that contains filtered content for this guild.");

            await ctx.RespondAsync(text)
                .ConfigureAwait(false);
        }
        #endregion

        #region COMMAND_TTS
        [Command("tts"), Module(ModuleType.Miscellaneous)]
        [Description("Sends a tts message.")]
        [UsageExamples("!tts I am gay.")]
        public async Task TTSAsync(CommandContext ctx,
                                  [RemainingText, Description("Text.")] string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                throw new InvalidCommandUsageException("Text missing.");

            if (Shared.MessageContainsFilter(ctx.Guild.Id, text))
                throw new CommandFailedException("You can't make me say something that contains filtered content for this guild.");

            await ctx.RespondAsync(text, isTTS: true)
                .ConfigureAwait(false);
        }
        #endregion

        #region COMMAND_ZUGIFY
        [Command("zugify"), Module(ModuleType.Miscellaneous)]
        [Description("I don't even...")]
        [Aliases("z")]
        [UsageExamples("!zugify Some random text")]
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
                    sb.Append(StaticDiscordEmoji.Question);
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
