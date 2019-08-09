#region USING_DIRECTIVES
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using TheGodfather.Common;
using TheGodfather.Common.Attributes;
using TheGodfather.Database;
using TheGodfather.Database.Entities;
using TheGodfather.Exceptions;
using TheGodfather.Extensions;
using TheGodfather.Modules.Administration.Extensions;
using TheGodfather.Modules.Administration.Services;
using TheGodfather.Modules.Misc.Common;
using TheGodfather.Services;
using TheGodfather.Services.Common;
#endregion

namespace TheGodfather.Modules.Misc
{
    [Module(ModuleType.Miscellaneous)]
    [Cooldown(3, 5, CooldownBucketType.Channel), NotBlocked]
    public class MiscModule : TheGodfatherModule
    {

        public MiscModule(DatabaseContextBuilder db) 
            : base(db)
        {
            
        }


        #region COMMAND_8BALL
        [Command("8ball")]
        [Description("An almighty ball which knows the answer to any question you ask. Alright, the answer is random, so what?")]
        [Aliases("8b")]
        
        public Task EightBallAsync(CommandContext ctx,
                                  [RemainingText, Description("A question for the almighty ball.")] string question)
        {
            if (string.IsNullOrWhiteSpace(question))
                throw new InvalidCommandUsageException("The almighty ball requires a question.");

            return this.InformAsync(ctx, $"{ctx.User.Mention} {EightBall.GenerateAnswer(question, ctx.Channel.Users)}", ":8ball:");
        }
        #endregion

        #region COMMAND_COINFLIP
        [Command("coinflip")]
        [Description("Flip a coin.")]
        [Aliases("coin", "flip")]
        public Task CoinflipAsync(CommandContext ctx)
            => this.InformAsync(ctx, $"{ctx.User.Mention} flipped {Formatter.Bold(GFRandom.Generator.NextBool() ? "Heads" : "Tails")}", ":full_moon_with_face:");
        #endregion

        #region COMMAND_DICE
        [Command("dice")]
        [Description("Roll a dice.")]
        [Aliases("die", "roll")]
        public Task DiceAsync(CommandContext ctx)
            => this.InformAsync(ctx, Emojis.Dice, $"{ctx.User.Mention} rolled a {Formatter.Bold(GFRandom.Generator.Next(1, 7).ToString())}");
        #endregion

        #region COMMAND_INVITE
        [Command("invite")]
        [Description("Get an instant invite link for the current guild.")]
        [Aliases("getinvite")]
        [RequirePermissions(Permissions.CreateInstantInvite)]
        public async Task GetInstantInviteAsync(CommandContext ctx)
        {
            IReadOnlyList<DiscordInvite> invites = await ctx.Guild.GetInvitesAsync();
            IEnumerable<DiscordInvite> permanent = invites.Where(inv => !inv.IsTemporary);
            if (permanent.Any()) {
                await ctx.RespondAsync(permanent.First().ToString());
            } else {
                DiscordInvite invite = await ctx.Channel.CreateInviteAsync(max_age: 3600, temporary: true, reason: ctx.BuildInvocationDetailsString());
                await ctx.RespondAsync($"{invite} {Formatter.Italic("(This invite will expire in one hour!)")}");
            }
        }
        #endregion

        #region COMMAND_ITEMS
        [Command("items")]
        [Description("View user's purchased items (see ``bank`` and ``shop``).")]
        [Aliases("myitems", "purchases")]
        
        [RequirePermissions(Permissions.CreateInstantInvite)]
        public async Task GetPurchasedItemsAsync(CommandContext ctx,
                                                [Description("User.")] DiscordUser user = null)
        {
            user = user ?? ctx.User;

            List<DatabasePurchasedItem> items;
            using (DatabaseContext db = this.Database.CreateContext()) {
                items = await db.PurchasedItems
                        .Include(i => i.DbPurchasableItem)
                    .Where(i => i.UserId == ctx.User.Id && i.DbPurchasableItem.GuildId == ctx.Guild.Id)
                    .OrderBy(i => i.DbPurchasableItem.Price)
                    .ToListAsync();
            }
            
            if (!items.Any())
                throw new CommandFailedException("No items purchased!");

            await ctx.SendCollectionInPagesAsync(
                $"Items owned by {user.Username}",
                items,
                item => $"{Formatter.Bold(item.DbPurchasableItem.Name)} | {item.DbPurchasableItem.Price}",
                this.ModuleColor,
                5
            );
        }
        #endregion

        #region COMMAND_LEAVE
        [Command("leave"), UsesInteractivity]
        [Description("Makes Godfather leave the guild.")]
        [RequireOwnerOrPermissions(Permissions.Administrator)]
        public async Task LeaveAsync(CommandContext ctx)
        {
            if (await ctx.WaitForBoolReplyAsync("Are you sure you want me to leave this guild?")) {
                await this.InformAsync(ctx, Emojis.Wave, "Go find a new bot, since this one is leaving!");
                await ctx.Guild.LeaveAsync();
            } else {
                await this.InformAsync(ctx, "Guess I'll stay then.", ":no_mouth:");
            }
        }
        #endregion

        #region COMMAND_LEET
        [Command("leet")]
        [Description("Wr1t3s g1v3n tEx7 1n p5EuDo 1337sp34k.")]
        [Aliases("l33t")]
        
        public Task L33tAsync(CommandContext ctx,
                             [RemainingText, Description("Text.")] string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                throw new InvalidCommandUsageException("Y0u d1dn'7 g1v3 m3 @ny 73x7...");

            var sb = new StringBuilder();
            foreach (char c in text) {
                char add;
                bool r = GFRandom.Generator.NextBool();
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
                sb.Append(GFRandom.Generator.NextBool() ? char.ToUpperInvariant(add) : char.ToLowerInvariant(add));
            }

            return this.InformAsync(ctx, Emojis.Information, sb.ToString());
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
                Description = FormatterExtensions.Spoiler(url.ToString()),
                Color = DiscordColor.Red
            };
            if (!string.IsNullOrWhiteSpace(info))
                emb.AddField("Additional info", Formatter.BlockCode(FormatterExtensions.StripMarkdown(info)));

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

            if (prefix.Length > 12)
                throw new CommandFailedException("Prefix cannot be longer than 12 characters.");

            DatabaseGuildConfig gcfg = await ctx.Services.GetService<GuildConfigService>().ModifyConfigAsync(ctx.Guild.Id, cfg => {
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
                using (var chart = new Bitmap("Resources/graph.png"))
                using (var g = Graphics.FromImage(chart)) {
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
                }
            } catch (FileNotFoundException e) {
                ctx.Client.DebugLogger.LogMessage(LogLevel.Error, "TheGodfather", $"graph.png load failed! Details: {e.ToString()}", DateTime.Now);
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
                ctx.Client.DebugLogger.LogMessage(LogLevel.Info, "TheGodfather", $"Report from {ctx.User.Username} ({ctx.User.Id}): {issue}", DateTime.Now);
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

            if (ctx.Services.GetService<FilteringService>().TextContainsFilter(ctx.Guild.Id, text))
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
            IReadOnlyList<DiscordMessage> messages = await ctx.Channel.GetMessagesFromAsync(member, 10);
            string[] parts = messages
                .Where(m => !string.IsNullOrWhiteSpace(m.Content) && !m.Content.StartsWith(ctx.Services.GetService<GuildConfigService>().GetGuildPrefix(ctx.Guild.Id)))
                .Select(m => SplitMessage(m.Content))
                .Distinct()
                .Shuffle()
                .Take(1 + GFRandom.Generator.Next(10))
                .ToArray();

            if (!parts.Any())
                throw new CommandFailedException("Not enough messages were sent from that user recently!");

            await ctx.RespondAsync(embed: new DiscordEmbedBuilder {
                Description = $"{Emojis.Information} {string.Join(" ", parts)}",
                Color = this.ModuleColor,
            }.WithFooter($"{member.DisplayName} simulation", member.AvatarUrl).Build());

            string SplitMessage(string data)
            {
                string[] words = new Regex("\\b").Split(data);
                if (words.Length == 1)
                    return words[0];
                int start = GFRandom.Generator.Next(words.Length);
                int count = GFRandom.Generator.Next(0, words.Length - start);
                return string.Join(" ", words.Skip(start).Take(count));
            }
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

            if (ctx.Services.GetService<FilteringService>().TextContainsFilter(ctx.Guild.Id, text))
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
