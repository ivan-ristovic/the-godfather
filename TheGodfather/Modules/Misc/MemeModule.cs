#region USING_DIRECTIVES
using System;
using System.Linq;
using System.Threading.Tasks;

using TheGodfather.Common.Attributes;
using TheGodfather.Exceptions;
using TheGodfather.Extensions;
using TheGodfather.Services;
using TheGodfather.Services.Database;
using TheGodfather.Services.Database.Memes;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
#endregion

namespace TheGodfather.Modules.Misc
{
    [Group("meme"), Module(ModuleType.Miscellaneous)]
    [Description("Manipulate guild memes. When invoked without subcommands, returns a meme from this guild's meme list given by name, otherwise returns random one.")]
    [Aliases("memes", "mm")]
    [UsageExamples("!meme",
                   "!meme SomeMemeNameWhichYouAdded")]
    [Cooldown(3, 5, CooldownBucketType.Channel)]
    [NotBlocked]
    public partial class MemeModule : TheGodfatherModule
    {

        public MemeModule(DBService db) : base(db: db) { }


        [GroupCommand, Priority(1)]
        public async Task ExecuteGroupAsync(CommandContext ctx)
        {
            string url = await Database.GetRandomMemeAsync(ctx.Guild.Id)
                .ConfigureAwait(false);

            if (url == null)
                throw new CommandFailedException("No memes registered in this guild!");

            await ctx.RespondAsync(embed: new DiscordEmbedBuilder {
                Title = "RANDOM DANK MEME FROM THIS GUILD MEME LIST",
                ImageUrl = url,
                Color = DiscordColor.Orange
            }.Build()).ConfigureAwait(false);
        }

        [GroupCommand, Priority(0)]
        public async Task ExecuteGroupAsync(CommandContext ctx,
                                           [RemainingText, Description("Meme name.")] string name)
        {
            string text = "DANK MEME YOU ASKED FOR";
            string url = await Database.GetMemeAsync(ctx.Guild.Id, name)
                .ConfigureAwait(false);
            if (url == null) {
                url = await Database.GetRandomMemeAsync(ctx.Guild.Id)
                    .ConfigureAwait(false);
                if (url == null)
                    throw new CommandFailedException("No memes registered in this guild!");
                text = "No meme registered with that name, here is a random one";
            }

            await ctx.RespondAsync(embed: new DiscordEmbedBuilder {
                Title = text,
                ImageUrl = url,
                Color = DiscordColor.Orange
            }.Build()).ConfigureAwait(false);
        }


        #region COMMAND_MEME_ADD
        [Command("add"), Priority(1)]
        [Module(ModuleType.Miscellaneous)]
        [Description("Add a new meme to the list.")]
        [Aliases("+", "new", "a")]
        [UsageExamples("!meme add pepe http://i0.kym-cdn.com/photos/images/facebook/000/862/065/0e9.jpg")]
        [RequireUserPermissions(Permissions.ManageGuild)]
        public async Task AddMemeAsync(CommandContext ctx,
                                      [Description("Short name (case insensitive).")] string name,
                                      [Description("URL.")] Uri url = null)
        {
            if (url == null) {
                if (!ctx.Message.Attachments.Any() || !Uri.TryCreate(ctx.Message.Attachments.First().Url, UriKind.Absolute, out url))
                    throw new InvalidCommandUsageException("Please specify a name and a URL pointing to a meme image or attach it manually.");
            }

            if (!await IsValidImageUriAsync(url))
                throw new InvalidCommandUsageException("URL must point to an image.");

            if (name.Length > 30 || url.OriginalString.Length > 120)
                throw new CommandFailedException("Name/URL is too long. Name must be shorter than 30 characters, and URL must be shorter than 120 characters.");

            await Database.AddMemeAsync(ctx.Guild.Id, name, url.AbsoluteUri)
                .ConfigureAwait(false);
            await ctx.InformSuccessAsync($"Meme {Formatter.Bold(name)} successfully added!")
                .ConfigureAwait(false);
        }

        [Command("add"), Priority(0)]
        public Task AddMemeAsync(CommandContext ctx,
                                [Description("URL.")] Uri url,
                                [Description("Short name (case insensitive).")] string name)
            => AddMemeAsync(ctx, name, url);
        #endregion

        #region COMMAND_MEME_CLEAR
        [Command("clear"), Module(ModuleType.Miscellaneous)]
        [Description("Deletes all guild memes.")]
        [Aliases("da", "ca", "cl", "clearall")]
        [UsageExamples("!memes clear")]
        [RequireUserPermissions(Permissions.Administrator)]
        [UsesInteractivity]
        public async Task ClearMemesAsync(CommandContext ctx)
        {
            if (!await ctx.WaitForBoolReplyAsync("Are you sure you want to delete all memes for this guild?").ConfigureAwait(false))
                return;

            try {
                await Database.RemoveAllMemesForGuildAsync(ctx.Guild.Id)
                    .ConfigureAwait(false);
            } catch (Exception e) {
                Shared.LogProvider.LogException(LogLevel.Warning, e);
                throw new CommandFailedException("Failed to delete memes from the database.");
            }

            await ctx.InformSuccessAsync("Removed all memes!")
                .ConfigureAwait(false);
        }
        #endregion

        #region COMMAND_MEME_CREATE
        [Command("create"), Module(ModuleType.Miscellaneous)]
        [Description("Creates a new meme from blank template.")]
        [Aliases("maker", "c", "make", "m")]
        [UsageExamples("!meme create 1stworld \"Top text\" \"Bottom text\"")]
        [RequirePermissions(Permissions.EmbedLinks)]
        public async Task CreateMemeAsync(CommandContext ctx,
                                         [Description("Template.")] string template,
                                         [Description("Top Text.")] string topText,
                                         [Description("Bottom Text.")] string bottomText)
        {
            var url = MemeGenService.GenerateMeme(template, topText, bottomText);
            await ctx.RespondAsync(embed: new DiscordEmbedBuilder() {
                Title = url,
                Url = url,
                ImageUrl = url
            }).ConfigureAwait(false);
        }
        #endregion

        #region COMMAND_MEME_DELETE
        [Command("delete"), Module(ModuleType.Miscellaneous)]
        [Description("Deletes a meme from this guild's meme list.")]
        [Aliases("-", "del", "remove", "rm", "d", "rem")]
        [UsageExamples("!meme delete pepe")]
        [RequireUserPermissions(Permissions.ManageGuild)]
        public async Task DeleteMemeAsync(CommandContext ctx,
                                         [Description("Short name (case insensitive).")] string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new InvalidCommandUsageException("Name missing.");

            await Database.RemoveMemeAsync(ctx.Guild.Id, name)
                .ConfigureAwait(false);
            await ctx.InformSuccessAsync($"Meme {Formatter.Bold(name)} successfully removed!")
                .ConfigureAwait(false);
        }
        #endregion

        #region COMMAND_MEME_LIST
        [Command("list"), Module(ModuleType.Miscellaneous)]
        [Description("List all registered memes for this guild.")]
        [Aliases("ls", "l")]
        [UsageExamples("!meme list")]
        public async Task ListAsync(CommandContext ctx)
        {
            var memes = await Database.GetAllMemesAsync(ctx.Guild.Id)
                .ConfigureAwait(false); ;

            await ctx.SendCollectionInPagesAsync(
                "Memes registered in this guild",
                memes.OrderBy(kvp => kvp.Key),
                kvp => $"{Formatter.Bold(kvp.Key)} ({kvp.Value})",
                DiscordColor.Orange
            ).ConfigureAwait(false);
        }
        #endregion

        #region COMMAND_MEME_TEMPLATES
        [Command("templates"), Module(ModuleType.Miscellaneous)]
        [Description("Lists all available meme templates.")]
        [Aliases("template", "t")]
        [UsageExamples("!meme templates")]
        public async Task TemplatesAsync(CommandContext ctx)
        {
            var templates = await MemeGenService.GetMemeTemplatesAsync()
                .ConfigureAwait(false);
            if (templates == null)
                throw new CommandFailedException("Failed to retrieve meme templates.");

            await ctx.SendCollectionInPagesAsync(
                "Meme templates",
                templates,
                s => s,
                DiscordColor.Brown
            ).ConfigureAwait(false);
        }
        #endregion
    }
}
