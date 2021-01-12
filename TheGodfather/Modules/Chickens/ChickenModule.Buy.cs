using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using Humanizer;
using Microsoft.Extensions.DependencyInjection;
using TheGodfather.Attributes;
using TheGodfather.Common;
using TheGodfather.Database.Models;
using TheGodfather.Exceptions;
using TheGodfather.Extensions;
using TheGodfather.Modules.Administration.Services;
using TheGodfather.Modules.Chickens.Services;
using TheGodfather.Modules.Currency.Services;
using TheGodfather.Services.Common;

namespace TheGodfather.Modules.Chickens
{
    public partial class ChickenModule
    {
        [Group("buy"), UsesInteractivity]
        [Aliases("b", "shop")]
        public sealed class BuyModule : TheGodfatherServiceModule<ChickenService>
        {
            #region chicken buy
            [GroupCommand]
            public Task ExecuteGroupAsync(CommandContext ctx,
                                         [RemainingText, Description("desc-chicken-name")] string name)
                => this.TryBuyInternalAsync(ctx, ChickenType.Default, name);
            #endregion

            #region chicken buy default
            [Command("default")]
            [Aliases("d", "def")]
            public Task DefaultAsync(CommandContext ctx,
                                    [RemainingText, Description("desc-chicken-name")] string name)
                => this.TryBuyInternalAsync(ctx, ChickenType.Default, name);
            #endregion

            #region chicken buy wellfed
            [Command("wellfed")]
            [Aliases("wf", "fed")]
            public Task WellFedAsync(CommandContext ctx,
                                    [RemainingText, Description("desc-chicken-name")] string name)
                => this.TryBuyInternalAsync(ctx, ChickenType.WellFed, name);
            #endregion

            #region chicken buy trained
            [Command("trained")]
            [Aliases("tr", "train")]
            public Task TrainedAsync(CommandContext ctx,
                                    [RemainingText, Description("desc-chicken-name")] string name)
                => this.TryBuyInternalAsync(ctx, ChickenType.Trained, name);
            #endregion

            #region chicken buy steroidempowered
            [Command("steroidempowered")]
            [Aliases("s", "steroid", "empowered")]

            public Task EmpoweredAsync(CommandContext ctx,
                                      [RemainingText, Description("desc-chicken-name")] string name)
                => this.TryBuyInternalAsync(ctx, ChickenType.SteroidEmpowered, name);
            #endregion

            #region chicken buy alien
            [Command("alien")]
            [Aliases("a", "extraterrestrial")]
            public Task AlienAsync(CommandContext ctx,
                                  [RemainingText, Description("desc-chicken-name")] string name)
                => this.TryBuyInternalAsync(ctx, ChickenType.Alien, name);
            #endregion

            #region chicken buy list
            [Command("list")]
            [Aliases("ls", "view")]
            public Task ListAsync(CommandContext ctx)
            {
                CachedGuildConfig gcfg = ctx.Services.GetRequiredService<GuildConfigService>().GetCachedConfig(ctx.Guild.Id);
                return ctx.RespondWithLocalizedEmbedAsync(emb => {
                    emb.WithLocalizedTitle("str-chicken-types");
                    emb.WithColor(this.ModuleColor);
                    foreach (((ChickenType type, ChickenStats stats), int i) in Chicken.StartingStats.Select((kvp, i) => (kvp, i)))
                        emb.AddLocalizedTitleField($"str-chicken-type-{i}", stats, titleArgs: new object[] { Chicken.Price(type), gcfg.Currency });
                });
            }
            #endregion


            #region internals
            private async Task TryBuyInternalAsync(CommandContext ctx, ChickenType type, string name)
            {
                if (string.IsNullOrWhiteSpace(name))
                    throw new InvalidCommandUsageException(ctx, "cmd-err-missing-name");

                if (name.Length > Chicken.NameLimit)
                    throw new InvalidCommandUsageException(ctx, "cmd-err-name", Chicken.NameLimit);

                if (!name.All(c => char.IsLetterOrDigit(c) || char.IsWhiteSpace(c)))
                    throw new InvalidCommandUsageException(ctx, "cmd-err-name-alnum");

                if (await this.Service.ContainsAsync(ctx.Guild.Id, ctx.User.Id))
                    throw new CommandFailedException(ctx, "cmd-err-chicken-dup");

                CachedGuildConfig gcfg = ctx.Services.GetRequiredService<GuildConfigService>().GetCachedConfig(ctx.Guild.Id);

                long price = Chicken.Price(type);
                if (!await ctx.WaitForBoolReplyAsync("q-chicken-buy", args: new object[] { ctx.User.Mention, price, gcfg.Currency }))
                    return;

                if (!await ctx.Services.GetRequiredService<BankAccountService>().TryDecreaseBankAccountAsync(ctx.Guild.Id, ctx.User.Id, price))
                    throw new CommandFailedException(ctx, "cmd-err-funds", gcfg.Currency, price);

                await this.Service.AddAsync(new Chicken(type) {
                    GuildId = ctx.Guild.Id,
                    Name = name,
                    UserId = ctx.User.Id,
                });

                await ctx.ImpInfoAsync(this.ModuleColor, Emojis.Chicken, "fmt-chicken-buy", ctx.User.Mention, type.Humanize(LetterCasing.LowerCase), name);
            }
            #endregion
        }
    }
}
