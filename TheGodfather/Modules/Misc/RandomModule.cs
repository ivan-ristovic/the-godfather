#region USING_DIRECTIVES
using System;
using System.Net;
using System.Linq;
using System.Threading.Tasks;

using TheGodfather.Attributes;
using TheGodfather.Exceptions;
using TheGodfather.Services;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
#endregion

namespace TheGodfather.Modules.Misc
{
    [Group("random")]
    [Description("Return random things.")]
    [Aliases("rnd", "rand")]
    [Cooldown(2, 3, CooldownBucketType.User), Cooldown(5, 3, CooldownBucketType.Channel)]
    [ListeningCheck]
    public class CommandsRandomGroup : TheGodfatherBaseModule
    {
        #region COMMAND_CAT
        [Command("cat")]
        [Description("Get a random cat image.")]
        [UsageExample("!random cat")]
        public async Task RandomCatAsync(CommandContext ctx)
        {
            string url = PetImagesService.RandomCatImage();
            if (url == null)
                throw new CommandFailedException("Connection to random.cat failed!");

            await ctx.RespondAsync(embed: new DiscordEmbedBuilder() {
                Description = DiscordEmoji.FromName(ctx.Client, ":cat:"),
                ImageUrl = url
            }).ConfigureAwait(false);
        }
        #endregion

        #region COMMAND_DOG
        [Command("dog")]
        [Description("Get a random dog image.")]
        [UsageExample("!random dog")]
        public async Task RandomDogAsync(CommandContext ctx)
        {
            string url = PetImagesService.RandomDogImage();
            if (url == null)
                throw new CommandFailedException("Connection to random.dog failed!");

            await ctx.RespondAsync(embed: new DiscordEmbedBuilder() {
                Description = DiscordEmoji.FromName(ctx.Client, ":dog:"),
                ImageUrl = url
            }).ConfigureAwait(false);
        }
        #endregion

        #region COMMAND_CHOOSE
        [Command("choose")]
        [Description("!choose option1, option2, option3...")]
        [Aliases("select")]
        public async Task ChooseAsync(CommandContext ctx,
                                     [RemainingText, Description("Option list (separated with a comma).")] string s)
        {
            if (string.IsNullOrWhiteSpace(s))
                throw new InvalidCommandUsageException("Missing list to choose from.");

            var options = s.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
            await ctx.RespondAsync(options[new Random().Next(options.Length)].Trim())
                .ConfigureAwait(false);
        }
        #endregion

        #region COMMAND_RAFFLE
        [Command("raffle")]
        [Description("Choose a user from the online members list belonging to a given role.")]
        public async Task RaffleAsync(CommandContext ctx,
                                     [Description("Role.")] DiscordRole role = null)
        {
            if (role == null)
                role = ctx.Guild.EveryoneRole;

            var members = await ctx.Guild.GetAllMembersAsync()
                .ConfigureAwait(false);
            var online = members.Where(
                m => m.Roles.Contains(role) && m.Presence?.Status != UserStatus.Offline
            );

            await ctx.RespondAsync("Raffled: " + online.ElementAt(new Random().Next(online.Count())).Mention)
                .ConfigureAwait(false);
        }
        #endregion
    }
}
