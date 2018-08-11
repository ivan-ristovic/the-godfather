#region USING_DIRECTIVES
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;

using Humanizer;

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TheGodfather.Common;
using TheGodfather.Common.Attributes;
using TheGodfather.Exceptions;
using TheGodfather.Extensions;
using TheGodfather.Modules.Administration.Extensions;
using TheGodfather.Modules.Misc.Common;
using TheGodfather.Modules.Misc.Extensions;
using TheGodfather.Services;
#endregion

namespace TheGodfather.Modules.Misc
{
    [Cooldown(3, 5, CooldownBucketType.Channel), NotBlocked]
    public class MiscModule : TheGodfatherModule
    {

        public MiscModule(SharedData shared, DBService db) 
            : base(shared, db)
        {
            this.ModuleColor = DiscordColor.LightGray;
        }


        #region COMMAND_8BALL
        [Command("8ball")]
        [Description("An almighty ball which knows the answer to any question you ask. Alright, the answer is random, so what?")]
        [Aliases("8b")]
        [UsageExamples("!8ball Am I gay?")]
        public Task EightBallAsync(CommandContext ctx,
                                  [RemainingText, Description("A question for the almighty ball.")] string question)
        {
            if (string.IsNullOrWhiteSpace(question))
                throw new InvalidCommandUsageException("The almighty ball requires a question.");

            return InformAsync(ctx, $"{ctx.User.Mention}: {EightBall.GenerateRandomAnswer}", ":8ball:");
        }
        #endregion

        #region COMMAND_COINFLIP
        [Command("coinflip")]
        [Description("Flip a coin.")]
        [Aliases("coin", "flip")]
        [UsageExamples("!coinflip")]
        public Task CoinflipAsync(CommandContext ctx)
            => InformAsync(ctx, $"{ctx.User.Mention} flipped {Formatter.Bold(GFRandom.Generator.GetBool() ? "Heads" : "Tails")}", ":full_moon_with_face:");
        #endregion

        #region COMMAND_DICE
        [Command("dice")]
        [Description("Roll a dice.")]
        [Aliases("die", "roll")]
        [UsageExamples("!dice")]
        public Task DiceAsync(CommandContext ctx)
            => InformAsync(ctx, StaticDiscordEmoji.Dice, $"{ctx.User.Mention} rolled a {Formatter.Bold(GFRandom.Generator.Next(1, 7).ToString())}");
        #endregion

        #region COMMAND_GIVEME
        [Command("giveme")]
        [Description("Grants you a role from this guild's self-assignable roles list.")]
        [Aliases("giverole", "gimme", "grantme")]
        [UsageExamples("!giveme @Announcements")]
        [RequireBotPermissions(Permissions.ManageRoles)]
        public async Task GiveRoleAsync(CommandContext ctx,
                                       [Description("Role to grant.")] DiscordRole role)
        {
            if (!await this.Database.IsSelfAssignableRoleAsync(ctx.Guild.Id, role.Id))
                throw new CommandFailedException("That role is not in this guild's self-assignable roles list.");

            await ctx.Member.GrantRoleAsync(role, ctx.BuildReasonString("Granted self-assignable role."));
            await InformAsync(ctx, "Successfully granted the required roles.", important: false);
        }
        #endregion

        #region COMMAND_INVITE
        [Command("invite")]
        [Description("Get an instant invite link for the current guild.")]
        [Aliases("getinvite")]
        [UsageExamples("!invite")]
        [RequirePermissions(Permissions.CreateInstantInvite)]
        public async Task GetInstantInviteAsync(CommandContext ctx)
        {
            IReadOnlyList<DiscordInvite> invites = await ctx.Guild.GetInvitesAsync();
            IEnumerable<DiscordInvite> permanent = invites.Where(inv => !inv.IsTemporary);
            if (permanent.Any()) {
                await ctx.RespondAsync(permanent.First().ToString());
            } else {
                DiscordInvite invite = await ctx.Channel.CreateInviteAsync(max_age: 3600, temporary: true, reason: ctx.BuildReasonString());
                await ctx.RespondAsync($"{invite} {Formatter.Italic("(This invite will expire in one hour!)")}");
            }
        }
        #endregion

        #region COMMAND_ITEMS
        [Command("items")]
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

            IReadOnlyList<PurchasableItem> items = await this.Database.GetPurchasedItemsAsync(user.Id);
            if (!items.Any())
                throw new CommandFailedException("No items purchased!");

            await ctx.SendCollectionInPagesAsync(
                $"Items owned by {user.Username}",
                items,
                item => $"{Formatter.Bold(item.Name)} | {item.Price}",
                this.ModuleColor,
                5
            );
        }
        #endregion

        #region COMMAND_LEAVE
        [Command("leave"), UsesInteractivity]
        [Description("Makes Godfather leave the guild.")]
        [UsageExamples("!leave")]
        [RequireOwnerOrPermissions(Permissions.Administrator)]
        public async Task LeaveAsync(CommandContext ctx)
        {
            if (await ctx.WaitForBoolReplyAsync("Are you sure you want me to leave this guild?")) {
                await InformAsync(ctx, StaticDiscordEmoji.Wave, "Go find a new bot, since this one is leaving!");
                await ctx.Guild.LeaveAsync();
            } else {
                await InformAsync(ctx, "Guess I'll stay then.", ":no_mouth:");
            }
        }
        #endregion

        #region COMMAND_LEET
        [Command("leet")]
        [Description("Wr1t3s g1v3n tEx7 1n p5EuDo 1337sp34k.")]
        [Aliases("l33t")]
        [UsageExamples("!leet Some sentence")]
        public Task L33tAsync(CommandContext ctx,
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

            return InformAsync(ctx, StaticDiscordEmoji.Information, sb.ToString());
        }
        #endregion

        #region COMMAND_PENIS
        [Command("penis")]
        [Description("An accurate measurement.")]
        [Aliases("size", "length", "manhood", "dick")]
        [UsageExamples("!penis @Someone")]
        public Task PenisAsync(CommandContext ctx,
                              [Description("Who to measure.")] DiscordUser user = null)
        {
            if (user == null)
                user = ctx.User;

            var sb = new StringBuilder($"{user.Mention}'s size:").AppendLine().AppendLine();

            if (user.IsCurrent) {
                sb.AppendLine(Formatter.Bold($"8{new string('=', 45)}"));
                sb.Append(Formatter.Italic("(Please plug in a second monitor for the entire display)"));
                return InformAsync(ctx, StaticDiscordEmoji.Ruler, sb.ToString());
            }

            sb.Append(Formatter.Bold($"8{new string('=', (int)(user.Id % 40))}D"));

            return InformAsync(ctx, StaticDiscordEmoji.Ruler, sb.ToString());
        }
        #endregion

        #region COMMAND_PENISCOMPARE
        [Command("peniscompare")]
        [Description("Comparison of the results given by ``penis`` command.")]
        [Aliases("sizecompare", "comparesize", "comparepenis", "cmppenis", "peniscmp", "comppenis")]
        [UsageExamples("!peniscompare @Someone",
                       "!peniscompare @Someone @SomeoneElse")]
        public Task PenisCompareAsync(CommandContext ctx,
                                     [Description("User1.")] DiscordUser user1,
                                     [Description("User2 (def. sender).")] DiscordUser user2 = null)
        {
            if (user2 == null)
                user2 = ctx.User;

            if (user1.Id == ctx.Client.CurrentUser.Id || user2.Id == ctx.Client.CurrentUser.Id)
                return InformAsync(ctx, StaticDiscordEmoji.Ruler, "Please, I do not want to make everyone laugh at you...");

            var sb = new StringBuilder();
            sb.Append('8').Append('=', (int)(user1.Id % 40)).Append("D ").AppendLine(user1.Mention);
            sb.Append('8').Append('=', (int)(user2.Id % 40)).Append("D ").AppendLine(user2.Mention);

            return InformAsync(ctx, StaticDiscordEmoji.Ruler, $"Comparing...\n\n{Formatter.Bold(sb.ToString())}");
        }
        #endregion

        #region COMMAND_PING
        [Command("ping")]
        [Description("Ping the bot.")]
        [UsageExamples("!ping")]
        public Task PingAsync(CommandContext ctx)
            => InformAsync(ctx, $"Pong! {ctx.Client.Ping}ms", ":heartbeat:");
        #endregion

        #region COMMAND_PREFIX
        [Command("prefix")]
        [Description("Get current guild prefix, or change it.")]
        [Aliases("setprefix", "pref", "setpref")]
        [UsageExamples("!prefix",
                       "!prefix ;")]
        [RequireOwnerOrPermissions(Permissions.Administrator)]
        public async Task GetOrSetPrefixAsync(CommandContext ctx,
                                             [Description("Prefix to set.")] string prefix = null)
        {
            if (string.IsNullOrWhiteSpace(prefix)) {
                string p = this.Shared.GetGuildPrefix(ctx.Guild.Id);
                await InformAsync(ctx, StaticDiscordEmoji.Information, $"Current prefix for this guild: {Formatter.Bold(p)}");
                return;
            }

            if (prefix.Length > 12)
                throw new CommandFailedException("Prefix cannot be longer than 12 characters.");
            
            if (prefix == this.Shared.BotConfiguration.DefaultPrefix)
                this.Shared.GetGuildConfig(ctx.Guild.Id).Prefix = null;
            else
                this.Shared.GetGuildConfig(ctx.Guild.Id).Prefix = prefix;

            await this.Database.UpdateGuildSettingsAsync(ctx.Guild.Id, this.Shared.GetGuildConfig(ctx.Guild.Id));

            await InformAsync(ctx, $"Successfully changed the prefix for this guild to: {Formatter.Bold(prefix)}", important: false);
        }
        #endregion

        #region COMMAND_RATE
        [Command("rate")]
        [Description("Gives a rating chart for the user. If the user is not provided, rates sender.")]
        [Aliases("score", "graph", "rating")]
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
                            Color = this.ModuleColor
                        });
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
        [Description("Resend a message after some time.")]
        [UsageExamples("!remind 1h Drink water!")]
        [RequireOwnerOrPermissions(Permissions.Administrator)]
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

            if (timespan.TotalMinutes < 1 || timespan.TotalDays > 31)
                throw new InvalidCommandUsageException("Time span cannot be less than 1 minute or greater than 31 days.");

            DateTimeOffset when = DateTimeOffset.Now + timespan;

            var task = new SendMessageTaskInfo(ctx.Channel.Id, ctx.User.Id, message, when);
            if (!await SavedTaskExecutor.TryScheduleAsync(this.Shared, this.Database, ctx.Client, task))
                throw new CommandFailedException("Failed to schedule saved task!");

            await InformAsync(ctx, StaticDiscordEmoji.AlarmClock, $"I will remind {channel.Mention} in {Formatter.Bold(timespan.Humanize(5))} ({when.ToUtcTimestamp()}) to:\n\n{Formatter.Italic(message)}", important: false);
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
        [Command("report")]
        [Description("Send a report message to owner about a bug (please don't abuse... please).")]
        [UsageExamples("!report Your bot sucks!")]
        [UsesInteractivity]
        public async Task SendErrorReportAsync(CommandContext ctx,
                                              [RemainingText, Description("Issue text.")] string issue)
        {
            if (string.IsNullOrWhiteSpace(issue))
                throw new InvalidCommandUsageException("Text missing.");

            if (await ctx.WaitForBoolReplyAsync("Are you okay with your user and guild info being sent for further inspection?")) {
                ctx.Client.DebugLogger.LogMessage(LogLevel.Info, "TheGodfather", $"Report from {ctx.User.Username} ({ctx.User.Id}): {issue}", DateTime.Now);
                DiscordDmChannel dm = await ctx.Client.CreateDmChannelAsync(ctx.Client.CurrentApplication.Owner.Id);
                if (dm == null)
                    throw new CommandFailedException("Owner has disabled DMs.");
                var emb = new DiscordEmbedBuilder() {
                    Title = "Issue",
                    Description = issue
                };
                emb.WithAuthor(ctx.User.ToString(), icon_url: ctx.User.AvatarUrl ?? ctx.User.DefaultAvatarUrl);
                emb.AddField("Guild", $"{ctx.Guild.ToString()} owned by {ctx.Guild.Owner.ToString()}");

                await dm.SendMessageAsync("A new issue has been reported!", embed: emb.Build());
                await InformAsync(ctx, "Your issue has been reported.", important: false);
            }
        }
        #endregion

        #region COMMAND_SAY
        [Command("say")]
        [Description("Echo echo echo.")]
        [Aliases("repeat")]
        [UsageExamples("!say I am gay.")]
        public Task SayAsync(CommandContext ctx,
                            [RemainingText, Description("Text to say.")] string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                throw new InvalidCommandUsageException("Text missing.");

            if (this.Shared.MessageContainsFilter(ctx.Guild.Id, text))
                throw new CommandFailedException("You can't make me say something that contains filtered content for this guild.");

            return InformAsync(ctx, Formatter.Sanitize(text), ":loudspeaker:");
        }
        #endregion

        #region COMMAND_TTS
        [Command("tts")]
        [Description("Sends a tts message.")]
        [UsageExamples("!tts I am gay.")]
        [RequirePermissions(Permissions.SendTtsMessages)]
        public Task TtsAsync(CommandContext ctx,
                            [RemainingText, Description("Text.")] string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                throw new InvalidCommandUsageException("Text missing.");

            if (this.Shared.MessageContainsFilter(ctx.Guild.Id, text))
                throw new CommandFailedException("You can't make me say something that contains filtered content for this guild.");

            return ctx.RespondAsync(Formatter.BlockCode(Formatter.Sanitize(text)), isTTS: true);
        }
        #endregion

        #region COMMAND_ZUGIFY
        [Command("zugify")]
        [Description("I don't even...")]
        [Aliases("z")]
        [UsageExamples("!zugify Some random text")]
        public Task ZugifyAsync(CommandContext ctx,
                               [RemainingText, Description("Text.")] string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                throw new InvalidCommandUsageException("Text missing.");

            text = text.ToLowerInvariant();
            var sb = new StringBuilder();
            foreach (char c in text) {
                if (Char.IsLetter(c)) {
                    sb.Append(DiscordEmoji.FromName(ctx.Client, $":regional_indicator_{c}:"));
                } else if (Char.IsDigit(c)) {
                    if (c == '0')
                        sb.Append(DiscordEmoji.FromName(ctx.Client, ":zero:"));
                    else
                        sb.Append(StaticDiscordEmoji.Numbers[c - '0' - 1]);
                } else if (Char.IsWhiteSpace(c)) {
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

            return ctx.RespondAsync(embed: new DiscordEmbedBuilder() {
                Description = sb.ToString()
            }.Build());
        }
        #endregion
    }
}
