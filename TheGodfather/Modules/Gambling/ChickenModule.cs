#region USING_DIRECTIVES
using System.Text;
using System.Threading.Tasks;

using TheGodfather.Common.Attributes;
using TheGodfather.Exceptions;
using TheGodfather.Extensions;
using TheGodfather.Services;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
#endregion

namespace TheGodfather.Modules.Gambling
{
    [Group("chicken"), Module(ModuleType.Gambling)]
    [Description("Manage your chicken. If invoked without subcommands, prints out your chicken information.")]
    [Aliases("cock", "hen", "chick")]
    [Cooldown(3, 5, CooldownBucketType.Channel)]
    [UsageExample("!chicken")]
    [UsageExample("!chicken @Someone")]
    [ListeningCheck]
    public class ChickenModule : TheGodfatherBaseModule
    {

        public ChickenModule(DBService db) : base(db: db) { }


        [GroupCommand]
        public Task ExecuteGroupAsync(CommandContext ctx,
                                     [Description("User.")] DiscordUser user = null)
            => InfoAsync(ctx, user);



        #region COMMAND_CHICKEN_INFO
        [Command("info"), Module(ModuleType.Gambling)]
        [Description("View user's chicken info. If the user is not given, views sender's chicken info.")]
        [Aliases("information", "stats")]
        [UsageExample("!chicken info @Someone")]
        public async Task InfoAsync(CommandContext ctx,
                                   [Description("User.")] DiscordUser user = null)
        {
            if (user == null)
                user = ctx.User;

            var chicken = await Database.GetChickenInfoAsync(user.Id)
                .ConfigureAwait(false);
            if (chicken == null)
                throw new CommandFailedException($"User {user.Mention} does not own a chicken! Use command {Formatter.InlineCode("chicken buy")} to buy a chicken (1000 credits).");

            DiscordUser owner = null;
            try {
                owner = await ctx.Client.GetUserAsync(chicken.OwnerId)
                    .ConfigureAwait(false);
            } catch {
                await Database.RemoveChickenAsync(chicken.OwnerId)
                    .ConfigureAwait(false);
                throw new CommandFailedException($"User {user.Mention} does not own a chicken! Use command {Formatter.InlineCode("chicken buy")} to buy a chicken (1000 credits).");
            }

            await ctx.RespondAsync(embed: chicken.Embed(owner))
                .ConfigureAwait(false);
        }
        #endregion


        // SO MANY IDEAS WTF IS THIS BRAINSTORM???
        // chicken buy
        // chicken sell - estimate price
        // chicken fight
        // chicken train
        // chicken stats - strength, agility, hitpoints
        // chicken upgrades - weapons, armor etc
    }
}
