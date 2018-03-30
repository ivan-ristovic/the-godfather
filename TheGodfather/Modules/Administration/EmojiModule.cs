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
    [Group("emoji")]
    [Description("Manipulate guild emoji. Standalone call lists all guild emoji.")]
    [Aliases("emojis", "e")]
    [UsageExample("!emoji")]
    [Cooldown(2, 5, CooldownBucketType.Guild)]
    [ListeningCheck]
    public class EmojiModule : TheGodfatherBaseModule
    {

        [GroupCommand]
        public async Task ExecuteGroupAsync(CommandContext ctx)
            => await ListEmojiAsync(ctx).ConfigureAwait(false);


        #region COMMAND_EMOJI_ADD
        [Command("add")]
        [Description("Add emoji.")]
        [Aliases("create", "a", "+")]
        [UsageExample("!emoji add pepe http://i0.kym-cdn.com/photos/images/facebook/000/862/065/0e9.jpg")]
        [RequirePermissions(Permissions.ManageEmojis)]
        public async Task AddEmojiAsync(CommandContext ctx,
                                       [Description("Name.")] string name,
                                       [Description("URL.")] string url)
        {
            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(url))
                throw new InvalidCommandUsageException("Name or URL missing or invalid.");

            if (!IsValidImageURL(url, out Uri uri))
                throw new CommandFailedException("URL must point to an image and use http or https protocols.");

            try {
                var stream = await HTTPClient.GetStreamAsync(uri)
                   .ConfigureAwait(false);
                using (var ms = new MemoryStream()) {
                    await stream.CopyToAsync(ms)
                        .ConfigureAwait(false);
                    await ctx.Guild.CreateEmojiAsync(name, ms, reason: ctx.BuildReasonString())
                        .ConfigureAwait(false);
                }
            } catch (WebException e) {
                throw new CommandFailedException("Error getting the image.", e);
            } catch (BadRequestException e) {
                throw new CommandFailedException("Possibly emoji slots are full for this guild?", e);
            } catch (Exception e) {
                throw new CommandFailedException("An error occured.", e);
            }

            await ctx.RespondWithIconEmbedAsync($"Emoji {Formatter.Bold(name)} successfully added!")
                .ConfigureAwait(false);
        }
        #endregion

        #region COMMAND_EMOJI_DELETE
        [Command("delete")]
        [Description("Remove guild emoji. Note: bots can only delete emojis they created.")]
        [Aliases("remove", "del", "-", "d")]
        [UsageExample("!emoji delete pepe")]
        [RequirePermissions(Permissions.ManageEmojis)]
        public async Task DeleteEmojiAsync(CommandContext ctx,
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

        #region COMMAND_EMOJI_DETAILS
        [Command("details")]
        [Description("Get details for guild emoji.")]
        [UsageExample("!emoji details pepe")]
        [Aliases("det")]
        public async Task EmojiDetailsAsync(CommandContext ctx,
                                           [Description("Emoji.")] DiscordEmoji emoji)
        {
            try {
                var gemoji = await ctx.Guild.GetEmojiAsync(emoji.Id)
                    .ConfigureAwait(false);
                var emb = new DiscordEmbedBuilder() {
                    Title = "Details for emoji:",
                    Description = gemoji,
                    Color = DiscordColor.CornflowerBlue
                };
                emb.AddField("Name", gemoji.Name, inline: true);
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
        [Command("list")]
        [Description("View guild emojis.")]
        [Aliases("print", "show", "l", "p", "ls")]
        [UsageExample("!emoji list")]
        public async Task ListEmojiAsync(CommandContext ctx)
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
        [Description("Edit name of an existing guild emoji.")]
        [Aliases("edit", "mod", "e", "m", "rename")]
        [UsageExample("!emoji modify :pepe: newname")]
        [UsageExample("!emoji modify newname :pepe:")]
        [RequirePermissions(Permissions.ManageEmojis)]
        public async Task ModifyEmojiAsync(CommandContext ctx,
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
        public async Task ModifyEmojiAsync(CommandContext ctx,
                                          [Description("Name.")] string newname,
                                          [Description("Emoji.")] DiscordEmoji emoji)
            => await ModifyEmojiAsync(ctx, emoji, newname).ConfigureAwait(false);
        #endregion
    }
}
