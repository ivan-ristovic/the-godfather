#region USING_DIRECTIVES
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using System.Linq;
using System.Threading.Tasks;

using TheGodfather.Common;
using TheGodfather.Common.Attributes;
using TheGodfather.Database;
using TheGodfather.Database.Entities;
using TheGodfather.Exceptions;
using TheGodfather.Extensions;
using TheGodfather.Modules.Chickens.Common;
using TheGodfather.Modules.Currency.Extensions;
#endregion

namespace TheGodfather.Modules.Chickens
{
    public partial class ChickenModule
    {
        [Group("buy"), UsesInteractivity]
        [Description("Buy a new chicken in this guild using your credits from WM bank.")]
        [Aliases("b", "shop")]
        [UsageExamples("!chicken buy My Chicken Name")]
        public class BuyModule : TheGodfatherModule
        {

            public BuyModule(SharedData shared, DatabaseContextBuilder db)
                : base(shared, db)
            {
                this.ModuleColor = DiscordColor.Yellow;
            }


            [GroupCommand]
            public Task ExecuteGroupAsync(CommandContext ctx,
                                         [RemainingText, Description("Chicken name.")] string name)
                => this.TryBuyInternalAsync(ctx, ChickenType.Default, name);


            #region COMMAND_CHICKEN_BUY_DEFAULT
            [Command("default")]
            [Description("Buy a chicken of default strength (cheapest).")]
            [Aliases("d", "def")]
            [UsageExamples("!chicken buy default My Chicken Name")]
            public Task DefaultAsync(CommandContext ctx,
                                    [RemainingText, Description("Chicken name.")] string name)
                => this.TryBuyInternalAsync(ctx, ChickenType.Default, name);
            #endregion

            #region COMMAND_CHICKEN_BUY_WELLFED
            [Command("wellfed")]
            [Description("Buy a well-fed chicken.")]
            [Aliases("wf", "fed")]
            [UsageExamples("!chicken buy wellfed My Chicken Name")]
            public Task WellFedAsync(CommandContext ctx,
                                    [RemainingText, Description("Chicken name.")] string name)
                => this.TryBuyInternalAsync(ctx, ChickenType.WellFed, name);
            #endregion

            #region COMMAND_CHICKEN_BUY_TRAINED
            [Command("trained")]
            [Description("Buy a trained chicken.")]
            [Aliases("tr", "train")]
            [UsageExamples("!chicken buy trained My Chicken Name")]
            public Task TrainedAsync(CommandContext ctx,
                                    [RemainingText, Description("Chicken name.")] string name)
                => this.TryBuyInternalAsync(ctx, ChickenType.Trained, name);
            #endregion

            #region COMMAND_CHICKEN_BUY_EMPOWERED
            [Command("steroidempowered")]
            [Description("Buy a steroid-empowered chicken.")]
            [Aliases("steroid", "empowered")]
            [UsageExamples("!chicken buy steroidempowered My Chicken Name")]
            public Task EmpoweredAsync(CommandContext ctx,
                                      [RemainingText, Description("Chicken name.")] string name)
                => this.TryBuyInternalAsync(ctx, ChickenType.SteroidEmpowered, name);
            #endregion

            #region COMMAND_CHICKEN_BUY_ALIEN
            [Command("alien")]
            [Description("Buy an alien chicken.")]
            [Aliases("a", "extraterrestrial")]
            [UsageExamples("!chicken buy alien My Chicken Name")]
            public Task AlienAsync(CommandContext ctx,
                                  [RemainingText, Description("Chicken name.")] string name)
                => this.TryBuyInternalAsync(ctx, ChickenType.Alien, name);
            #endregion

            #region COMMAND_CHICKEN_BUY_LIST
            [Command("list")]
            [Description("List all available chicken types.")]
            [Aliases("ls", "view")]
            [UsageExamples("!chicken buy list")]
            public Task ListAsync(CommandContext ctx)
            {
                var emb = new DiscordEmbedBuilder() {
                    Title = "Available chicken types",
                    Color = this.ModuleColor
                };

                emb.AddField($"Default ({Chicken.Price(ChickenType.Default)} {this.Shared.GetGuildConfig(ctx.Guild.Id).Currency ?? "credits"})", Chicken.StartingStats[ChickenType.Default].ToString());
                emb.AddField($"Well-Fed ({Chicken.Price(ChickenType.WellFed)} {this.Shared.GetGuildConfig(ctx.Guild.Id).Currency ?? "credits"})", Chicken.StartingStats[ChickenType.WellFed].ToString());
                emb.AddField($"Trained ({Chicken.Price(ChickenType.Trained)} {this.Shared.GetGuildConfig(ctx.Guild.Id).Currency ?? "credits"})", Chicken.StartingStats[ChickenType.Trained].ToString());
                emb.AddField($"Steroid Empowered ({Chicken.Price(ChickenType.SteroidEmpowered)} {this.Shared.GetGuildConfig(ctx.Guild.Id).Currency ?? "credits"})", Chicken.StartingStats[ChickenType.SteroidEmpowered].ToString());
                emb.AddField($"Alien ({Chicken.Price(ChickenType.Alien)} {this.Shared.GetGuildConfig(ctx.Guild.Id).Currency ?? "credits"})", Chicken.StartingStats[ChickenType.Alien].ToString());

                return ctx.RespondAsync(embed: emb.Build());
            }
            #endregion


            #region HELPER_FUNCTIONS
            private async Task TryBuyInternalAsync(CommandContext ctx, ChickenType type, string name)
            {
                if (string.IsNullOrWhiteSpace(name))
                    throw new InvalidCommandUsageException("Name for your chicken is missing.");

                if (name.Length < 2 || name.Length > 30)
                    throw new InvalidCommandUsageException("Name cannot be shorter than 2 and longer than 30 characters.");

                if (!name.All(c => char.IsLetterOrDigit(c) || char.IsWhiteSpace(c)))
                    throw new InvalidCommandUsageException("Name cannot contain characters that are not letters or digits.");
                
                if (!(Chicken.FromDatabase(this.Database, ctx.Guild.Id, ctx.User.Id) is null))
                    throw new CommandFailedException("You already own a chicken!");

                if (!await ctx.WaitForBoolReplyAsync($"{ctx.User.Mention}, are you sure you want to buy a chicken for {Formatter.Bold(Chicken.Price(type).ToString())} {this.Shared.GetGuildConfig(ctx.Guild.Id).Currency ?? "credits"}?"))
                    return;
                
                using (DatabaseContext db = this.Database.CreateContext()) {
                    if (!await db.TryDecreaseBankAccountAsync(ctx.User.Id, ctx.Guild.Id, Chicken.Price(type))) 
                        throw new CommandFailedException($"You do not have enough {this.Shared.GetGuildConfig(ctx.Guild.Id).Currency ?? "credits"} to buy a chicken ({Chicken.Price(type)} needed)!");

                    var stats = Chicken.StartingStats[type];
                    db.Chickens.Add(new DatabaseChicken() {
                        GuildId = ctx.Guild.Id,
                        MaxVitality = stats.BareMaxVitality,
                        Name = name,
                        Strength = stats.BareStrength,
                        UserId = ctx.User.Id,
                        Vitality = stats.BareVitality
                    });

                    await db.SaveChangesAsync();
                }
                    
                await this.InformAsync(ctx, StaticDiscordEmoji.Chicken, $"{ctx.User.Mention} bought a chicken named {Formatter.Bold(name)}");
            }
            #endregion
        }
    }
}
