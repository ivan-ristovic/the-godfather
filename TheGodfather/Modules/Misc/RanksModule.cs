#region USING_DIRECTIVES
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using TheGodfather.Common.Attributes;
using TheGodfather.Database;
using TheGodfather.Database.Entities;
using TheGodfather.Exceptions;
using TheGodfather.Extensions;
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

        public RanksModule(SharedData shared, DatabaseContextBuilder db) 
            : base(shared, db)
        {
            this.ModuleColor = DiscordColor.Gold;
        }


        [GroupCommand]
        public async Task ExecuteGroupAsync(CommandContext ctx,
                                           [Description("User.")] DiscordUser user = null)
        {
            user = user ?? ctx.User;

            short rank = this.Shared.CalculateRankForUser(user.Id);
            int msgcount = this.Shared.GetMessageCountForUser(user.Id);

            DatabaseGuildRank rankInfo;
            using (DatabaseContext db = this.Database.CreateContext())
                rankInfo = await db.GuildRanks.FindAsync((long)ctx.Guild.Id, rank);

            var emb = new DiscordEmbedBuilder() {
                Title = user.Username,
                Color = this.ModuleColor,
                ThumbnailUrl = user.AvatarUrl
            };
            emb.AddField("Rank", $"{Formatter.Bold($"#{rank}")} : {Formatter.Italic(rankInfo?.Name ?? "No custom rank name set for this rank in this guild")}");
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

            using (DatabaseContext db = this.Database.CreateContext()) {
                db.GuildRanks.Add(new DatabaseGuildRank() {
                    GuildId = ctx.Guild.Id,
                    Name = name,
                    Rank = (short)rank
                });
                await db.SaveChangesAsync();
            }

            await this.InformAsync(ctx, $"Successfully added rank {Formatter.Bold(name)} as an alias for rank {Formatter.Bold(rank.ToString())}.", important: false);
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
            using (DatabaseContext db = this.Database.CreateContext()) {
                db.GuildRanks.Remove(new DatabaseGuildRank() {
                    GuildId = ctx.Guild.Id,
                    Rank = (short)rank
                });
                await db.SaveChangesAsync();
            }

            await this.InformAsync(ctx, $"Removed an alias for rank {Formatter.Bold(rank.ToString())}", important: false);
        }
        #endregion

        #region COMMAND_RANK_LIST
        [Command("list")]
        [Description("Print all customized ranks for this guild.")]
        [Aliases("levels", "ls", "l", "print")]
        [UsageExamples("!rank list")]
        public async Task RankListAsync(CommandContext ctx)
        {
            List<DatabaseGuildRank> ranks;
            using (DatabaseContext db = this.Database.CreateContext()) {
                ranks = await db.GuildRanks
                    .Where(r => r.GuildId == ctx.Guild.Id)
                    .OrderBy(r => r.Rank)
                    .ToListAsync();
            }

            if (!ranks.Any())
                throw new CommandFailedException("No custom rank names registered for this guild!");

            await ctx.SendCollectionInPagesAsync(
                "Custom ranks for this guild",
                ranks,
                rank => $"{Formatter.InlineCode($"{rank.Rank:D2}")} : | XP needed: {Formatter.InlineCode($"{this.Shared.CalculateXpNeededForRank(rank.Rank)}:D5")} | {Formatter.Bold(rank.Name)}",
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
            IEnumerable<KeyValuePair<ulong, int>> top = this.Shared.MessageCount
                .OrderByDescending(v => v.Value)
                .Take(10);

            var emb = new DiscordEmbedBuilder() {
                Title = "Top ranked users (globally)",
                Color = this.ModuleColor
            };
            
            Dictionary<short, string> ranks;
            using (DatabaseContext db = this.Database.CreateContext()) {
                ranks = await db.GuildRanks
                    .Where(r => r.GuildId == ctx.Guild.Id)
                    .OrderBy(r => r.Rank)
                    .ToDictionaryAsync(r => r.Rank, r => r.Name);
            }

            foreach ((ulong uid, int xp) in top) {
                DiscordUser user = null;
                try {
                    user = await ctx.Client.GetUserAsync(uid);
                } catch (NotFoundException) {
                    this.Shared.MessageCount.TryRemove(uid, out _);
                }

                short rank = this.Shared.CalculateRankForMessageCount(xp);
                if (ranks.TryGetValue(rank, out string name))
                    emb.AddField(user?.Username ?? "<unknown>", $"{name} ({rank}) ({xp} XP)");
                else
                    emb.AddField(user?.Username ?? "<unknown>", $"Level {rank} ({xp} XP)");
            }

            await ctx.RespondAsync(embed: emb.Build());
        }
        #endregion
    }
}