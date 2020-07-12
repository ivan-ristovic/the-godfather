#region USING_DIRECTIVES
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;

using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

using TheGodfather.Common;
using TheGodfather.Common.Attributes;
using TheGodfather.Database;
using TheGodfather.Modules.Currency.Common;
using TheGodfather.Modules.Currency.Extensions;
using TheGodfather.Modules.Administration.Services;
#endregion

namespace TheGodfather.Modules.Currency
{
    [Module(ModuleType.Currency), NotBlocked]
    public class WorkModule : TheGodfatherModule
    {

        public WorkModule(DbContextBuilder db)
            : base(db)
        {
            
        }


        #region COMMAND_SLUT
        [Command("slut")]
        [Description("Work the streets tonight hoping to gather some easy money but beware, there are many threats lurking at that hour. You can work the streets once per 5s.")]
        [Cooldown(1, 5, CooldownBucketType.User)]
        public async Task StreetsAsync(CommandContext ctx)
        {
            int change = GFRandom.Generator.NextBool() ? GFRandom.Generator.Next(1000, 5000) : -GFRandom.Generator.Next(5, 2500);
            using (TheGodfatherDbContext db = this.Database.CreateDbContext()) {
                await db.ModifyBankAccountAsync(ctx.User.Id, ctx.Guild.Id, b => b + change);
                await db.SaveChangesAsync();
            }

            await ctx.RespondAsync(embed: new DiscordEmbedBuilder {
                Description = $"{ctx.Member.Mention} {WorkHandler.GetWorkStreetsString(change, ctx.Services.GetService<GuildConfigService>().GetCachedConfig(ctx.Guild.Id).Currency)}",
                Color = change > 0 ? DiscordColor.PhthaloGreen : DiscordColor.IndianRed
            }.WithFooter("Who needs dignity?", ctx.Member.AvatarUrl).Build());
        }
        #endregion

        #region COMMAND_WORK
        [Command("work")]
        [Description("Do something productive with your life. You can work once per minute.")]
        [Aliases("job")]
        [Cooldown(1, 60, CooldownBucketType.User)]
        public async Task WorkAsync(CommandContext ctx)
        {
            int earned = GFRandom.Generator.Next(1000) + 1;
            using (TheGodfatherDbContext db = this.Database.CreateDbContext()) {
                await db.ModifyBankAccountAsync(ctx.User.Id, ctx.Guild.Id, b => b + earned);
                await db.SaveChangesAsync();
            }

            await ctx.RespondAsync(embed: new DiscordEmbedBuilder {
                Description = $"{ctx.Member.Mention} {WorkHandler.GetWorkString(earned, ctx.Services.GetService<GuildConfigService>().GetCachedConfig(ctx.Guild.Id).Currency)}",
                Color = DiscordColor.SapGreen
            }.WithFooter("Arbeit macht frei.", ctx.Member.AvatarUrl).Build());
        }
        #endregion

        #region COMMAND_CRIME
        [Command("crime")]
        [Description("Commit a crime and hope to get away with large amounts of cash. You can attempt to commit a crime once every 10 minutes.")]
        [Cooldown(1, 600, CooldownBucketType.User)]
        public async Task CrimeAsync(CommandContext ctx)
        {
            bool success = GFRandom.Generator.Next(10) == 0;
            int earned = success ? GFRandom.Generator.Next(10000, 100000) : -10000;
            using (TheGodfatherDbContext db = this.Database.CreateDbContext()) {
                await db.ModifyBankAccountAsync(ctx.User.Id, ctx.Guild.Id, b => b + earned);
                await db.SaveChangesAsync();
            }

            await ctx.RespondAsync(embed: new DiscordEmbedBuilder {
                Description = $"{ctx.Member.Mention} {WorkHandler.GetCrimeString(earned, ctx.Services.GetService<GuildConfigService>().GetCachedConfig(ctx.Guild.Id).Currency)}",
                Color = success ? DiscordColor.SapGreen : DiscordColor.IndianRed
            }.WithFooter("PAYDAY3", ctx.Member.AvatarUrl).Build());
        }
        #endregion
    }
}