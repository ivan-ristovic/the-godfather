#region USING_DIRECTIVES
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TheGodfather.Common;
using TheGodfather.Common.Attributes;
using TheGodfather.Exceptions;
using TheGodfather.Services;
using TheGodfather.Services.Database;
#endregion

namespace TheGodfather.Modules.Misc
{
    [Group("random"), Module(ModuleType.Miscellaneous), NotBlocked]
    [Description("Random gibberish.")]
    [Aliases("rnd", "rand")]
    [Cooldown(3, 5, CooldownBucketType.Channel)]
    public class RandomModule : TheGodfatherModule
    {
        
        public RandomModule(SharedData shared, DBService db)
            : base(shared, db)
        {
            this.ModuleColor = DiscordColor.HotPink;
        }


        #region COMMAND_CAT
        [Command("cat")]
        [Description("Get a random cat image.")]
        [UsageExamples("!random cat")]
        public async Task RandomCatAsync(CommandContext ctx)
        {
            string url = await PetImagesService.GetRandomCatImageAsync();
            if (url == null)
                throw new CommandFailedException("Connection to random.cat failed!");

            await ctx.RespondAsync(embed: new DiscordEmbedBuilder() {
                Description = DiscordEmoji.FromName(ctx.Client, ":cat:"),
                ImageUrl = url,
                Color = this.ModuleColor
            });
        }
        #endregion

        #region COMMAND_DOG
        [Command("dog")]
        [Description("Get a random dog image.")]
        [UsageExamples("!random dog")]
        public async Task RandomDogAsync(CommandContext ctx)
        {
            string url = await PetImagesService.GetRandomDogImageAsync();
            if (url == null)
                throw new CommandFailedException("Connection to random.dog failed!");

            await ctx.RespondAsync(embed: new DiscordEmbedBuilder() {
                Description = DiscordEmoji.FromName(ctx.Client, ":dog:"),
                ImageUrl = url,
                Color = this.ModuleColor
            });
        }
        #endregion

        #region COMMAND_CHOOSE
        [Command("choose")]
        [Description("Choose one of the provided options separated by comma.")]
        [Aliases("select")]
        [UsageExamples("!random choose option 1, option 2, option 3...")]
        public Task ChooseAsync(CommandContext ctx,
                               [RemainingText, Description("Option list (comma separated).")] string list)
        {
            if (string.IsNullOrWhiteSpace(list))
                throw new InvalidCommandUsageException("Missing list to choose from.");

            IEnumerable<string> options = list.Split(',')
                .Select(s => s.Trim())
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .Distinct();

            return InformAsync(ctx, options.ElementAt(GFRandom.Generator.Next(options.Count())), ":arrow_right:");
        }
        #endregion

        #region COMMAND_RAFFLE
        [Command("raffle")]
        [Description("Choose a user from the online members list optionally belonging to a given role.")]
        [Aliases("chooseuser")]
        [UsageExamples("!random raffle",
                       "!random raffle Admins")]
        public Task RaffleAsync(CommandContext ctx,
                               [Description("Role.")] DiscordRole role = null)
        {
            IEnumerable<DiscordMember> online = ctx.Guild.Members
                .Where(m => m.Presence != null && m.Presence.Status != UserStatus.Offline);

            if (role != null)
                online = online.Where(m => m.Roles.Any(r => r.Id == role.Id));

            if (online.Count() == 0)
                throw new CommandFailedException("There are no memebers that meet the given criteria.");

            DiscordMember raffled = online.ElementAt(GFRandom.Generator.Next(online.Count()));
            return InformAsync(ctx, StaticDiscordEmoji.Dice, $"Raffled: {raffled.Mention}");
        }
        #endregion
    }
}
