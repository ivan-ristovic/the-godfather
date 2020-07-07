#region USING_DIRECTIVES
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;

using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

using TheGodfather.Common.Attributes;
using TheGodfather.Database;
using TheGodfather.Exceptions;
using TheGodfather.Extensions;
#endregion

namespace TheGodfather.Modules.Administration
{
    [Group("emoji"), Module(ModuleType.Administration), NotBlocked]
    [Description("Manipulate guild emoji. Standalone call lists all guild emoji or prints information about given emoji.")]
    [Aliases("emojis", "e")]
    
    [Cooldown(3, 5, CooldownBucketType.Guild)]
    public class EmojiModule : TheGodfatherModule
    {

        public EmojiModule(DbContextBuilder db)
            : base(db)
        {
            
        }


        [GroupCommand, Priority(1)]
        public Task ExecuteGroupAsync(CommandContext ctx)
            => this.ListAsync(ctx);

        [GroupCommand, Priority(0)]
        public Task ExecuteGroupAsync(CommandContext ctx,
                                     [Description("Emoji to print information about.")] DiscordEmoji emoji)
            => this.InfoAsync(ctx, emoji);


        #region COMMAND_EMOJI_ADD
        [Command("add"), Priority(3)]
        [Description("Add emoji specified via URL or as an attachment. If you have Discord Nitro, you can " +
                     "also pass emojis from another guild as arguments instead of their URLs.")]
        [Aliases("addnew", "create", "install", "a", "+", "+=", "<", "<<")]
        
        [RequirePermissions(Permissions.ManageEmojis)]
        public async Task AddAsync(CommandContext ctx,
                                  [Description("Name for the emoji.")] string name,
                                  [Description("Image URL.")] Uri url = null)
        {
            if (name.Length < 2 || name.Length > 50)
                throw new InvalidCommandUsageException("Emoji name length must be between 2 and 50 characters.");

            if (url is null) {
                if (!ctx.Message.Attachments.Any() || !Uri.TryCreate(ctx.Message.Attachments.First().Url, UriKind.Absolute, out url))
                    throw new InvalidCommandUsageException("Please specify a name and URL pointing to an emoji image or attach an image.");
            }

            if (!await url.IsValidImageUriAsync())
                throw new InvalidCommandUsageException("URL must point to an image and use HTTP or HTTPS protocols.");

            try {
                using (System.Net.Http.HttpResponseMessage response = await _http.GetAsync(url))
                using (System.IO.Stream stream = await response.Content.ReadAsStreamAsync()) {
                    if (stream.Length >= 256000)
                        throw new CommandFailedException("The specified emoji is too large. Maximum allowed image size is 256KB.");
                    DiscordGuildEmoji emoji = await ctx.Guild.CreateEmojiAsync(name, stream, reason: ctx.BuildInvocationDetailsString());
                    await this.InformAsync(ctx, $"Successfully added emoji: {emoji}", important: false);
                }
            } catch (WebException e) {
                throw new CommandFailedException("An error occured while fetching the image.", e);
            } catch (BadRequestException e) {
                throw new CommandFailedException("Discord prevented the emoji from being added. Possibly emoji slots are full for this guild or the image format is not supported?", e);
            }

        }

        [Command("add"), Priority(2)]
        public Task AddAsync(CommandContext ctx,
                            [Description("Image URL.")] Uri url,
                            [Description("Name for the emoji.")] string name)
            => this.AddAsync(ctx, name, url);

        [Command("add"), Priority(1)]
        public Task AddAsync(CommandContext ctx,
                            [Description("Name for the emoji.")] string name,
                            [Description("Emoji from another server to steal.")] DiscordEmoji emoji)
        {
            if (emoji.Id == 0)
                throw new InvalidCommandUsageException("Cannot add a unicode emoji.");

            return this.AddAsync(ctx, name, new Uri(emoji.Url));
        }

        [Command("add"), Priority(0)]
        public Task AddAsync(CommandContext ctx,
                            [Description("Emoji from another server to steal.")] DiscordEmoji emoji,
                            [Description("Name.")] string name)
            => this.AddAsync(ctx, name, emoji);
        #endregion

        #region COMMAND_EMOJI_DELETE
        [Command("delete")]
        [Description("Remove guild emoji. Note: Bots can only delete emojis they created!")]
        [Aliases("remove", "rm", "del", "d", "-", "-=", ">", ">>")]
        
        [RequirePermissions(Permissions.ManageEmojis)]
        public async Task DeleteAsync(CommandContext ctx,
                                     [Description("Emoji to delete.")] DiscordEmoji emoji)
        {
            try {
                DiscordGuildEmoji gemoji = await ctx.Guild.GetEmojiAsync(emoji.Id);
                string name = gemoji.Name;
                await ctx.Guild.DeleteEmojiAsync(gemoji, ctx.BuildInvocationDetailsString());
                await this.InformAsync(ctx, $"Successfully deleted emoji: {Formatter.Bold(name)}", important: false);
            } catch (NotFoundException) {
                throw new CommandFailedException("Can't find that emoji in list of emoji that I made for this guild.");
            }
        }
        #endregion

        #region COMMAND_EMOJI_INFO
        [Command("info")]
        [Description("Prints information for given guild emoji.")]
        [Aliases("details", "information", "i")]
        
        public async Task InfoAsync(CommandContext ctx,
                                   [Description("Emoji.")] DiscordEmoji emoji)
        {
            DiscordGuildEmoji gemoji = await ctx.Guild.GetEmojiAsync(emoji.Id);

            var emb = new DiscordEmbedBuilder {
                Title = "Emoji details:",
                Description = gemoji,
                Color = this.ModuleColor,
                ThumbnailUrl = gemoji.Url
            };

            emb.AddField("Name", Formatter.InlineCode(gemoji.Name), inline: true);
            emb.AddField("Created by", gemoji.User is null ? "<unknown>" : gemoji.User.Username, inline: true);
            emb.AddField("Integration managed", gemoji.IsManaged.ToString(), inline: true);

            await ctx.RespondAsync(embed: emb.Build());
        }
        #endregion

        #region COMMAND_EMOJI_LIST
        [Command("list")]
        [Description("List all emojis for this guild.")]
        [Aliases("print", "show", "l", "p", "ls")]
        public Task ListAsync(CommandContext ctx)
        {
            return ctx.SendCollectionInPagesAsync(
                $"Emoji available for guild {ctx.Guild.Name}:",
                ctx.Guild.Emojis.Select(kvp => kvp.Value).OrderBy(e => e.Name),
                emoji => $"{emoji} | {Formatter.InlineCode(emoji.Id.ToString())} | {Formatter.InlineCode(emoji.Name)}",
                this.ModuleColor
            );
        }
        #endregion

        #region COMMAND_EMOJI_MODIFY
        [Command("modify"), Priority(1)]
        [Description("Edit name of an existing guild emoji.")]
        [Aliases("edit", "mod", "e", "m", "rename")]
        
        [RequirePermissions(Permissions.ManageEmojis)]
        public async Task ModifyAsync(CommandContext ctx,
                                     [Description("Emoji to rename.")] DiscordEmoji emoji,
                                     [Description("New name.")] string newname)
        {
            if (string.IsNullOrWhiteSpace(newname))
                throw new InvalidCommandUsageException("Name missing.");

            try {
                DiscordGuildEmoji gemoji = await ctx.Guild.GetEmojiAsync(emoji.Id);
                gemoji = await ctx.Guild.ModifyEmojiAsync(gemoji, name: newname, reason: ctx.BuildInvocationDetailsString());
                await this.InformAsync(ctx, $"Successfully modified emoji: {gemoji}", important: false);
            } catch (NotFoundException) {
                throw new CommandFailedException("Can't find that emoji in list of emoji that I made for this guild.");
            }
        }

        [Command("modify"), Priority(0)]
        public Task ModifyAsync(CommandContext ctx,
                               [Description("New name.")] string newname,
                               [Description("Emoji to rename.")] DiscordEmoji emoji)
            => this.ModifyAsync(ctx, emoji, newname);
        #endregion
    }
}
