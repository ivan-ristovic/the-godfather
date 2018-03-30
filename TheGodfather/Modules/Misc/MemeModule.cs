#region USING_DIRECTIVES
using System;
using System.Linq;
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

namespace TheGodfather.Modules.Misc
{
    [Group("meme")]
    [Description("Manipulate guild memes. When invoked without subcommands, returns a meme from this guild's meme list given by name, otherwise returns random one.")]
    [Aliases("memes", "mm")]
    [UsageExample("!meme")]
    [UsageExample("!meme SomeMemeNameWhichYouAdded")]
    [Cooldown(2, 3, CooldownBucketType.User), Cooldown(5, 3, CooldownBucketType.Channel)]
    [ListeningCheck]
    public partial class MemeModule : TheGodfatherBaseModule
    {

        public MemeModule(DBService db) : base(db: db) { }


        [GroupCommand]
        public async Task ExecuteGroupAsync(CommandContext ctx,
                                           [RemainingText, Description("Meme name.")] string name = null)
        {
            string url = null;
            string text = "DANK MEME YOU ASKED FOR";

            if (string.IsNullOrWhiteSpace(name)) {
                url = await Database.GetRandomGuildMemeAsync(ctx.Guild.Id)
                    .ConfigureAwait(false);
            } else {
                url = await Database.GetGuildMemeUrlAsync(ctx.Guild.Id, name)
                    .ConfigureAwait(false);
                if (url == null) {
                    url = await Database.GetRandomGuildMemeAsync(ctx.Guild.Id)
                        .ConfigureAwait(false);
                    text = "No meme registered with that name, here is a random one:";
                }
            }

            if (url == null)
                throw new CommandFailedException("No memes registered in this guild!");

            await ctx.RespondAsync(embed: new DiscordEmbedBuilder {
                Title = text,
                ImageUrl = url,
                Color = DiscordColor.Orange
            }.Build()).ConfigureAwait(false);
        }


        #region COMMAND_MEME_ADD
        [Command("add")]
        [Description("Add a new meme to the list.")]
        [Aliases("+", "new", "a")]
        [UsageExample("!meme add pepe http://i0.kym-cdn.com/photos/images/facebook/000/862/065/0e9.jpg")]
        [RequireUserPermissions(Permissions.ManageGuild)]
        public async Task AddMemeAsync(CommandContext ctx,
                                      [Description("Short name (case insensitive).")] string name,
                                      [Description("URL.")] string url)
        {
            if (string.IsNullOrWhiteSpace(url))
                throw new InvalidCommandUsageException("URL missing.");

            if (!IsValidImageURL(url, out Uri uri))
                throw new InvalidCommandUsageException("URL must point to an image.");

            if (name.Length > 30 || url.Length > 120)
                throw new CommandFailedException("Name/URL is too long. Name must be shorter than 30 characters, and URL must be shorter than 120 characters.");

            await Database.AddMemeAsync(ctx.Guild.Id, name, uri.AbsoluteUri)
                .ConfigureAwait(false);
            await ctx.RespondWithIconEmbedAsync($"Meme {Formatter.Bold(name)} successfully added!")
                .ConfigureAwait(false);
        }
        #endregion

        #region COMMAND_MEME_CLEAR
        [Command("clear")]
        [Description("Deletes all guild memes.")]
        [Aliases("da", "ca", "cl", "clearall")]
        [UsageExample("!memes clear")]
        [RequireUserPermissions(Permissions.Administrator)]
        public async Task ClearMemesAsync(CommandContext ctx)
        {
            await Database.RemoveAllGuildMemesAsync(ctx.Guild.Id)
                .ConfigureAwait(false);
            await ctx.RespondWithIconEmbedAsync()
                .ConfigureAwait(false);
        }
        #endregion

        #region COMMAND_MEME_CREATE
        [Command("create")]
        [Description("Creates a new meme from blank template.")]
        [Aliases("maker", "c", "make", "m")]
        [UsageExample("!meme create 1stworld \"Top text\" \"Bottom text\"")]
        [RequirePermissions(Permissions.EmbedLinks)]
        public async Task CreateMemeAsync(CommandContext ctx,
                                         [Description("Template.")] string template,
                                         [Description("Top Text.")] string topText,
                                         [Description("Bottom Text.")] string bottomText)
        {
            var url = MemeGenService.GetMemeGenerateUrl(template, topText, bottomText);
            await ctx.RespondAsync(embed: new DiscordEmbedBuilder() {
                Title = url,
                Url = url,
                ImageUrl = url
            }).ConfigureAwait(false);
        }
        #endregion

        #region COMMAND_MEME_DELETE
        [Command("delete")]
        [Description("Deletes a meme from this guild's meme list.")]
        [Aliases("-", "del", "remove", "rm", "d", "rem")]
        [UsageExample("!meme delete pepe")]
        [RequireUserPermissions(Permissions.ManageGuild)]
        public async Task DeleteMemeAsync(CommandContext ctx,
                                         [Description("Short name (case insensitive).")] string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new InvalidCommandUsageException("Name missing.");

            await Database.RemoveMemeAsync(ctx.Guild.Id, name)
                .ConfigureAwait(false);
            await ctx.RespondWithIconEmbedAsync($"Meme {Formatter.Bold(name)} successfully removed!")
                .ConfigureAwait(false);
        }
        #endregion

        #region COMMAND_MEME_LIST
        [Command("list")]
        [Description("List all registered memes for this guild.")]
        [Aliases("ls", "l")]
        [UsageExample("!meme list")]
        public async Task ListAsync(CommandContext ctx)
        {
            var memes = await Database.GetMemesForAllGuildsAsync(ctx.Guild.Id)
                .ConfigureAwait(false); ;

            await ctx.SendPaginatedCollectionAsync(
                "Memes registered in this guild",
                memes.OrderBy(kvp => kvp.Key),
                kvp => $"{Formatter.Bold(kvp.Key)} ({kvp.Value})",
                DiscordColor.Orange
            ).ConfigureAwait(false);
        }
        #endregion
        
        #region COMMAND_MEME_TEMPLATES
        [Command("templates")]
        [Description("Lists all available meme templates.")]
        [Aliases("template", "t")]
        [UsageExample("!meme templates")]
        public async Task TemplatesAsync(CommandContext ctx)
        {
            var templates = await MemeGenService.GetMemeTemplatesAsync()
                .ConfigureAwait(false);
            if (templates == null)
                throw new CommandFailedException("Failed to retrieve meme templates.");

            await ctx.SendPaginatedCollectionAsync(
                "Meme templates",
                templates,
                s => s,
                DiscordColor.Brown
            ).ConfigureAwait(false);
        }
        #endregion
    }
}
