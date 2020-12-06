using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using TheGodfather.Attributes;
using TheGodfather.Common;
using TheGodfather.Database.Models;
using TheGodfather.Exceptions;
using TheGodfather.Extensions;
using TheGodfather.Modules.Administration.Services;
using TheGodfather.Modules.Misc.Extensions;
using TheGodfather.Modules.Misc.Services;
using TheGodfather.Services;
using TheGodfather.Services.Common;

namespace TheGodfather.Modules.Misc
{
    [Module(ModuleType.Misc)]
    [Cooldown(3, 5, CooldownBucketType.Channel), NotBlocked]
    public class MiscModule : TheGodfatherServiceModule<RandomService>
    {
        public MiscModule(RandomService service)
            : base(service) { }


        #region 8ball
        [Command("8ball")]
        [Aliases("8b")]
        public Task EightBallAsync(CommandContext ctx,
                                  [RemainingText, Description("desc-8b-question")] string question)
        {
            if (string.IsNullOrWhiteSpace(question))
                throw new InvalidCommandUsageException(ctx, "cmd-err-8b");

            return this.Service.EightBall(ctx.Channel, question, out string answer)
                ? ctx.ImpInfoAsync(this.ModuleColor, Emojis.EightBall, answer)
                : ctx.RespondAsync(embed: new DiscordEmbedBuilder {
                    Description = $"{Emojis.EightBall} {answer}",
                    Color = this.ModuleColor
                });
        }
        #endregion

        #region coinflip
        [Command("coinflip")]
        [Aliases("coin", "flip")]
        public Task CoinflipAsync(CommandContext ctx,
                                 [Description("desc-coinflip-ratio")] int ratio = 1)
            => ctx.ImpInfoAsync(this.ModuleColor, Emojis.NewMoon, this.Service.Coinflip(ratio) ? "fmt-coin-heads" : "fmt-coin-tails", ctx.User.Mention);
        #endregion

        #region dice
        [Command("dice")]
        [Aliases("die", "roll")]
        public Task DiceAsync(CommandContext ctx,
                             [Description("desc-dice-sides")] int sides = 6)
            => ctx.ImpInfoAsync(this.ModuleColor, Emojis.Dice, "fmt-dice", ctx.User.Mention, this.Service.Dice(sides));
        #endregion

        #region invite
        [Command("invite")]
        [Aliases("getinvite")]
        [RequirePermissions(Permissions.CreateInstantInvite)]
        public async Task GetInstantInviteAsync(CommandContext ctx,
                                               [Description("desc-invite-expire")] TimeSpan? expiryTime = null)
        {
            DiscordInvite? invite = null;
            if (expiryTime is null) {
                IReadOnlyList<DiscordInvite> invites = await ctx.Guild.GetInvitesAsync();
                invite = invites.Where(inv => !inv.IsTemporary).FirstOrDefault();
            } 
            
            if (invite is null || expiryTime is { }) {
                expiryTime ??= TimeSpan.FromSeconds(86400);

                // TODO check timespan because only some values are allowed - 400 is thrown
                invite = await ctx.Channel.CreateInviteAsync(max_age: (int)expiryTime.Value.TotalSeconds, temporary: true, reason: ctx.BuildInvocationDetailsString());
            }

            await ctx.RespondAsync(invite.ToString());
        }
        #endregion

        #region leave
        [Command("leave"), UsesInteractivity]
        [RequireOwnerOrPermissions(Permissions.Administrator)]
        public async Task LeaveAsync(CommandContext ctx)
        {
            if (await ctx.WaitForBoolReplyAsync("q-leave"))
                await ctx.Guild.LeaveAsync();
        }
        #endregion

        #region leet
        [Command("leet")]
        [Aliases("l33t", "1337")]
        public Task L33tAsync(CommandContext ctx,
                             [RemainingText, Description("Text.")] string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                throw new InvalidCommandUsageException(ctx, "cmd-err-leet-none");

            string leet = this.Service.ToLeet(text);
            return ctx.RespondAsync(embed: new DiscordEmbedBuilder {
                Color = this.ModuleColor,
                Description = $"{Emojis.Information} {leet}",
            });
        }
        #endregion

        #region COMMAND_NSFW
        [Command("nsfw")]
        [Description("Wraps the URL into a special NSFW block.")]

        [RequireBotPermissions(Permissions.ManageMessages)]
        public async Task NsfwAsync(CommandContext ctx,
                                   [Description("URL to wrap.")] Uri url,
                                   [RemainingText, Description("Additional info")] string info = null)
        {
            await ctx.Message.DeleteAsync();

            var emb = new DiscordEmbedBuilder {
                Title = $"{Emojis.NoEntry} NSFW link from {ctx.Member.DisplayName} {Emojis.NoEntry}",
                Description = FormatterExt.Spoiler(url.ToString()),
                Color = DiscordColor.Red
            };
            if (!string.IsNullOrWhiteSpace(info))
                emb.AddField("Additional info", Formatter.BlockCode(Formatter.Strip(info)));

            await ctx.RespondAsync(embed: emb.Build());
        }
        #endregion

        #region COMMAND_PENIS
        [Command("penis"), Priority(1)]
        [Description("An accurate measurement.")]
        [Aliases("size", "length", "manhood", "dick", "dicksize")]

        public Task PenisAsync(CommandContext ctx,
                              [Description("Who to measure.")] DiscordMember member = null)
            => this.PenisAsync(ctx, member as DiscordUser);

        [Command("penis"), Priority(0)]
        public Task PenisAsync(CommandContext ctx,
                              [Description("Who to measure.")] DiscordUser user = null)
        {
            user = user ?? ctx.User;

            StringBuilder sb = new StringBuilder($"{user.Mention}'s size:").AppendLine().AppendLine();

            if (user.IsCurrent) {
                sb.AppendLine(Formatter.Bold($"8{new string('=', 45)}"));
                sb.Append(Formatter.Italic("(Please plug in a second monitor for the entire display)"));
                return this.InformAsync(ctx, Emojis.Ruler, sb.ToString());
            }

            sb.Append(Formatter.Bold($"8{new string('=', (int)(user.Id % 40))}D"));

            return this.InformAsync(ctx, Emojis.Ruler, sb.ToString());
        }
        #endregion

        #region COMMAND_PENISCOMPARE
        [Command("peniscompare"), Priority(1)]
        [Description("Comparison of the results given by ``penis`` command.")]
        [Aliases("sizecompare", "comparesize", "comparepenis", "cmppenis", "peniscmp", "comppenis")]

        public Task PenisCompareAsync(CommandContext ctx,
                                     [Description("User1.")] params DiscordMember[] members)
            => this.PenisCompareAsync(ctx, members.Select(u => u as DiscordUser).ToArray());

        [Command("peniscompare"), Priority(0)]
        public Task PenisCompareAsync(CommandContext ctx,
                                     [Description("User1.")] params DiscordUser[] users)
        {
            if (users is null || users.Length < 2 || users.Length >= 10)
                throw new InvalidCommandUsageException("You must provide atleast two and less than 10 users to compare.");

            var sb = new StringBuilder();
            foreach (DiscordUser u in users.Distinct()) {
                if (u.IsCurrent)
                    return this.InformAsync(ctx, Emojis.Ruler, "Please, I do not want to make everyone laugh at you...");
                sb.Append('8').Append('=', (int)(u.Id % 40)).Append("D ").AppendLine(u.Mention);
            }

            return this.InformAsync(ctx, Emojis.Ruler, $"Comparing...\n\n{Formatter.Bold(sb.ToString())}");
        }
        #endregion

        #region COMMAND_PING
        [Command("ping")]
        [Description("Ping the bot.")]
        public Task PingAsync(CommandContext ctx)
            => this.InformAsync(ctx, $"Pong! {ctx.Client.Ping}ms", ":heartbeat:");
        #endregion

        #region COMMAND_PREFIX
        [Command("prefix")]
        [Description("Get current guild prefix, or change it.")]
        [Aliases("setprefix", "pref", "setpref")]

        [RequireOwnerOrPermissions(Permissions.Administrator)]
        public async Task GetOrSetPrefixAsync(CommandContext ctx,
                                             [Description("Prefix to set.")] string prefix = null)
        {
            if (string.IsNullOrWhiteSpace(prefix)) {
                string p = ctx.Services.GetService<GuildConfigService>().GetGuildPrefix(ctx.Guild.Id);
                await this.InformAsync(ctx, Emojis.Information, $"Current prefix for this guild: {Formatter.Bold(p)}");
                return;
            }

            if (prefix.Length > 8)
                throw new CommandFailedException("Prefix cannot be longer than 8 characters.");

            GuildConfig gcfg = await ctx.Services.GetService<GuildConfigService>().ModifyConfigAsync(ctx.Guild.Id, cfg => {
                cfg.Prefix = (prefix == ctx.Services.GetService<BotConfigService>().CurrentConfiguration.Prefix) ? null : prefix;
            });

            await this.InformAsync(ctx, $"Successfully changed the prefix for this guild to: {Formatter.Bold(gcfg.Prefix ?? ctx.Services.GetService<BotConfigService>().CurrentConfiguration.Prefix)}", important: false);
        }
        #endregion

        #region COMMAND_RATE
        [Command("rate"), Priority(1)]
        [Description("Gives a rating chart for the user. If the user is not provided, rates sender.")]
        [Aliases("score", "graph", "rating")]

        [RequireBotPermissions(Permissions.AttachFiles)]
        public Task RateAsync(CommandContext ctx,
                             [Description("Who to measure.")] params DiscordMember[] members)
            => this.RateAsync(ctx, members.Select(u => u as DiscordUser).ToArray());

        [Command("rate"), Priority(1)]
        public async Task RateAsync(CommandContext ctx,
                                   [Description("Who to measure.")] params DiscordUser[] users)
        {
            users = users?.Distinct().ToArray() ?? null;
            if (users is null || !users.Any() || users.Length > 8)
                throw new InvalidCommandUsageException("You must provide atleast 1 and at most 8 users to rate.");

            try {
                using var chart = new Bitmap("Resources/graph.png");
                using var g = Graphics.FromImage(chart);
                Brush[] colors = new[] { Brushes.Red, Brushes.Green, Brushes.Blue, Brushes.Orange, Brushes.Pink, Brushes.Purple, Brushes.Gold, Brushes.Cyan };

                int position = 0;
                foreach (DiscordUser user in users)
                    DrawUserRating(g, user, position++);

                using (var ms = new MemoryStream()) {
                    chart.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
                    ms.Position = 0;
                    await ctx.RespondWithFileAsync("Rating.jpg", ms, embed: new DiscordEmbedBuilder {
                        Description = Formatter.Bold($"Rating for: {string.Join(", ", users.Select(u => u.Mention))}"),
                        Color = this.ModuleColor
                    });
                }

                void DrawUserRating(Graphics graphics, DiscordUser user, int pos)
                {
                    int start_x, start_y;
                    if (user.Id == ctx.Client.CurrentUser.Id) {
                        start_x = chart.Width - 10;
                        start_y = 0;
                    } else {
                        start_x = (int)(user.Id % (ulong)(chart.Width - 280)) + 110;
                        start_y = (int)(user.Id % (ulong)(chart.Height - 55)) + 15;
                    }
                    graphics.FillEllipse(colors[pos], start_x, start_y, 10, 10);
                    graphics.DrawString(user.Username, new Font("Arial", 13), colors[pos], 750, pos * 30 + 20);
                    graphics.Flush();
                }
            } catch (FileNotFoundException e) {
                Log.Error(e, "graph.png load failed!");
                throw new CommandFailedException("I can't find the graph image on server machine, please contact owner and tell him.");
            }
        }
        #endregion

        #region COMMAND_REPORT
        [Command("report"), UsesInteractivity]
        [Description("Send a report message to owner about a bug (please don't abuse... please).")]

        public async Task SendErrorReportAsync(CommandContext ctx,
                                              [RemainingText, Description("Issue text.")] string issue)
        {
            if (string.IsNullOrWhiteSpace(issue))
                throw new InvalidCommandUsageException("Text missing.");

            if (await ctx.WaitForBoolReplyAsync("Are you okay with your user and guild info being sent for further inspection?")) {
                Log.Information($"Report from {ctx.User.Username} ({ctx.User.Id}): {issue}", DateTime.Now);
                DiscordDmChannel dm = await ctx.Client.CreateDmChannelAsync(ctx.Client.CurrentApplication.Owners.First().Id);
                if (dm is null)
                    throw new CommandFailedException("Owner has disabled DMs.");
                var emb = new DiscordEmbedBuilder {
                    Title = "Issue",
                    Description = issue
                };
                emb.WithAuthor(ctx.User.ToString(), iconUrl: ctx.User.AvatarUrl ?? ctx.User.DefaultAvatarUrl);
                emb.AddField("Guild", $"{ctx.Guild.ToString()} owned by {ctx.Guild.Owner.ToString()}");

                await dm.SendMessageAsync("A new issue has been reported!", embed: emb.Build());
                await this.InformAsync(ctx, "Your issue has been reported.", important: false);
            }
        }
        #endregion

        #region COMMAND_SAY
        [Command("say")]
        [Description("Echo echo echo.")]
        [Aliases("repeat")]

        public Task SayAsync(CommandContext ctx,
                            [RemainingText, Description("Text to say.")] string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                throw new InvalidCommandUsageException("Text missing.");

            if (ctx.Services.GetService<FilteringService>().TextContainsFilter(ctx.Guild.Id, text, out _))
                throw new CommandFailedException("You can't make me say something that contains filtered content for this guild.");

            return this.InformAsync(ctx, Formatter.Strip(text), ":loudspeaker:");
        }
        #endregion

        #region COMMAND_SIMULATE
        [Command("simulate")]
        [Description("Simulate another user.")]
        [Aliases("sim")]

        public async Task SimulateAsync(CommandContext ctx,
                                       [Description("Member to simulate.")] DiscordMember member)
        {
            var wbRegex = new Regex(@"\b");
            var rng = new SecureRandom();

            // IReadOnlyList<DiscordMessage> messages = await ctx.Channel.GetMessagesFromAsync(member, 10);
            //string[] parts = messages
            //    .Where(m => !string.IsNullOrWhiteSpace(m.Content) && !m.Content.StartsWith(ctx.Services.GetService<GuildConfigService>().GetGuildPrefix(ctx.Guild.Id)))
            //    .Select(m => SplitMessage(m.Content))
            //    .Distinct()
            //    .Shuffle()
            //    .Take(1 + rng.Next(10))
            //    .ToArray();

            //if (!parts.Any())
            //    throw new CommandFailedException("Not enough messages were sent from that user recently!");

            //await ctx.RespondAsync(embed: new DiscordEmbedBuilder {
            //    Description = $"{Emojis.Information} {string.Join(" ", parts)}",
            //    Color = this.ModuleColor,
            //}.WithFooter($"{member.DisplayName} simulation", member.AvatarUrl).Build());


            //string SplitMessage(string data)
            //{
            //    string[] words = wbRegex.Split(data);
            //    if (words.Length == 1)
            //        return words[0];
            //    int start = rng.Next(words.Length);
            //    int count = rng.Next(0, words.Length - start);
            //    return string.Join(" ", words.Skip(start).Take(count));
            //}
        }
        #endregion

        #region COMMAND_TTS
        [Command("tts")]
        [Description("Sends a tts message.")]

        [RequirePermissions(Permissions.SendTtsMessages)]
        public Task TtsAsync(CommandContext ctx,
                            [RemainingText, Description("Text.")] string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                throw new InvalidCommandUsageException("Text missing.");

            if (ctx.Services.GetService<FilteringService>().TextContainsFilter(ctx.Guild.Id, text, out _))
                throw new CommandFailedException("You can't make me say something that contains filtered content for this guild.");

            return ctx.RespondAsync(Formatter.BlockCode(Formatter.Strip(text)), isTTS: true);
        }
        #endregion

        #region COMMAND_UNLEET
        [Command("unleet")]
        [Description("Translates a message from leetspeak (expecting only letters in translated output).")]
        [Aliases("unl33t")]

        public Task Unl33tAsync(CommandContext ctx,
                               [RemainingText, Description("Text to unleet.")] string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                throw new InvalidCommandUsageException("Y0u d1dn'7 g1v3 m3 @ny 73x7...");

            var sb = new StringBuilder();
            foreach (char c in text) {
                char add = char.ToLowerInvariant(c);
                switch (add) {
                    case '1': add = 'i'; break;
                    case '@': add = 'a'; break;
                    case '4': add = 'a'; break;
                    case '3': add = 'e'; break;
                    case '5': add = 's'; break;
                    case '7': add = 't'; break;
                    case '0': add = 'o'; break;
                    default: break;
                }
                sb.Append(add);
            }

            return this.InformAsync(ctx, Emojis.Information, sb.ToString());
        }
        #endregion

        #region COMMAND_UPTIME
        [Command("uptime")]
        [Description("Prints out bot runtime information.")]
        public Task UptimeAsync(CommandContext ctx)
        {
            BotActivityService bas = ctx.Services.GetService<BotActivityService>();
            UptimeInformation uptimeInfo = bas.ShardUptimeInformation[ctx.Client.ShardId];
            TimeSpan processUptime = uptimeInfo.ProgramUptime;
            TimeSpan socketUptime = uptimeInfo.SocketUptime;

            return this.InformAsync(ctx, Emojis.Information,
                Formatter.Bold($"Uptime information:") +
                $"\n\n{Formatter.Bold("Shard:")} {ctx.Client.ShardId}\n" +
                $"{Formatter.Bold("Bot uptime:")} {processUptime.Days} days, {processUptime.ToString(@"hh\:mm\:ss")}\n" +
                $"{Formatter.Bold("Socket uptime:")} {socketUptime.Days} days, {socketUptime.ToString(@"hh\:mm\:ss")}"
            );
        }
        #endregion

        #region COMMAND_ZUGIFY
        [Command("zugify")]
        [Description("I don't even...")]
        [Aliases("z")]

        public Task ZugifyAsync(CommandContext ctx,
                               [RemainingText, Description("Text.")] string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                throw new InvalidCommandUsageException("Text missing.");

            text = text.ToLowerInvariant();
            var sb = new StringBuilder();
            foreach (char c in text) {
                if (char.IsLetter(c)) {
                    sb.Append(DiscordEmoji.FromName(ctx.Client, $":regional_indicator_{c}:"));
                } else if (char.IsDigit(c)) {
                    if (c == '0')
                        sb.Append(DiscordEmoji.FromName(ctx.Client, ":zero:"));
                    else
                        sb.Append(Emojis.Numbers.Get(c - '0' - 1));
                } else if (char.IsWhiteSpace(c)) {
                    sb.Append(DiscordEmoji.FromName(ctx.Client, ":large_blue_circle:"));
                } else if (c == '?')
                    sb.Append(Emojis.Question);
                else if (c == '!')
                    sb.Append(DiscordEmoji.FromName(ctx.Client, ":exclamation:"));
                else if (c == '.')
                    sb.Append(DiscordEmoji.FromName(ctx.Client, ":stop_button:"));
                else
                    sb.Append(c);
                sb.Append(' ');
            }

            return ctx.RespondAsync(embed: new DiscordEmbedBuilder {
                Description = sb.ToString()
            }.Build());
        }
        #endregion
    }
}
