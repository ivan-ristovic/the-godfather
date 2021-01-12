using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Extensions;
using Microsoft.Extensions.DependencyInjection;
using TheGodfather.Attributes;
using TheGodfather.Common;
using TheGodfather.Database.Models;
using TheGodfather.Exceptions;
using TheGodfather.Extensions;
using TheGodfather.Modules.Games.Extensions;
using TheGodfather.Modules.Games.Services;
using TheGodfather.Services;

namespace TheGodfather.Modules.Games
{
    public partial class GamesModule
    {
        [Group("hangman"), UsesInteractivity]
        [Aliases("h", "hang", "hm")]
        [RequireGuild]
        public sealed class HangmanModule : TheGodfatherServiceModule<ChannelEventService>
        {
            #region game hangman
            [GroupCommand]
            public async Task ExecuteGroupAsync(CommandContext ctx)
            {
                if (this.Service.IsEventRunningInChannel(ctx.Channel.Id))
                    throw new CommandFailedException(ctx, "cmd-err-evt-dup");

                DiscordDmChannel? dm = await ctx.Client.CreateDmChannelAsync(ctx.User.Id);
                if (dm is null)
                    throw new CommandFailedException(ctx, "cmd-err-dm-create");

                await dm.LocalizedEmbedAsync(this.Localization, Emojis.Question, this.ModuleColor, "q-game-hm");
                await ctx.ImpInfoAsync(this.ModuleColor, Emojis.Question, "fmt-game-hm", ctx.User.Mention);

                string? word = null;
                for (int i = 0; i < 5; i++) {
                    DiscordMessage? reply = await ctx.WaitForDmReplyAsync(dm, ctx.User);
                    if (string.IsNullOrWhiteSpace(reply?.Content)) {
                        await ctx.FailAsync("cmd-err-game-hm");
                        return;
                    }
                    if (reply.Content.All(c => char.IsLetter(c))) {
                        word = reply.Content;
                        break;
                    }
                    await dm.LocalizedEmbedAsync(this.Localization, Emojis.Question, this.ModuleColor, "cmd-err-game-hm-format");
                }

                if (word is null) {
                    await ctx.FailAsync("cmd-err-game-hm");
                    return;
                }

                await dm.LocalizedEmbedAsync(this.Localization, Emojis.Information, this.ModuleColor, "fmt-game-hm-ok", word);
                var game = new HangmanGame(ctx.Client.GetInteractivity(), ctx.Channel, word, ctx.User);
                this.Service.RegisterEventInChannel(game, ctx.Channel.Id);
                try {
                    await game.RunAsync(this.Localization);
                    if (game.Winner is { }) {
                        GameStatsService gss = ctx.Services.GetRequiredService<GameStatsService>();
                        await gss.UpdateStatsAsync(game.Winner.Id, s => s.HangmanWon++);
                    }
                } finally {
                    this.Service.UnregisterEventInChannel(ctx.Channel.Id);
                }
            }
            #endregion

            #region game hangman rules
            [Command("rules")]
            [Aliases("help", "h", "ruling", "rule")]
            public Task RulesAsync(CommandContext ctx)
                => ctx.ImpInfoAsync(this.ModuleColor, Emojis.Information, "str-game-hm");
            #endregion

            #region game hangman stats
            [Command("stats"), Priority(1)]
            [Aliases("s")]
            public Task StatsAsync(CommandContext ctx,
                                  [Description("desc-member")] DiscordMember? member = null)
                => this.StatsAsync(ctx, member as DiscordUser);

            [Command("stats"), Priority(0)]
            public async Task StatsAsync(CommandContext ctx,
                                        [Description("desc-user")] DiscordUser? user = null)
            {
                user ??= ctx.User;
                GameStatsService gss = ctx.Services.GetRequiredService<GameStatsService>();

                GameStats? stats = await gss.GetAsync(user.Id);
                await ctx.RespondWithLocalizedEmbedAsync(emb => {
                    emb.WithLocalizedTitle("fmt-game-stats", user.ToDiscriminatorString());
                    emb.WithColor(this.ModuleColor);
                    emb.WithThumbnail(user.AvatarUrl);
                    if (stats is null)
                        emb.WithLocalizedDescription("str-game-stats-none");
                    else
                        emb.WithDescription(stats.BuildHangmanStatsString());
                });
            }
            #endregion

            #region game hangman top
            [Command("top")]
            [Aliases("t", "leaderboard")]
            public async Task TopAsync(CommandContext ctx)
            {
                GameStatsService gss = ctx.Services.GetRequiredService<GameStatsService>();
                IReadOnlyList<GameStats> topStats = await gss.GetTopHangmanStatsAsync();
                string top = await GameStatsExtensions.BuildStatsStringAsync(ctx.Client, topStats, s => s.BuildHangmanStatsString());
                await ctx.ImpInfoAsync(this.ModuleColor, Emojis.Trophy, "fmt-game-hm-top", top);
            }
            #endregion
        }
    }
}
