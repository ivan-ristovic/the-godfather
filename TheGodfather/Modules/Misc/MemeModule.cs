#region USING_DIRECTIVES
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using TheGodfather.Common.Attributes;
using TheGodfather.Database;
using TheGodfather.Database.Entities;
using TheGodfather.Exceptions;
using TheGodfather.Extensions;
using TheGodfather.Modules.Misc.Services;
#endregion

namespace TheGodfather.Modules.Misc
{
    [Group("meme"), Module(ModuleType.Miscellaneous), NotBlocked]
    [Description("Manipulate guild memes. Group call returns a meme from this guild's meme list given by name " +
                 "or a random one if name isn't provided.")]
    [Aliases("memes", "mm")]
    [UsageExampleArgs("SomeMemeNameWhichYouAdded")]
    [Cooldown(3, 5, CooldownBucketType.Channel)]
    public partial class MemeModule : TheGodfatherModule
    {

        public MemeModule(SharedData shared, DatabaseContextBuilder db)
            : base(shared, db)
        {
            
        }


        [GroupCommand, Priority(1)]
        public Task ExecuteGroupAsync(CommandContext ctx)
        {
            DatabaseMeme meme;
            using (DatabaseContext db = this.Database.CreateContext()) {
                if (!db.Memes.Where(m => m.GuildId == ctx.Guild.Id).Any())
                    throw new CommandFailedException("No memes registered in this guild!");
                meme = db.Memes
                    .Where(m => m.GuildId == ctx.Guild.Id)
                    .Shuffle()
                    .First();
            }

            return ctx.RespondAsync(embed: new DiscordEmbedBuilder {
                Title = "RANDOM DANK MEME FROM THIS GUILD MEME LIST",
                Description = meme.Name,
                ImageUrl = meme.Url,
                Color = this.ModuleColor
            }.Build());
        }

        [GroupCommand, Priority(0)]
        public async Task ExecuteGroupAsync(CommandContext ctx,
                                           [RemainingText, Description("Meme name.")] string name)
        {
            name = name.ToLowerInvariant();
            string text = "DANK MEME YOU ASKED FOR";

            DatabaseMeme meme;
            using (DatabaseContext db = this.Database.CreateContext()) {
                if (!db.Memes.Where(m => m.GuildId == ctx.Guild.Id).Any())
                    throw new CommandFailedException("No memes registered in this guild!");
                meme = await db.Memes.FindAsync((long)ctx.Guild.Id, name);
                if (meme is null) {
                    text = "No meme registered with that name, here is a random one";
                    meme = db.Memes
                        .Where(m => m.GuildId == ctx.Guild.Id)
                        .Shuffle()
                        .First();
                }
            }

            await ctx.RespondAsync(embed: new DiscordEmbedBuilder {
                Title = text,
                Description = meme.Name,
                ImageUrl = meme.Url,
                Color = this.ModuleColor
            }.Build());
        }


        #region COMMAND_MEME_ADD
        [Command("add"), Priority(1)]
        [Description("Add a new meme to the list.")]
        [Aliases("+", "new", "a", "+=", "<", "<<")]
        [UsageExampleArgs("pepe http://i0.kym-cdn.com/photos/images/facebook/000/862/065/0e9.jpg")]
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

            using (DatabaseContext db = this.Database.CreateContext()) {
                db.Memes.Add(new DatabaseMeme {
                    GuildId = ctx.Guild.Id,
                    Name = name.ToLowerInvariant(),
                    Url = url.AbsoluteUri
                });
                await db.SaveChangesAsync();
            }

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
        [UsageExampleArgs("1stworld \"Top text\" \"Bottom text\"")]
        [RequirePermissions(Permissions.EmbedLinks)]
        public Task CreateMemeAsync(CommandContext ctx,
                                   [Description("Template.")] string template,
                                   [Description("Top Text.")] string topText,
                                   [Description("Bottom Text.")] string bottomText)
        {
            string url = MemeGenService.GenerateMeme(template, topText, bottomText);
            return ctx.RespondAsync(embed: new DiscordEmbedBuilder {
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
        [UsageExampleArgs("pepe")]
        [RequireUserPermissions(Permissions.ManageGuild)]
        public async Task DeleteMemeAsync(CommandContext ctx,
                                         [Description("Short name (case insensitive).")] string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new InvalidCommandUsageException("Meme name is missing.");

            using (DatabaseContext db = this.Database.CreateContext()) {
                db.Memes.Remove(new DatabaseMeme {
                    GuildId = ctx.Guild.Id,
                    Name = name.ToLowerInvariant(),
                });
                await db.SaveChangesAsync();
            }

            await this.InformAsync(ctx, $"Meme {Formatter.Bold(name)} successfully removed!", important: false);
        }
        #endregion

        #region COMMAND_MEME_DELETEALL
        [Command("deleteall"), UsesInteractivity]
        [Description("Deletes all guild memes.")]
        [Aliases("clear", "da", "ca", "cl", "clearall", ">>>")]
        [RequireUserPermissions(Permissions.Administrator)]
        public async Task ClearMemesAsync(CommandContext ctx)
        {
            if (!await ctx.WaitForBoolReplyAsync("Are you sure you want to delete all memes for this guild?"))
                return;

            using (DatabaseContext db = this.Database.CreateContext()) {
                db.Memes.RemoveRange(db.Memes.Where(m => m.GuildId == ctx.Guild.Id));
                await db.SaveChangesAsync();
            }

            await this.InformAsync(ctx, "Removed all guild memes!", important: false);
        }
        #endregion

        #region COMMAND_MEME_LIST
        [Command("list")]
        [Description("List all registered memes for this guild.")]
        [Aliases("ls", "l")]
        public async Task ListAsync(CommandContext ctx)
        {
            List<DatabaseMeme> memes;
            using (DatabaseContext db = this.Database.CreateContext()) {
                memes = await db.Memes
                    .Where(m => m.GuildId == ctx.Guild.Id)
                    .OrderBy(m => m.Name)
                    .ToListAsync();
            }

            if (!memes.Any())
                throw new CommandFailedException("No memes registered in this guild!");

            await ctx.SendCollectionInPagesAsync(
                "Memes registered in this guild",
                memes,
                meme => Formatter.MaskedUrl(meme.Name, new Uri(meme.Url)),
                this.ModuleColor
            );
        }
        #endregion

        #region COMMAND_MEME_TEMPLATES
        [Command("templates")]
        [Description("Lists all available meme templates.")]
        [Aliases("template", "t")]
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
