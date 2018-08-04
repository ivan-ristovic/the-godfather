#region USING_DIRECTIVES
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TheGodfather.Common.Attributes;
using TheGodfather.Exceptions;
using TheGodfather.Extensions;
using TheGodfather.Services.Database;
using TheGodfather.Services.Database.Ranks;
#endregion

namespace TheGodfather.Modules.Misc
{
    [Group("rank"), Module(ModuleType.Miscellaneous), NotBlocked]
    [Description("User ranking commands. Group command prints given user's rank.")]
    [Aliases("ranks", "ranking", "level")]
    [UsageExamples("!rank",
                   "!rank @Someone")]
    [Cooldown(3, 5, CooldownBucketType.Channel)]
    public class RanksModule : TheGodfatherModule
    {

        public RanksModule(SharedData shared, DBService db) 
            : base(shared, db)
        {
            this.ModuleColor = DiscordColor.Gold;
        }


        [GroupCommand]
        public async Task ExecuteGroupAsync(CommandContext ctx,
                                           [Description("User.")] DiscordUser user = null)
        {
            if (user == null)
                user = ctx.User;

            ushort rank = this.Shared.CalculateRankForUser(user.Id);
            ulong msgcount = this.Shared.GetMessageCountForUser(user.Id);
            string rankName = await this.Database.GetRankAsync(ctx.Guild.Id, rank);

            var emb = new DiscordEmbedBuilder() {
                Title = user.Username,
                Color = this.ModuleColor,
                ThumbnailUrl = user.AvatarUrl
            };
            emb.AddField("Rank", $"{Formatter.Bold($"#{rank}")} : {Formatter.Italic(rankName ?? "No custom rank name set for this rank in this guild")}");
            emb.AddField("XP", $"{msgcount}", inline: true);
            emb.AddField("XP needed for next rank", $"{(rank + 1) * (rank + 1) * 10}", inline: true);

            await ctx.RespondAsync(embed: emb.Build());
        }


        #region COMMAND_RANK_ADD
        [Command("add"), Priority(1)]
        [Description("Add a custom name for given rank in this guild.")]
        [Aliases("+", "a", "rename", "rn", "newname", "<", "<<", "+=")]
        [RequireUserPermissions(Permissions.ManageGuild)]
        [UsageExamples("!rank add 1 Private")]
        public async Task AddAsync(CommandContext ctx,
                                  [Description("Rank.")] int rank,
                                  [RemainingText, Description("Rank name.")] string name)
        {
            if (rank < 0 || rank > 99)
                throw new CommandFailedException("You can only set rank names in range [0, 99]!");

            if (string.IsNullOrWhiteSpace(name))
                throw new CommandFailedException("Name for the rank is missing!");

            if (name.Length > 30)
                throw new CommandFailedException("Rank name cannot be longer than 30 characters!");

            await this.Database.AddOrUpdateRankAsync(ctx.Guild.Id, rank, name);
            await InformAsync(ctx, $"Successfully added rank {Formatter.Bold(name)} as an alias for rank {Formatter.Bold(rank.ToString())}.", important: false);
        }
        #endregion

        #region COMMAND_RANK_DELETE
        [Command("delete")]
        [Description("Remove a custom name for given rank in this guild.")]
        [Aliases("-", "remove", "rm", "del", "revert")]
        [UsageExamples("!rank delete 3")]
        [RequireUserPermissions(Permissions.ManageGuild)]
        public async Task DeleteAsync(CommandContext ctx,
                                     [Description("Rank.")] int rank)
        {
            await this.Database.RemoveRankAsync(ctx.Guild.Id, rank);
            await InformAsync(ctx, $"Removed an alias for rank {Formatter.Bold(rank.ToString())}", important: false);
        }
        #endregion

        #region COMMAND_RANK_LIST
        [Command("list")]
        [Description("Print all customized ranks for this guild.")]
        [Aliases("levels", "ls", "l", "print")]
        [UsageExamples("!rank list")]
        public async Task RankListAsync(CommandContext ctx)
        {
            IReadOnlyDictionary<ushort, string> ranks = await this.Database.GetAllRanksAsync(ctx.Guild.Id);
            if (!ranks.Any())
                throw new CommandFailedException("No custom rank names registered for this guild!");

            await ctx.SendCollectionInPagesAsync(
                "Custom ranks for this guild",
                ranks,
                kvp => $"{Formatter.InlineCode($"{kvp.Key:D2}")} : | XP needed: {Formatter.InlineCode($"this.Shared.CalculateXpNeededForRank(kvp.Key):D5")} | {Formatter.Bold(kvp.Value)}",
                this.ModuleColor
            );
        }
        #endregion

        #region COMMAND_RANK_TOP
        [Command("top")]
        [Description("Get rank leaderboard.")]
        [UsageExamples("!rank top")]
        public async Task TopAsync(CommandContext ctx)
        {
            IEnumerable<KeyValuePair<ulong, ulong>> top = this.Shared.MessageCount
                .OrderByDescending(v => v.Value)
                .Take(10);

            var emb = new DiscordEmbedBuilder() {
                Title = "Top ranked users (globally)",
                Color = this.ModuleColor
            };

            IReadOnlyDictionary<ushort, string> ranks = await this.Database.GetAllRanksAsync(ctx.Guild.Id);

            foreach ((ulong uid, ulong xp) in top) {
                DiscordUser user = null;
                try {
                    user = await ctx.Client.GetUserAsync(uid);
                } catch (NotFoundException) {
                    user = null;
                    this.Shared.MessageCount.TryRemove(uid, out _);
                    await this.Database.RemoveUserXpAsync(user.Id);
                }

                ushort rank = this.Shared.CalculateRankForMessageCount(xp);
                if (ranks.ContainsKey(rank))
                    emb.AddField(user.Username ?? "<unknown>", $"{ranks[rank]} ({rank}) ({xp} XP)");
                else
                    emb.AddField(user.Username ?? "<unknown>", $"Level {rank} ({xp} XP)");
            }

            await ctx.RespondAsync(embed: emb.Build());
        }
        #endregion
    }
}