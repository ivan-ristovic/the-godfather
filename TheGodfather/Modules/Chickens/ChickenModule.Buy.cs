#region USING_DIRECTIVES
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Microsoft.Extensions.DependencyInjection;
using TheGodfather.Attributes;
using TheGodfather.Common;
using TheGodfather.Database;
using TheGodfather.Database.Models;
using TheGodfather.Exceptions;
using TheGodfather.Extensions;
using TheGodfather.Modules.Administration.Services;
using TheGodfather.Modules.Chickens.Extensions;
using TheGodfather.Modules.Currency.Extensions;
using TheGodfather.Services.Common;
#endregion

namespace TheGodfather.Modules.Chickens
{
    //public partial class ChickenModule
    //{
    //    [Group("buy"), UsesInteractivity]
    //    [Description("Buy a new chicken in this guild using your credits from WM bank.")]
    //    [Aliases("b", "shop")]

    //    public class BuyModule : TheGodfatherModule
    //    {

    //        public BuyModule(DbContextBuilder db)
    //            : base(db)
    //        {

    //        }


    //        [GroupCommand]
    //        public Task ExecuteGroupAsync(CommandContext ctx,
    //                                     [RemainingText, Description("Chicken name.")] string name)
    //            => this.TryBuyInternalAsync(ctx, ChickenType.Default, name);


    //        #region COMMAND_CHICKEN_BUY_DEFAULT
    //        [Command("default")]
    //        [Description("Buy a chicken of default strength (cheapest).")]
    //        [Aliases("d", "def")]

    //        public Task DefaultAsync(CommandContext ctx,
    //                                [RemainingText, Description("Chicken name.")] string name)
    //            => this.TryBuyInternalAsync(ctx, ChickenType.Default, name);
    //        #endregion

    //        #region COMMAND_CHICKEN_BUY_WELLFED
    //        [Command("wellfed")]
    //        [Description("Buy a well-fed chicken.")]
    //        [Aliases("wf", "fed")]

    //        public Task WellFedAsync(CommandContext ctx,
    //                                [RemainingText, Description("Chicken name.")] string name)
    //            => this.TryBuyInternalAsync(ctx, ChickenType.WellFed, name);
    //        #endregion

    //        #region COMMAND_CHICKEN_BUY_TRAINED
    //        [Command("trained")]
    //        [Description("Buy a trained chicken.")]
    //        [Aliases("tr", "train")]

    //        public Task TrainedAsync(CommandContext ctx,
    //                                [RemainingText, Description("Chicken name.")] string name)
    //            => this.TryBuyInternalAsync(ctx, ChickenType.Trained, name);
    //        #endregion

    //        #region COMMAND_CHICKEN_BUY_EMPOWERED
    //        [Command("steroidempowered")]
    //        [Description("Buy a steroid-empowered chicken.")]
    //        [Aliases("steroid", "empowered")]

    //        public Task EmpoweredAsync(CommandContext ctx,
    //                                  [RemainingText, Description("Chicken name.")] string name)
    //            => this.TryBuyInternalAsync(ctx, ChickenType.SteroidEmpowered, name);
    //        #endregion

    //        #region COMMAND_CHICKEN_BUY_ALIEN
    //        [Command("alien")]
    //        [Description("Buy an alien chicken.")]
    //        [Aliases("a", "extraterrestrial")]

    //        public Task AlienAsync(CommandContext ctx,
    //                              [RemainingText, Description("Chicken name.")] string name)
    //            => this.TryBuyInternalAsync(ctx, ChickenType.Alien, name);
    //        #endregion

    //        #region COMMAND_CHICKEN_BUY_LIST
    //        [Command("list")]
    //        [Description("List all available chicken types.")]
    //        [Aliases("ls", "view")]
    //        public Task ListAsync(CommandContext ctx)
    //        {
    //            var emb = new DiscordEmbedBuilder {
    //                Title = "Available chicken types",
    //                Color = this.ModuleColor
    //            };

    //            CachedGuildConfig gcfg = ctx.Services.GetService<GuildConfigService>().GetCachedConfig(ctx.Guild.Id);

    //            emb.AddField($"Default ({Chicken.Price(ChickenType.Default)} {gcfg.Currency})", Chicken.StartingStats[ChickenType.Default].ToString());
    //            emb.AddField($"Well-Fed ({Chicken.Price(ChickenType.WellFed)} {gcfg.Currency})", Chicken.StartingStats[ChickenType.WellFed].ToString());
    //            emb.AddField($"Trained ({Chicken.Price(ChickenType.Trained)} {gcfg.Currency})", Chicken.StartingStats[ChickenType.Trained].ToString());
    //            emb.AddField($"Steroid Empowered ({Chicken.Price(ChickenType.SteroidEmpowered)} {gcfg.Currency})", Chicken.StartingStats[ChickenType.SteroidEmpowered].ToString());
    //            emb.AddField($"Alien ({Chicken.Price(ChickenType.Alien)} {gcfg.Currency})", Chicken.StartingStats[ChickenType.Alien].ToString());

    //            return ctx.RespondAsync(embed: emb.Build());
    //        }
    //        #endregion


    //        #region HELPER_FUNCTIONS
    //        private async Task TryBuyInternalAsync(CommandContext ctx, ChickenType type, string name)
    //        {
    //            if (string.IsNullOrWhiteSpace(name))
    //                throw new InvalidCommandUsageException(ctx, "cmd-err-missing-name");

    //            if (name.Length > Chicken.NameLimit)
    //                throw new InvalidCommandUsageException(ctx, "cmd-err-name", Chicken.NameLimit);

    //            if (!name.All(c => char.IsLetterOrDigit(c) || char.IsWhiteSpace(c)))
    //                throw new InvalidCommandUsageException(ctx, "cmd-err-name-alnum");

    //            if (await this.Service.GetAndSetOwnerAsync(ctx.Client, ctx.Guild.Id, ctx.User.Id) is { })
    //                throw new CommandFailedException("You already own a chicken!");

    //            CachedGuildConfig gcfg = ctx.Services.GetService<GuildConfigService>().GetCachedConfig(ctx.Guild.Id);

    //            if (!await ctx.WaitForBoolReplyAsync($"{ctx.User.Mention}, are you sure you want to buy a chicken for {Formatter.Bold(Chicken.Price(type).ToString())} {gcfg.Currency}?"))
    //                return;

    //            using (TheGodfatherDbContext db = this.Database.CreateContext()) {
    //                if (!await db.TryDecreaseBankAccountAsync(ctx.User.Id, ctx.Guild.Id, Chicken.Price(type)))
    //                    throw new CommandFailedException($"You do not have enough {gcfg.Currency} to buy a chicken ({Chicken.Price(type)} needed)!");

    //                db.Chickens.Add(new Chicken(type) {
    //                    GuildId = ctx.Guild.Id,
    //                    Name = name,
    //                    UserId = ctx.User.Id,
    //                });

    //                await db.SaveChangesAsync();
    //            }

    //            await this.InformAsync(ctx, Emojis.Chicken, $"{ctx.User.Mention} bought a chicken named {Formatter.Bold(name)}");
    //        }
    //        #endregion
    //    }
    //}
}
