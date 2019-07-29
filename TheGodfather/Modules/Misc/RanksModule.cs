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
using TheGodfather.Services;
#endregion

namespace TheGodfather.Modules.Misc
{
    [Group("rank"), Module(ModuleType.Miscellaneous), NotBlocked]
    [Description("User ranking commands. Group command prints given user's rank.")]
    [Aliases("ranks", "ranking", "level")]
    [UsageExampleArgs("@Someone")]
    [Cooldown(3, 5, CooldownBucketType.Channel)]
    public class RanksModule : TheGodfatherServiceModule<UserRanksService>
    {

        public RanksModule(UserRanksService service, SharedData shared, DatabaseContextBuilder db) 
            : base(service, shared, db)
        {
            
        }


        [GroupCommand]
        public async Task ExecuteGroupAsync(CommandContext ctx,
                                           [Description("User.")] DiscordUser user = null)
        {
            user = user ?? ctx.User;

            short rank = this.Service.CalculateRankForUser(user.Id);
            uint msgcount = this.Service.GetMessageCountForUser(user.Id);

            DatabaseGuildRank rankInfo;
            using (DatabaseContext db = this.Database.CreateContext())
                rankInfo = await db.GuildRanks.FindAsync((long)ctx.Guild.Id, rank);

            var emb = new DiscordEmbedBuilder {
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
        [UsageExampleArgs("1 Private")]
        public async Task AddAsync(CommandContext ctx,
                                  [Description("Rank.")] short rank,
                                  [RemainingText, Description("Rank name.")] string name)
        {
            if (rank < 0 || rank > 99)
                throw new CommandFailedException("You can only set rank names in range [0, 99]!");

            if (string.IsNullOrWhiteSpace(name))
                throw new CommandFailedException("Name for the rank is missing!");

            if (name.Length > 30)
                throw new CommandFailedException("Rank name cannot be longer than 30 characters!");

            using (DatabaseContext db = this.Database.CreateContext()) {
                DatabaseGuildRank dbr = db.GuildRanks.SingleOrDefault(r => r.GuildId == ctx.Guild.Id && r.Rank == rank);
                if (dbr is null) {
                    db.GuildRanks.Add(new DatabaseGuildRank {
                        GuildId = ctx.Guild.Id,
                        Name = name,
                        Rank = rank
                    });
                } else {
                    dbr.Name = name;
                }

                await db.SaveChangesAsync();
            }

            await this.InformAsync(ctx, $"Successfully added rank {Formatter.Bold(name)} as an alias for rank {Formatter.Bold(rank.ToString())}.", important: false);
        }
        #endregion

        #region COMMAND_RANK_DELETE
        [Command("delete")]
        [Description("Remove a custom name for given rank in this guild.")]
        [Aliases("-", "remove", "rm", "del", "revert")]
        [UsageExampleArgs("3")]
        [RequireUserPermissions(Permissions.ManageGuild)]
        public async Task DeleteAsync(CommandContext ctx,
                                     [Description("Rank.")] short rank)
        {
            using (DatabaseContext db = this.Database.CreateContext()) {
                db.GuildRanks.Remove(new DatabaseGuildRank {
                    GuildId = ctx.Guild.Id,
                    Rank = rank
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
                rank => $"{Formatter.InlineCode($"{rank.Rank:D2}")} | XP needed: {Formatter.InlineCode($"{this.Service.CalculateXpNeededForRank(rank.Rank):D5}")} | {Formatter.Bold(rank.Name)}",
                this.ModuleColor
            );
        }
        #endregion

        #region COMMAND_RANK_TOP
        [Command("top")]
        [Description("Get rank leaderboard.")]
        public async Task TopAsync(CommandContext ctx)
        {
            var emb = new DiscordEmbedBuilder {
                Title = "Top ranked users (globally)",
                Color = this.ModuleColor
            };

            Dictionary<short, string> ranks;
            Dictionary<ulong, uint> top;
            using (DatabaseContext db = this.Database.CreateContext()) {
                ranks = await db.GuildRanks
                    .Where(r => r.GuildId == ctx.Guild.Id)
                    .OrderBy(r => r.Rank)
                    .ToDictionaryAsync(r => r.Rank, r => r.Name);
                top = await db.MessageCount
                    .OrderByDescending(r => r.MessageCount)
                    .Take(10)
                    .ToDictionaryAsync(r => r.UserId, r => r.MessageCount);
            }

            var notFoundUsers = new List<ulong>();
            foreach ((ulong uid, uint xp) in top) {
                DiscordUser user = null;
                try {
                    user = await ctx.Client.GetUserAsync(uid);
                } catch (NotFoundException) {
                    notFoundUsers.Add(uid);
                }

                short rank = this.Service.CalculateRankForMessageCount(xp);
                if (ranks.TryGetValue(rank, out string name))
                    emb.AddField(user?.Username ?? "<unknown>", $"{name} ({rank}) ({xp} XP)");
                else
                    emb.AddField(user?.Username ?? "<unknown>", $"Level {rank} ({xp} XP)");
            }

            using (DatabaseContext db = this.Database.CreateContext()) {
                db.MessageCount.RemoveRange(notFoundUsers.Select(uid => new DatabaseMessageCount() { UserId = uid }));
                await db.SaveChangesAsync();
            }

            await ctx.RespondAsync(embed: emb.Build());
        }
        #endregion
    }
}