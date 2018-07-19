#region USING_DIRECTIVES
using System;
using System.Linq;
using System.Threading.Tasks;

using TheGodfather.Common.Attributes;
using TheGodfather.Exceptions;
using TheGodfather.Extensions;
using TheGodfather.Services.Database.Ranks;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;
using TheGodfather.Services.Database;
#endregion

namespace TheGodfather.Modules.Misc
{
    [Group("rank"), Module(ModuleType.Miscellaneous)]
    [Description("User ranking commands. If invoked without subcommands, prints sender's rank.")]
    [Aliases("ranks", "ranking", "level")]
    [UsageExamples("!rank",
                   "!rank @Someone")]
    [Cooldown(3, 5, CooldownBucketType.Channel)]
    [NotBlocked]
    public class RanksModule : TheGodfatherModule
    {

        public RanksModule(SharedData shared, DBService db) : base(shared, db) { }


        [GroupCommand]
        public async Task ExecuteGroupAsync(CommandContext ctx,
                                           [Description("User.")] DiscordUser user = null)
        {
            if (user == null)
                user = ctx.User;

            var rank = Shared.CalculateRankForUser(user.Id);
            var msgcount = Shared.GetMessageCountForUser(user.Id);
            var rankname = await Database.GetRankAsync(ctx.Guild.Id, rank)
                .ConfigureAwait(false);

            var emb = new DiscordEmbedBuilder() {
                Title = user.Username,
                Description = "User status",
                Color = DiscordColor.Aquamarine,
                ThumbnailUrl = user.AvatarUrl
            };
            emb.AddField("Rank", $"{Formatter.Bold($"#{rank}")} : {Formatter.Italic(rankname ?? "No custom rank name set for this rank in this guild")}")
               .AddField("XP", $"{msgcount}", inline: true)
               .AddField("XP needed for next rank", $"{(rank + 1) * (rank + 1) * 10}", inline: true);

            await ctx.RespondAsync(embed: emb.Build())
                .ConfigureAwait(false);
        }


        #region COMMAND_RANK_ADD
        [Command("add"), Priority(1)]
        [Module(ModuleType.Miscellaneous)]
        [Description("Add a custom name for given rank in this guild.")]
        [Aliases("+", "a", "rename")]
        [RequireUserPermissions(Permissions.ManageGuild)]
        [UsageExamples("!rank add 1 Private")]
        public async Task AddAsync(CommandContext ctx,
                                  [Description("Rank.")] int rank,
                                  [RemainingText, Description("Rank name.")] string name)
        {
            if (rank < 0 || rank > 100)
                throw new CommandFailedException("You can only set rank names in range [0, 100]!");

            if (string.IsNullOrWhiteSpace(name))
                throw new CommandFailedException("Name for the rank is missing!");

            if (name.Length > 30)
                throw new CommandFailedException("Rank name cannot be longer than 30 characters!");

            await Database.AddRankAsync(ctx.Guild.Id, rank, name)
                .ConfigureAwait(false);
            await ctx.InformSuccessAsync()
                .ConfigureAwait(false);
        }
        #endregion

        #region COMMAND_RANK_DELETE
        [Command("delete"), Module(ModuleType.Miscellaneous)]
        [Description("Remove a custom name for given rank in this guild.")]
        [Aliases("-", "remove", "rm", "del", "revert")]
        [RequireUserPermissions(Permissions.ManageGuild)]
        [UsageExamples("!rank delete 3")]
        public async Task DeleteAsync(CommandContext ctx,
                                     [Description("Rank.")] int rank)
        {
            await Database.RemoveRankAsync(ctx.Guild.Id, rank)
                .ConfigureAwait(false);
            await ctx.InformSuccessAsync()
                .ConfigureAwait(false);
        }
        #endregion

        #region COMMAND_RANK_LIST
        [Command("list"), Module(ModuleType.Miscellaneous)]
        [Description("Print all customized ranks for this guild.")]
        [Aliases("levels")]
        [UsageExamples("!rank list")]
        public async Task RankListAsync(CommandContext ctx)
        {
            var ranks = await Database.GetAllRanksAsync(ctx.Guild.Id)
                .ConfigureAwait(false);
            if (!ranks.Any())
                throw new CommandFailedException("No custom rank names registered for this guild!");

            await ctx.SendCollectionInPagesAsync(
                "Custom ranks in this guild",
                ranks,
                kvp => $"{kvp.Key} | {Formatter.Bold(kvp.Value)} | XP needed: {Formatter.Bold(Shared.CalculateXpNeededForRank(kvp.Key).ToString())}",
                DiscordColor.IndianRed
            ).ConfigureAwait(false);
        }
        #endregion

        #region COMMAND_RANK_TOP
        [Command("top"), Module(ModuleType.Miscellaneous)]
        [Description("Get rank leaderboard.")]
        [UsageExamples("!rank top")]
        public async Task TopAsync(CommandContext ctx)
        {
            var top = Shared.MessageCount.OrderByDescending(v => v.Value).Take(10);
            var emb = new DiscordEmbedBuilder() {
                Title = "Top ranked users (globally): ",
                Color = DiscordColor.Purple
            };

            var ranks = await Database.GetAllRanksAsync(ctx.Guild.Id)
                .ConfigureAwait(false);

            foreach (var kvp in top) {
                DiscordUser u = null;
                try {
                    u = await ctx.Client.GetUserAsync(kvp.Key)
                        .ConfigureAwait(false);
                } catch (NotFoundException) {
                    u = null;
                    Shared.MessageCount.TryRemove(kvp.Key, out _);
                    // TODO remove from db
                }
                var rank = Shared.CalculateRankForMessageCount(kvp.Value);
                if (ranks.ContainsKey(rank))
                    emb.AddField(u.Username ?? "<unknown>", $"{ranks[rank]} ({rank}) ({kvp.Value} XP)");
                else
                    emb.AddField(u.Username ?? "<unknown>", $"Level {rank} ({kvp.Value} XP)");
            }

            await ctx.RespondAsync(embed: emb.Build())
                .ConfigureAwait(false);
        }
        #endregion
    }
}