#region USING_DIRECTIVES
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

using TheGodfather.Common.Attributes;
using TheGodfather.Exceptions;
using TheGodfather.Extensions;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;
#endregion

namespace TheGodfather.Modules.Administration
{
    [Group("emoji"), Module(ModuleType.Administration)]
    [Description("Manipulate guild emoji. Standalone call lists all guild emoji or gives information about given emoji.")]
    [Aliases("emojis", "e")]
    [UsageExample("!emoji")]
    [Cooldown(3, 5, CooldownBucketType.Guild)]
    [NotBlocked]
    public class EmojiModule : TheGodfatherBaseModule
    {

        [GroupCommand, Priority(1)]
        public Task ExecuteGroupAsync(CommandContext ctx)
            => ListAsync(ctx);

        [GroupCommand, Priority(0)]
        public Task ExecuteGroupAsync(CommandContext ctx,
                                     [Description("Emoji to print information about.")] DiscordEmoji emoji)
            => InfoAsync(ctx, emoji);


        #region COMMAND_EMOJI_ADD
        [Command("add"), Priority(3)]
        [Module(ModuleType.Administration)]
        [Description("Add emoji specified via URL or message attachment. If you have Discord Nitro, you can also pass emojis from another guild as arguments instead of their URLs.")]
        [Aliases("create", "a", "+", "install")]
        [UsageExample("!emoji add pepe http://i0.kym-cdn.com/photos/images/facebook/000/862/065/0e9.jpg")]
        [UsageExample("!emoji add pepe [ATTACHED IMAGE]")]
        [UsageExample("!emoji add pepe :pepe_from_other_server:")]
        [RequirePermissions(Permissions.ManageEmojis)]
        public async Task AddAsync(CommandContext ctx,
                                  [Description("Name.")] string name,
                                  [Description("URL.")] Uri url = null)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new InvalidCommandUsageException("Emoji name missing or invalid.");

            if (url == null) {
                if (!ctx.Message.Attachments.Any() || !Uri.TryCreate(ctx.Message.Attachments.First().Url, UriKind.Absolute, out url))
                    throw new InvalidCommandUsageException("Please specify a name and a URL pointing to an emoji image or attach an emoji image.");
            }

            if (!await IsValidImageUriAsync(url).ConfigureAwait(false))
                throw new InvalidCommandUsageException("URL must point to an image and use HTTP or HTTPS protocols.");

            try {
                using (var response = await HTTPClient.GetAsync(url).ConfigureAwait(false))
                using (var stream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false)) {
                    if (stream.Length >= 256000)
                        throw new CommandFailedException("The specified emoji is too large. Maximum allowed image size is 256KB.");
                    await ctx.Guild.CreateEmojiAsync(name, stream, reason: ctx.BuildReasonString())
                        .ConfigureAwait(false);
                }
            } catch (WebException e) {
                throw new CommandFailedException("Error getting the image.", e);
            } catch (BadRequestException e) {
                throw new CommandFailedException("Possibly emoji slots are full for this guild or the image format is not supported?", e);
            }

            await ctx.RespondWithIconEmbedAsync($"Emoji {Formatter.Bold(name)} successfully added!")
                .ConfigureAwait(false);
        }

        [Command("add"), Priority(2)]
        public Task AddAsync(CommandContext ctx,
                            [Description("URL.")] Uri url,
                            [Description("Name.")] string name)
            => AddAsync(ctx, name, url);

        [Command("add"), Priority(1)]
        public Task AddAsync(CommandContext ctx,
                            [Description("Name.")] string name,
                            [Description("Emoji from another server to steal.")] DiscordEmoji emoji)
        {
            if (emoji.Id == 0)
                throw new InvalidCommandUsageException("Cannot add a unicode emoji.");

            return AddAsync(ctx, name, new Uri(emoji.Url));
        }

        [Command("add"), Priority(0)]
        public Task AddAsync(CommandContext ctx,
                            [Description("Emoji from another server to steal.")] DiscordEmoji emoji,
                            [Description("Name.")] string name)
            => AddAsync(ctx, name, emoji);
        #endregion

        #region COMMAND_EMOJI_DELETE
        [Command("delete"), Module(ModuleType.Administration)]
        [Description("Remove guild emoji. Note: bots can only delete emojis they created.")]
        [Aliases("remove", "del", "-", "d")]
        [UsageExample("!emoji delete pepe")]
        [RequirePermissions(Permissions.ManageEmojis)]
        public async Task DeleteAsync(CommandContext ctx,
                                     [Description("Emoji to delete.")] DiscordEmoji emoji)
        {
            try {
                var gemoji = await ctx.Guild.GetEmojiAsync(emoji.Id)
                    .ConfigureAwait(false);
                string name = gemoji.Name;
                await ctx.Guild.DeleteEmojiAsync(gemoji, ctx.BuildReasonString())
                    .ConfigureAwait(false);
                await ctx.RespondWithIconEmbedAsync($"Emoji {Formatter.Bold(name)} successfully deleted!")
                    .ConfigureAwait(false);
            } catch (NotFoundException) {
                throw new CommandFailedException("Can't find that emoji in list of emoji that I made for this guild.");
            }
        }
        #endregion

        #region COMMAND_EMOJI_INFO
        [Command("info"), Module(ModuleType.Administration)]
        [Description("Get information for given guild emoji.")]
        [UsageExample("!emoji info pepe")]
        [Aliases("details", "information", "i")]
        public async Task InfoAsync(CommandContext ctx,
                                   [Description("Emoji.")] DiscordEmoji emoji)
        {
            try {
                var gemoji = await ctx.Guild.GetEmojiAsync(emoji.Id)
                    .ConfigureAwait(false);
                var emb = new DiscordEmbedBuilder() {
                    Title = "Details for emoji:",
                    Description = gemoji,
                    Color = DiscordColor.CornflowerBlue,
                    ThumbnailUrl = gemoji.Url
                };
                emb.AddField("Name", Formatter.InlineCode(gemoji.Name), inline: true);
                emb.AddField("Created by", gemoji.User != null ? gemoji.User.Username : "<unknown>", inline: true);
                emb.AddField("Integration managed", gemoji.IsManaged.ToString(), inline: true);
                await ctx.RespondAsync(embed: emb.Build())
                    .ConfigureAwait(false);
            } catch (NotFoundException) {
                throw new CommandFailedException("Can't find that emoji in list of emoji for this guild.");
            }
        }
        #endregion

        #region COMMAND_EMOJI_LIST
        [Command("list"), Module(ModuleType.Administration)]
        [Description("View guild emojis.")]
        [Aliases("print", "show", "l", "p", "ls")]
        [UsageExample("!emoji list")]
        public async Task ListAsync(CommandContext ctx)
        {
            await ctx.SendPaginatedCollectionAsync(
                "Guild specific emojis:",
                ctx.Guild.Emojis.OrderBy(e => e.Name),
                e => $"{e}  {e.Name}",
                DiscordColor.CornflowerBlue
            ).ConfigureAwait(false);
        }
        #endregion

        #region COMMAND_EMOJI_MODIFY
        [Command("modify"), Priority(1)]
        [Module(ModuleType.Administration)]
        [Description("Edit name of an existing guild emoji.")]
        [Aliases("edit", "mod", "e", "m", "rename")]
        [UsageExample("!emoji modify :pepe: newname")]
        [UsageExample("!emoji modify newname :pepe:")]
        [RequirePermissions(Permissions.ManageEmojis)]
        public async Task ModifyAsync(CommandContext ctx,
                                     [Description("Emoji.")] DiscordEmoji emoji,
                                     [Description("Name.")] string newname)
        {
            if (string.IsNullOrWhiteSpace(newname))
                throw new InvalidCommandUsageException("Name missing.");

            try {
                var gemoji = await ctx.Guild.GetEmojiAsync(emoji.Id)
                    .ConfigureAwait(false);
                await ctx.Guild.ModifyEmojiAsync(gemoji, name: newname, reason: ctx.BuildReasonString())
                    .ConfigureAwait(false);
                await ctx.RespondWithIconEmbedAsync()
                    .ConfigureAwait(false);
            } catch (NotFoundException) {
                throw new CommandFailedException("Can't find that emoji in list of emoji that I made for this guild.");
            }
        }

        [Command("modify"), Priority(0)]
        public Task ModifyAsync(CommandContext ctx,
                               [Description("Name.")] string newname,
                               [Description("Emoji.")] DiscordEmoji emoji)
            => ModifyAsync(ctx, emoji, newname);
        #endregion
    }
}
