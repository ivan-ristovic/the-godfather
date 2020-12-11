using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
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

namespace TheGodfather.Modules.Misc
{
    [Module(ModuleType.Misc)]
    [Cooldown(3, 5, CooldownBucketType.Channel), NotBlocked]
    public sealed class MiscModule : TheGodfatherServiceModule<RandomService>
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
                             [RemainingText, Description("desc-say")] string text)
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

        #region penis
        [Command("penis"), Priority(1)]
        [Aliases("size", "length", "manhood", "dick", "dicksize")]
        public Task PenisAsync(CommandContext ctx,
                              [Description("desc-members")] params DiscordMember[] members)
            => this.InternalPenisAsync(ctx, members);

        [Command("peniscompare"), Priority(0)]
        public Task PenisAsync(CommandContext ctx,
                              [Description("desc-users")] params DiscordUser[] users)
            => this.InternalPenisAsync(ctx, users);
        #endregion

        #region ping
        [Command("ping")]
        public Task PingAsync(CommandContext ctx)
            => ctx.ImpInfoAsync(this.ModuleColor, Emojis.Heartbeat, "fmt-ping", ctx.Client.Ping);
        #endregion

        #region prefix
        [Command("prefix")]
        [Aliases("setprefix", "pref", "setpref")]
        [RequireGuild, RequireOwnerOrPermissions(Permissions.Administrator)]
        public async Task GetOrSetPrefixAsync(CommandContext ctx,
                                             [Description("desc-prefix")] string? prefix = null)
        {
            GuildConfigService gcs = ctx.Services.GetRequiredService<GuildConfigService>();
            if (string.IsNullOrWhiteSpace(prefix)) {
                string p = gcs.GetGuildPrefix(ctx.Guild.Id);
                await ctx.ImpInfoAsync(this.ModuleColor, Emojis.Information, "fmt-prefix", p);
                return;
            }

            if (prefix.Length > GuildConfig.PrefixLimit)
                throw new CommandFailedException(ctx, "cmd-err-prefix", GuildConfig.PrefixLimit);

            GuildConfig gcfg = await gcs.ModifyConfigAsync(ctx.Guild.Id, cfg => cfg.Prefix = prefix);
            await ctx.InfoAsync(this.ModuleColor);
        }
        #endregion

        #region rate
        [Command("rate"), Priority(1)]
        [Aliases("score", "graph", "rating")]
        [RequireBotPermissions(Permissions.AttachFiles)]
        public Task RateAsync(CommandContext ctx,
                             [Description("desc-members")] params DiscordMember[] members)
            => this.InternalRateAsync(ctx, members);

        [Command("rate"), Priority(0)]
        public Task RateAsync(CommandContext ctx,
                             [Description("desc-users")] params DiscordUser[] users)
            => this.InternalRateAsync(ctx, users);
        #endregion

        #region report
        [Command("report"), UsesInteractivity]
        [Cooldown(1, 3600, CooldownBucketType.User)]
        public async Task SendErrorReportAsync(CommandContext ctx,
                                              [RemainingText, Description("desc-issue")] string issue)
        {
            if (string.IsNullOrWhiteSpace(issue))
                throw new InvalidCommandUsageException(ctx, "cmd-err-issue-none");

            DiscordDmChannel? dm = await ctx.Client.CreateOwnerDmChannel();
            if (dm is null)
                throw new CommandFailedException(ctx, "cmd-err-issue-fail");

            if (await ctx.WaitForBoolReplyAsync("q-issue")) {
                Log.Warning($"Report from {ctx.User.Username} ({ctx.User.Id}): {issue}");
                var emb = new DiscordEmbedBuilder {
                    Title = "Issue reported",
                    Description = issue
                };
                emb.WithAuthor(ctx.User.ToString(), iconUrl: ctx.User.AvatarUrl ?? ctx.User.DefaultAvatarUrl);
                if (ctx.Guild is { })
                    emb.AddField("Guild", $"{ctx.Guild} owned by {ctx.Guild.Owner}");

                await dm.SendMessageAsync("A new issue has been reported!", embed: emb.Build());
                await ctx.InfoAsync(this.ModuleColor, "str-reported");
            }
        }
        #endregion

        #region say
        [Command("say")]
        [Aliases("repeat", "echo")]
        public Task SayAsync(CommandContext ctx,
                            [RemainingText, Description("desc-say")] string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                throw new InvalidCommandUsageException(ctx, "cmd-err-text-none");

            return ctx.Services.GetRequiredService<FilteringService>().TextContainsFilter(ctx.Guild.Id, text, out _)
                ? throw new CommandFailedException(ctx, "cmd-err-say")
                : ctx.RespondWithLocalizedEmbedAsync(emb => {
                    emb.WithColor(this.ModuleColor);
                    emb.WithDescription($"{Emojis.Loudspeaker} {Formatter.Strip(text)}");
                });
        }
        #endregion

        #region simulate
        //[Command("simulate")]
        [Aliases("sim")]
        [RequireGuild]
        public async Task SimulateAsync(CommandContext ctx,
                                       [Description("desc-member")] DiscordMember member)
        {
            // TODO
        }
        #endregion

        #region tts
        [Command("tts")]
        [RequirePermissions(Permissions.SendTtsMessages)]
        public Task TtsAsync(CommandContext ctx,
                            [RemainingText, Description("desc-say")] string text)
        {

            if (string.IsNullOrWhiteSpace(text))
                throw new InvalidCommandUsageException(ctx, "cmd-err-text-none");

            return ctx.Services.GetRequiredService<FilteringService>().TextContainsFilter(ctx.Guild.Id, text, out _)
                ? throw new CommandFailedException(ctx, "cmd-err-say")
                : ctx.RespondAsync(Formatter.BlockCode(Formatter.Strip(text)), isTTS: true);
        }
        #endregion

        #region unleet
        [Command("unleet")]
        [Aliases("unl33t")]
        public Task Unl33tAsync(CommandContext ctx,
                               [RemainingText, Description("desc-say")] string leet)
        {
            if (string.IsNullOrWhiteSpace(leet))
                throw new InvalidCommandUsageException(ctx, "cmd-err-leet-none");

            string text = this.Service.FromLeet(leet);
            return ctx.RespondAsync(embed: new DiscordEmbedBuilder {
                Color = this.ModuleColor,
                Description = $"{Emojis.Information} {text}",
            });
        }
        #endregion


        #region helpers
        private Task InternalPenisAsync(CommandContext ctx, IReadOnlyList<DiscordUser> users)
        {
            users = users.Distinct().ToList();
            if (!users.Any())
                users = new[] { ctx.User };
            if (users.Count >= 10)
                throw new InvalidCommandUsageException(ctx, "cmd-err-size");

            var sb = new StringBuilder();
            foreach (DiscordUser user in users) {
                if (user.IsCurrent)
                    return ctx.InfoAsync(this.ModuleColor, Emojis.Ruler, "cmd-err-size-bot");
                sb.Append(Formatter.Bold(this.Service.Size(user.Id))).Append(' ').AppendLine(user.Mention);
            }

            return ctx.RespondWithLocalizedEmbedAsync(emb => {
                emb.WithColor(this.ModuleColor);
                emb.WithLocalizedDescription("fmt-size", Emojis.Ruler, sb.ToString());
            });
        }


        public async Task InternalRateAsync(CommandContext ctx, IReadOnlyList<DiscordUser> users)
        {
            users = users.Distinct().ToList();
            if (!users.Any())
                users = new[] { ctx.User };
            if (users.Count > 8)
                throw new InvalidCommandUsageException(ctx, "cmd-err-rate");

            if (users.Any(u => u.IsCurrent)) {
                await ctx.InfoAsync(this.ModuleColor, Emojis.Ruler, "cmd-err-size-bot");
                return;
            }

            LocalizationService lcs = ctx.Services.GetRequiredService<LocalizationService>();
            using Stream ms = this.Service.Rate(users.Select(u => (u.ToDiscriminatorString(), u.Id)));
            await ctx.RespondWithFileAsync("Rating.jpg", ms, embed: new DiscordEmbedBuilder {
                Description = lcs.GetString(ctx.Guild?.Id, "fmt-rating", Emojis.Ruler, users.Select(u => u.Mention).JoinWith(", ")),
                Color = this.ModuleColor,
            });
        }
        #endregion
    }
}
