#region USING_DIRECTIVES
using System;
using System.Linq;
using System.Threading.Tasks;

using TheGodfather.Helpers;
using TheGodfather.Helpers.DataManagers;
using TheGodfather.Exceptions;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Interactivity;
using DSharpPlus.Entities;
#endregion

namespace TheGodfather.Commands.Main
{
    [Description("Main bot commands.")]
    [Cooldown(2, 3, CooldownBucketType.User), Cooldown(5, 3, CooldownBucketType.Channel)]
    [CheckListeningAttribute]
    public class CommandsMain
    {
        #region COMMAND_EMBED
        [Command("embed")]
        [Description("Embed an image given as an URL.")]
        [RequirePermissions(Permissions.AttachFiles)]
        public async Task EmbedUrlAsync(CommandContext ctx,
                                       [Description("Image URL.")] string url)
        {
            if (string.IsNullOrWhiteSpace(url))
                throw new InvalidCommandUsageException("URL missing!");

            try {
                await ctx.RespondAsync(embed: new DiscordEmbedBuilder() { ImageUrl = url })
                    .ConfigureAwait(false);
            } catch (UriFormatException e) {
                throw new CommandFailedException("URL is not in correct format!", e);
            }
        }
        #endregion

        #region COMMAND_GREET
        [Command("greet")]
        [Description("Greets a user and starts a conversation.")]
        [Aliases("hello", "hi", "halo", "hey", "howdy", "sup")]
        public async Task Greet(CommandContext ctx)
        {
            await ctx.RespondAsync($"{DiscordEmoji.FromName(ctx.Client, ":wave:")} Hi, {ctx.User.Mention}!")
                .ConfigureAwait(false);
            var interactivity = ctx.Client.GetInteractivityModule();
            var msg = await interactivity.WaitForMessageAsync(
                xm => xm.Author.Id == ctx.User.Id && xm.Content.ToLower().StartsWith("how are you"),
                TimeSpan.FromMinutes(1)
            ).ConfigureAwait(false);

            if (msg != null) {
                switch (new Random().Next(0, 5)) {
                    case 0: await ctx.RespondAsync($"I'm fine, thank you!").ConfigureAwait(false); break;
                    case 1: await ctx.RespondAsync($"Up and running, thanks for asking!").ConfigureAwait(false); break;
                    case 2: await ctx.RespondAsync($"Doing fine, thanks!").ConfigureAwait(false); break;
                    case 3: await ctx.RespondAsync($"Wonderful, thanks!").ConfigureAwait(false); break;
                    case 4: await ctx.RespondAsync($"Awesome, thank you!").ConfigureAwait(false); break;
                    default: break;
                }
            }
        }
        #endregion

        #region COMMAND_INVITE
        [Command("invite")]
        [Description("Get an instant invite link for the current channel.")]
        [Aliases("getinvite")]
        [RequirePermissions(Permissions.CreateInstantInvite)]
        public async Task GetInstantInviteAsync(CommandContext ctx)
        {
            var invites = await ctx.Channel.GetInvitesAsync()
                .ConfigureAwait(false);
            
            var permanent = invites.Where(
                inv => (inv.Channel.Id == ctx.Channel.Id) && !inv.IsTemporary
            );

            if (permanent.Count() > 0) {
                await ctx.RespondAsync(permanent.ElementAt(0).ToString())
                    .ConfigureAwait(false);
            } else {
                var invite = await ctx.Channel.CreateInviteAsync(max_age: 3600, temporary: true)
                    .ConfigureAwait(false);
                await ctx.RespondAsync(invite.ToString() + "\n\n" + Formatter.Italic("This invite will expire in one hour!"))
                    .ConfigureAwait(false);
            }
        }
        #endregion

        #region COMMAND_LEAVE
        [Command("leave")]
        [Description("Makes Godfather leave the server.")]
        [RequireUserPermissions(Permissions.KickMembers)]
        public async Task LeaveAsync(CommandContext ctx)
        {
            var inter = ctx.Client.GetInteractivityModule();
            await ctx.RespondAsync("Are you sure?")
                .ConfigureAwait(false);
            var m = await inter.WaitForMessageAsync(
                x => x.Channel.Id == ctx.Channel.Id && x.Author.Id == ctx.User.Id && (x.Content.ToLower() == "yes" || x.Content.ToLower() == "no")
                , TimeSpan.FromSeconds(10)
            ).ConfigureAwait(false);
            if (m != null) {
                if (m.Message.Content == "yes") {
                    await ctx.RespondAsync("Go find a new bot, since this one is leaving!")
                        .ConfigureAwait(false);
                    await ctx.Guild.LeaveAsync()
                        .ConfigureAwait(false);
                } else {
                    await ctx.RespondAsync("Jewsus Krest you scared me.")
                        .ConfigureAwait(false);
                }
            } else {
                await ctx.RespondAsync("No response? Guess I'll stay then.")
                    .ConfigureAwait(false);
            }
        }
        #endregion

        #region COMMAND_LEET
        [Command("leet")]
        [Description("Wr1t3s m3ss@g3 1n 1337sp34k.")]
        public async Task L33tAsync(CommandContext ctx, 
                                   [RemainingText, Description("Text")] string s)
        {
            if (string.IsNullOrWhiteSpace(s))
                throw new InvalidCommandUsageException("Y0u d1dn'7 g1v3 m3 @ny 73x7...");

            var rnd = new Random();
            string leet_s = "";
            foreach (char c in s) {
                char add;
                switch (c) {
                    case 'i': add = (rnd.Next() % 2 == 0) ? 'i' : '1'; break;
                    case 'l': add = (rnd.Next() % 2 == 0) ? 'l' : '1'; break;
                    case 'e': add = (rnd.Next() % 2 == 0) ? 'e' : '3'; break;
                    case 'a': add = (rnd.Next() % 2 == 0) ? '@' : '4'; break;
                    case 't': add = (rnd.Next() % 2 == 0) ? 't' : '7'; break;
                    case 'o': add = (rnd.Next() % 2 == 0) ? 'o' : '0'; break;
                    case 's': add = (rnd.Next() % 2 == 0) ? 's' : '5'; break;
                    default: add = c ; break;
                }
                leet_s += (rnd.Next() % 2 == 0) ? Char.ToUpper(add) : Char.ToLower(add);
            }

            await ctx.RespondAsync(leet_s)
                .ConfigureAwait(false);
        }
        #endregion
        
        #region COMMAND_PING
        [Command("ping")]
        [Description("Ping the bot.")]
        public async Task PingAsync(CommandContext ctx)
        {
            await ctx.RespondAsync($"Pong! {ctx.Client.Ping}ms")
                .ConfigureAwait(false);
        }
        #endregion
        
        #region COMMAND_PREFIX
        [Command("prefix")]
        [Description("Get current guild prefix, or change it.")]
        [Aliases("setprefix")]
        [RequireUserPermissions(Permissions.Administrator)]
        public async Task GetOrSetPrefixAsync(CommandContext ctx,
                                             [Description("Prefix to set.")] string prefix = null)
        {
            var gcm = ctx.Dependencies.GetDependency<GuildConfigManager>();

            if (string.IsNullOrWhiteSpace(prefix)) {
                string p = gcm.GetGuildPrefix(ctx.Guild.Id);
                await ctx.RespondAsync("Current prefix for this guild is: " + Formatter.Bold(p))
                    .ConfigureAwait(false);
                return;
            }

            if (prefix.Length > 10)
                throw new CommandFailedException("Prefix length cannot be longer than 10 characters.");

            if (gcm.TrySetGuildPrefix(ctx.Guild.Id, prefix))
                await ctx.RespondAsync("Successfully changed the prefix for this guild to: " + Formatter.Bold(prefix)).ConfigureAwait(false);
            else
                throw new CommandFailedException("Failed to set prefix.");
        }
        #endregion

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

        #region COMMAND_REPORT
        [Command("report")]
        [Description("Send a report message to owner about a bug (please don't abuse... please).")]
        public async Task SendErrorReportAsync(CommandContext ctx, 
                                              [RemainingText, Description("Text.")] string issue)
        {
            if (string.IsNullOrWhiteSpace(issue))
                throw new InvalidCommandUsageException("Text missing.");
            
            await ctx.RespondAsync("Are you okay with your user and guild info being sent for further inspection?\n\n" +
                Formatter.Italic("(Please either respond with 'yes' or wait 15 seconds for the prompt to time out)"))
                .ConfigureAwait(false);
            var interactivity = ctx.Client.GetInteractivityModule();
            var msg = await interactivity.WaitForMessageAsync(
                x => x.Author.Id == ctx.User.Id && x.Channel.Id == ctx.Channel.Id && x.Content.ToLower() == "yes"
                , TimeSpan.FromSeconds(15)
            ).ConfigureAwait(false);
            if (msg != null) {
                ctx.Client.DebugLogger.LogMessage(LogLevel.Info, "TheGodfather", $"Report from {ctx.User.Username} ({ctx.User.Id}): {issue}", DateTime.Now);
                var dm = await ctx.Client.CreateDmAsync(ctx.Client.CurrentApplication.Owner)
                    .ConfigureAwait(false);

                await dm.SendMessageAsync("A new issue has been reported!", embed:
                    new DiscordEmbedBuilder() {
                        Title = "Issue",
                        Description = issue,
                    }.WithAuthor($"{ctx.User.Username}#{ctx.User.Discriminator}", icon_url: ctx.User.AvatarUrl ?? ctx.User.DefaultAvatarUrl)
                     .AddField("Guild", $"{ctx.Guild.Name} ({ctx.Guild.Id}) owned by {ctx.Guild.Owner.Username}#{ctx.Guild.Owner.Discriminator}")
                     .Build()
                ).ConfigureAwait(false);
                await ctx.RespondAsync("Your issue has been reported.")
                    .ConfigureAwait(false);
            } else {
                await ctx.RespondAsync("Your issue has not been reported.")
                    .ConfigureAwait(false);
            }
        }
        #endregion

        #region COMMAND_SAY
        [Command("say")]
        [Description("Repeats after you.")]
        [Aliases("repeat")]
        public async Task SayAsync(CommandContext ctx, 
                                  [RemainingText, Description("Text.")] string s)
        {
            if (string.IsNullOrWhiteSpace(s))
                throw new InvalidCommandUsageException("Text missing.");

            if (ctx.Dependencies.GetDependency<GuildConfigManager>().ContainsFilter(ctx.Guild.Id, s))
                throw new CommandFailedException("You can't make me say something that contains filtered content for this guild.");
            
            await ctx.RespondAsync(s)
                .ConfigureAwait(false);
        }
        #endregion

        #region COMMAND_ZUGIFY
        [Command("zugify")]
        [Description("I don't even...")]
        [Aliases("z")]
        public async Task ZugifyAsync(CommandContext ctx, 
                                     [RemainingText, Description("Text.")] string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                throw new InvalidCommandUsageException("Text missing.");

            text = text.ToLower();
            string s = "";
            foreach (char c in text) {
                if (c >= 'a' && c <= 'z') {
                    s += DiscordEmoji.FromName(ctx.Client, $":regional_indicator_{c}:");
                } else if (char.IsDigit(c)) {
                    switch (c) {
                        case '0': s += DiscordEmoji.FromName(ctx.Client, ":zero:");     break;
                        case '1': s += DiscordEmoji.FromName(ctx.Client, ":one:");      break;
                        case '2': s += DiscordEmoji.FromName(ctx.Client, ":two:");      break;
                        case '3': s += DiscordEmoji.FromName(ctx.Client, ":three:");    break;
                        case '4': s += DiscordEmoji.FromName(ctx.Client, ":four:");     break;
                        case '5': s += DiscordEmoji.FromName(ctx.Client, ":five:");     break;
                        case '6': s += DiscordEmoji.FromName(ctx.Client, ":six:");      break;
                        case '7': s += DiscordEmoji.FromName(ctx.Client, ":seven:");    break;
                        case '8': s += DiscordEmoji.FromName(ctx.Client, ":eight:");    break;
                        case '9': s += DiscordEmoji.FromName(ctx.Client, ":nine:");     break;
                    }
                } else if (c == ' ') {
                    s += DiscordEmoji.FromName(ctx.Client, ":large_blue_circle:");
                } else if (c == '?')
                    s += DiscordEmoji.FromName(ctx.Client, ":question:");
                else if (c == '!')
                    s += DiscordEmoji.FromName(ctx.Client, ":exclamation:");
                else
                    s += c;
                s += " ";
            }

            await ctx.RespondAsync(s)
                .ConfigureAwait(false);
        }
        #endregion
    }
}
