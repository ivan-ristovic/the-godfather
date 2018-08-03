#region USING_DIRECTIVES
using System;
using System.Linq;
using System.Threading.Tasks;

using TheGodfather.Common;
using TheGodfather.Common.Attributes;
using TheGodfather.Exceptions;
using TheGodfather.Extensions;
using TheGodfather.Services;

using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
#endregion

namespace TheGodfather.Modules.Misc
{
    [Group("random"), Module(ModuleType.Miscellaneous)]
    [Description("Random gibberish.")]
    [Aliases("rnd", "rand")]
    [Cooldown(3, 5, CooldownBucketType.Channel)]
    [NotBlocked]
    public class CommandsRandomGroup : TheGodfatherModule
    {
        #region COMMAND_CAT
        [Command("cat"), Module(ModuleType.Miscellaneous)]
        [Description("Get a random cat image.")]
        [UsageExamples("!random cat")]
        public async Task RandomCatAsync(CommandContext ctx)
        {
            string url = await PetImagesService.GetRandomCatImageAsync()
                .ConfigureAwait(false);
            if (url == null)
                throw new CommandFailedException("Connection to random.cat failed!");

            await ctx.RespondAsync(embed: new DiscordEmbedBuilder() {
                Description = DiscordEmoji.FromName(ctx.Client, ":cat:"),
                ImageUrl = url
            }).ConfigureAwait(false);
        }
        #endregion

        #region COMMAND_DOG
        [Command("dog"), Module(ModuleType.Miscellaneous)]
        [Description("Get a random dog image.")]
        [UsageExamples("!random dog")]
        public async Task RandomDogAsync(CommandContext ctx)
        {
            string url = await PetImagesService.GetRandomDogImageAsync()
                .ConfigureAwait(false);
            if (url == null)
                throw new CommandFailedException("Connection to random.dog failed!");

            await ctx.RespondAsync(embed: new DiscordEmbedBuilder() {
                Description = DiscordEmoji.FromName(ctx.Client, ":dog:"),
                ImageUrl = url
            }).ConfigureAwait(false);
        }
        #endregion

        #region COMMAND_CHOOSE
        [Command("choose"), Module(ModuleType.Miscellaneous)]
        [Description("Choose one of the provided options separated by comma.")]
        [Aliases("select")]
        [UsageExamples("!random choose option 1, option 2, option 3...")]
        public async Task ChooseAsync(CommandContext ctx,
                                     [RemainingText, Description("Option list (separated by comma).")] string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                throw new InvalidCommandUsageException("Missing list to choose from.");

            var options = text.Split(',')
                              .Distinct()
                              .Select(s => s.Trim())
                              .Where(s => !string.IsNullOrWhiteSpace(s));
            await InformAsync(ctx, options.ElementAt(GFRandom.Generator.Next(options.Count())), ":arrow_right:")
                .ConfigureAwait(false);
        }
        #endregion

        #region COMMAND_RAFFLE
        [Command("raffle"), Module(ModuleType.Miscellaneous)]
        [Description("Choose a user from the online members list belonging to a given role.")]
        [Aliases("chooseuser")]
        [UsageExamples("!random raffle",
                       "!random raffle Admins")]
        public async Task RaffleAsync(CommandContext ctx,
                                     [Description("Role.")] DiscordRole role = null)
        {
            var online = ctx.Guild.Members.Where(m => m.Presence != null && m.Presence.Status != UserStatus.Offline);

            if (role != null)
                online = online.Where(m => m.Roles.Any(r => r.Id == role.Id));

            if (online.Count() == 0)
                throw new CommandFailedException("No online members to raffle from.");

            var raffled = online.ElementAt(GFRandom.Generator.Next(online.Count()));
            await InformAsync(ctx, StaticDiscordEmoji.Dice, $"Raffled: {raffled.Mention}")
                .ConfigureAwait(false);
        }
        #endregion
    }
}
