#region USING_DIRECTIVES
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using TheGodfather.Common.Attributes;
using TheGodfather.Exceptions;
using TheGodfather.Extensions;
using TheGodfather.Modules.Misc.Extensions;
using TheGodfather.Modules.Misc.Services;
using TheGodfather.Services;
#endregion

namespace TheGodfather.Modules.Misc
{
    [Group("meme"), Module(ModuleType.Miscellaneous), NotBlocked]
    [Description("Manipulate guild memes. Group call returns a meme from this guild's meme list given by name " +
                 "or a random one if name isn't provided.")]
    [Aliases("memes", "mm")]
    [UsageExamples("!meme",
                   "!meme SomeMemeNameWhichYouAdded")]
    [Cooldown(3, 5, CooldownBucketType.Channel)]
    public partial class MemeModule : TheGodfatherModule
    {

        public MemeModule(SharedData shared, DBService db)
            : base(shared, db)
        {
            this.ModuleColor = DiscordColor.Goldenrod;
        }


        [GroupCommand, Priority(1)]
        public async Task ExecuteGroupAsync(CommandContext ctx)
        {
            string url = await this.Database.GetRandomMemeAsync(ctx.Guild.Id);
            if (url is null)
                throw new CommandFailedException("No memes registered in this guild!");

            await ctx.RespondAsync(embed: new DiscordEmbedBuilder {
                Title = "RANDOM DANK MEME FROM THIS GUILD MEME LIST",
                ImageUrl = url,
                Color = this.ModuleColor
            }.Build());
        }

        [GroupCommand, Priority(0)]
        public async Task ExecuteGroupAsync(CommandContext ctx,
                                           [RemainingText, Description("Meme name.")] string name)
        {
            string text = "DANK MEME YOU ASKED FOR";
            string url = await this.Database.GetMemeAsync(ctx.Guild.Id, name);
            if (url is null) {
                url = await this.Database.GetRandomMemeAsync(ctx.Guild.Id);
                if (url is null)
                    throw new CommandFailedException("No memes registered in this guild!");
                text = "No meme registered with that name, here is a random one";
            }

            await ctx.RespondAsync(embed: new DiscordEmbedBuilder {
                Title = text,
                ImageUrl = url,
                Color = this.ModuleColor
            }.Build());
        }


        #region COMMAND_MEME_ADD
        [Command("add"), Priority(1)]
        [Description("Add a new meme to the list.")]
        [Aliases("+", "new", "a", "+=", "<", "<<")]
        [UsageExamples("!meme add pepe http://i0.kym-cdn.com/photos/images/facebook/000/862/065/0e9.jpg")]
        [RequireUserPermissions(Permissions.ManageGuild)]
        public async Task AddMemeAsync(CommandContext ctx,
                                      [Description("Short name (case insensitive).")] string name,
                                      [Description("URL.")] Uri url = null)
        {
            if (url is null) {
                if (!ctx.Message.Attachments.Any() || !Uri.TryCreate(ctx.Message.Attachments.First().Url, UriKind.Absolute, out url))
                    throw new InvalidCommandUsageException("Please specify a name and a URL pointing to a meme image or attach it manually.");
            }

            if (!await this.IsValidImageUriAsync(url))
                throw new InvalidCommandUsageException("URL must point to an image.");

            if (name.Length > 30 || url.OriginalString.Length > 120)
                throw new CommandFailedException("Name/URL is too long. Name must be shorter than 30 characters, and URL must be shorter than 120 characters.");

            await this.Database.AddMemeAsync(ctx.Guild.Id, name, url.AbsoluteUri);
            await this.InformAsync(ctx, $"Meme {Formatter.Bold(name)} successfully added!", important: false);
        }

        [Command("add"), Priority(0)]
        public Task AddMemeAsync(CommandContext ctx,
                                [Description("URL.")] Uri url,
                                [Description("Short name (case insensitive).")] string name)
            => this.AddMemeAsync(ctx, name, url);
        #endregion

        #region COMMAND_MEME_CREATE
        [Command("create")]
        [Description("Creates a new meme from blank template.")]
        [Aliases("maker", "c", "make", "m")]
        [UsageExamples("!meme create 1stworld \"Top text\" \"Bottom text\"")]
        [RequirePermissions(Permissions.EmbedLinks)]
        public Task CreateMemeAsync(CommandContext ctx,
                                   [Description("Template.")] string template,
                                   [Description("Top Text.")] string topText,
                                   [Description("Bottom Text.")] string bottomText)
        {
            string url = MemeGenService.GenerateMeme(template, topText, bottomText);
            return ctx.RespondAsync(embed: new DiscordEmbedBuilder() {
                Title = "The meme you asked me to generate",
                Url = url,
                ImageUrl = url
            }.WithFooter("Powered by memegen.link").Build());
        }
        #endregion

        #region COMMAND_MEME_DELETE
        [Command("delete")]
        [Description("Deletes a meme from this guild's meme list.")]
        [Aliases("-", "del", "remove", "rm", "d", "rem", "-=", ">", ">>")]
        [UsageExamples("!meme delete pepe")]
        [RequireUserPermissions(Permissions.ManageGuild)]
        public async Task DeleteMemeAsync(CommandContext ctx,
                                         [Description("Short name (case insensitive).")] string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new InvalidCommandUsageException("Meme name is missing.");

            await this.Database.RemoveMemeAsync(ctx.Guild.Id, name);
            await this.InformAsync(ctx, $"Meme {Formatter.Bold(name)} successfully removed!", important: false);
        }
        #endregion

        #region COMMAND_MEME_DELETEALL
        [Command("deleteall"), UsesInteractivity]
        [Description("Deletes all guild memes.")]
        [Aliases("clear", "da", "ca", "cl", "clearall", ">>>")]
        [UsageExamples("!memes clear")]
        [RequireUserPermissions(Permissions.Administrator)]
        public async Task ClearMemesAsync(CommandContext ctx)
        {
            if (!await ctx.WaitForBoolReplyAsync("Are you sure you want to delete all memes for this guild?"))
                return;

            await this.Database.RemoveAllMemesForGuildAsync(ctx.Guild.Id);
            await this.InformAsync(ctx, "Removed all guild memes!", important: false);
        }
        #endregion

        #region COMMAND_MEME_LIST
        [Command("list")]
        [Description("List all registered memes for this guild.")]
        [Aliases("ls", "l")]
        [UsageExamples("!meme list")]
        public async Task ListAsync(CommandContext ctx)
        {
            IReadOnlyDictionary<string, string> memes = await this.Database.GetAllMemesAsync(ctx.Guild.Id);
            if (!memes.Any())
                throw new CommandFailedException("No memes registered in this guild!");

            await ctx.SendCollectionInPagesAsync(
                "Memes registered in this guild",
                memes.OrderBy(kvp => kvp.Key),
                kvp => $"{Formatter.Bold(kvp.Key)} | ({Formatter.InlineCode(kvp.Value)})",
                this.ModuleColor
            );
        }
        #endregion

        #region COMMAND_MEME_TEMPLATES
        [Command("templates")]
        [Description("Lists all available meme templates.")]
        [Aliases("template", "t")]
        [UsageExamples("!meme templates")]
        public async Task TemplatesAsync(CommandContext ctx)
        {
            IReadOnlyList<string> templates = await MemeGenService.GetMemeTemplatesAsync();
            if (templates is null)
                throw new CommandFailedException("Failed to retrieve meme templates.");

            await ctx.SendCollectionInPagesAsync(
                "Meme templates",
                templates,
                s => s,
                this.ModuleColor
            ).ConfigureAwait(false);
        }
        #endregion
    }
}
