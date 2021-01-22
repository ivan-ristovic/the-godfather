using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;
using Humanizer;
using TheGodfather.Attributes;
using TheGodfather.Common;
using TheGodfather.Exceptions;
using TheGodfather.Extensions;
using TheGodfather.Services;
using TheGodfather.Services.Common;

namespace TheGodfather.Modules.Administration
{
    [Group("emoji"), Module(ModuleType.Administration), NotBlocked]
    [Aliases("emojis", "e")]
    [RequireGuild, RequirePermissions(Permissions.ManageEmojis)]
    [Cooldown(3, 5, CooldownBucketType.Guild)]
    public sealed class EmojiModule : TheGodfatherModule
    {
        #region emoji
        [GroupCommand, Priority(1)]
        public Task ExecuteGroupAsync(CommandContext ctx)
            => this.ListAsync(ctx);

        [GroupCommand, Priority(0)]
        public Task ExecuteGroupAsync(CommandContext ctx,
                                     [Description("desc-emoji-info")] DiscordEmoji emoji)
            => this.InfoAsync(ctx, emoji);
        #endregion

        #region emoji add
        [Command("add"), Priority(3)]
        [Aliases("create", "install", "register", "reg", "a", "+", "+=", "<<", "<", "<-", "<=")]
        public async Task AddAsync(CommandContext ctx,
                                  [Description("desc-emoji-name")] string name,
                                  [Description("desc-emoji-url")] Uri? url = null)
        {
            if (string.IsNullOrWhiteSpace(name) || name.Length < 2 || name.Length > DiscordLimits.EmojiNameLimit)
                throw new InvalidCommandUsageException(ctx, "cmd-err-emoji-name", 2, DiscordLimits.EmojiNameLimit);

            if (url is null) {
                if (!ctx.Message.Attachments.Any() || !Uri.TryCreate(ctx.Message.Attachments[0].Url, UriKind.Absolute, out url))
                    throw new InvalidCommandUsageException(ctx, "cmd-err-image-url");
            }

            if (!await url.ContentTypeHeaderIsImageAsync(DiscordLimits.EmojiSizeLimit))
                throw new InvalidCommandUsageException(ctx, "cmd-err-image-url-fail", DiscordLimits.EmojiSizeLimit.ToMetric());

            try {
                using Stream stream = await HttpService.GetStreamAsync(url);
                DiscordGuildEmoji emoji = await ctx.Guild.CreateEmojiAsync(name, stream, reason: ctx.BuildInvocationDetailsString());
                await ctx.InfoAsync(this.ModuleColor, "fmt-emoji-add", emoji.GetDiscordName());
            } catch (WebException e) {
                throw new CommandFailedException(ctx, e, "err-url-image-fail");
            }
        }

        [Command("add"), Priority(2)]
        public Task AddAsync(CommandContext ctx,
                            [Description("desc-emoji-url")] Uri url,
                            [Description("desc-emoji-name")] string name)
            => this.AddAsync(ctx, name, url);

        [Command("add"), Priority(1)]
        public Task AddAsync(CommandContext ctx,
                            [Description("desc-emoji-name")] string name,
                            [Description("desc-emoji-steal")] DiscordEmoji emoji)
        {
            return emoji.Id == 0
                ? throw new InvalidCommandUsageException(ctx, "cmd-err-emoji-add-unicode")
                : this.AddAsync(ctx, name, new Uri(emoji.Url));
        }

        [Command("add"), Priority(0)]
        public Task AddAsync(CommandContext ctx,
                            [Description("desc-emoji-steal")] DiscordEmoji emoji,
                            [Description("desc-emoji-name")] string? name = null)
            => this.AddAsync(ctx, name ?? emoji.Name, new Uri(emoji.Url));
        #endregion

        #region emoji delete
        [Command("delete")]
        [Aliases("unregister", "uninstall", "remove", "rm", "del", "d", "-", "-=", ">", ">>", "->", "=>")]
        public async Task DeleteAsync(CommandContext ctx,
                                     [Description("desc-emoji-del")] DiscordEmoji emoji,
                                     [RemainingText, Description("desc-rsn")] string? reason = null)
        {
            try {
                DiscordGuildEmoji gemoji = await ctx.Guild.GetEmojiAsync(emoji.Id);
                string name = gemoji.Name;
                await gemoji.DeleteAsync(ctx.BuildInvocationDetailsString(reason));
                await ctx.InfoAsync(this.ModuleColor, "fmt-emoji-del", Formatter.Bold(name));
            } catch (NotFoundException) {
                throw new CommandFailedException(ctx, "cmd-err-emoji-del-404");
            }
        }
        #endregion

        #region emoji info
        [Command("info")]
        [Aliases("information", "details", "about", "i")]
        public async Task InfoAsync(CommandContext ctx,
                                   [Description("desc-emoji-info")] DiscordEmoji emoji)
        {
            try {
                DiscordGuildEmoji gemoji = await ctx.Guild.GetEmojiAsync(emoji.Id);

                var emb = new LocalizedEmbedBuilder(this.Localization, ctx.Guild.Id);
                emb.WithColor(this.ModuleColor);
                emb.WithLocalizedTitle("str-emoji-details");
                emb.WithDescription($"{gemoji.GetDiscordName()} ({gemoji.Id})");
                emb.WithThumbnail(gemoji.Url);

                emb.AddLocalizedTitleField("str-created-by", gemoji.User?.ToDiscriminatorString(), inline: true);
                emb.AddLocalizedTitleField("str-animated", gemoji.IsAnimated, inline: true);
                emb.AddLocalizedTitleField("str-managed", gemoji.IsManaged, inline: true);
                emb.AddLocalizedTitleField("str-url", gemoji.Url);
                emb.AddLocalizedTimestampField("str-created-at", gemoji.CreationTimestamp);

                await ctx.RespondAsync(embed: emb.Build());
            } catch (NotFoundException) {
                throw new CommandFailedException(ctx, "cmd-err-emoji-404");
            }
        }
        #endregion

        #region emoji list
        [Command("list")]
        [Aliases("print", "show", "view", "ls", "l", "p")]
        public Task ListAsync(CommandContext ctx)
        {
            return ctx.PaginateAsync(
                "str-emojis",
                ctx.Guild.Emojis.Select(kvp => kvp.Value).OrderBy(e => e.Name),
                e => $"{e} | {Formatter.InlineCode(e.Id.ToString())} | {Formatter.InlineCode(e.Name)}",
                this.ModuleColor
            );
        }
        #endregion

        #region emoji modify
        [Command("modify"), Priority(1)]
        [Aliases("edit", "mod", "e", "m", "rename", "mv", "setname")]
        public async Task ModifyAsync(CommandContext ctx,
                                     [Description("desc-emoji-edit")] DiscordEmoji emoji,
                                     [Description("str-name")] string newname,
                                     [RemainingText, Description("desc-rsn")] string? reason = null)
        {
            if (string.IsNullOrWhiteSpace(newname) || newname.Length < 2 || newname.Length > DiscordLimits.EmojiNameLimit)
                throw new InvalidCommandUsageException(ctx, "cmd-err-emoji-name", 2, DiscordLimits.EmojiNameLimit);

            try {
                DiscordGuildEmoji gemoji = await ctx.Guild.GetEmojiAsync(emoji.Id);
                string name = gemoji.GetDiscordName();
                await ctx.Guild.ModifyEmojiAsync(gemoji, name: newname, reason: ctx.BuildInvocationDetailsString(reason));
                await ctx.InfoAsync(this.ModuleColor, "fmt-emoji-edit", Formatter.Bold(name));
            } catch (NotFoundException) {
                throw new CommandFailedException(ctx, "cmd-err-emoji-del-404");
            }
        }

        [Command("modify"), Priority(0)]
        public Task ModifyAsync(CommandContext ctx,
                               [Description("str-name")] string newname,
                               [Description("desc-emoji-edit")] DiscordEmoji emoji,
                               [RemainingText, Description("desc-rsn")] string? reason = null)
            => this.ModifyAsync(ctx, emoji, newname, reason);
        #endregion
    }
}
