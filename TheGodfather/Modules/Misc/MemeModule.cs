using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using TheGodfather.Attributes;
using TheGodfather.Common;
using TheGodfather.Database.Models;
using TheGodfather.Exceptions;
using TheGodfather.Extensions;
using TheGodfather.Modules.Misc.Common;
using TheGodfather.Modules.Misc.Services;

namespace TheGodfather.Modules.Misc
{
    [Group("meme"), Module(ModuleType.Misc), NotBlocked]
    [Aliases("memes", "mm")]
    [RequireGuild, Cooldown(3, 5, CooldownBucketType.Channel)]
    public sealed class MemeModule : TheGodfatherServiceModule<MemeService>
    {
        #region meme
        [GroupCommand, Priority(1)]
        [RequirePermissions(Permissions.EmbedLinks)]
        public async Task ExecuteGroupAsync(CommandContext ctx)
        {
            IReadOnlyList<Meme> memes = await this.Service.GetAllAsync(ctx.Guild.Id);
            if (!memes.Any())
                throw new CommandFailedException(ctx, TranslationKey.cmd_err_memes_none);

            Meme randomMeme = memes[new SecureRandom().Next(memes.Count)];
            await ctx.RespondAsync(embed: new DiscordEmbedBuilder {
                Title = randomMeme.Name,
                ImageUrl = randomMeme.Url,
                Color = this.ModuleColor,
                Url = randomMeme.Url,
            }.Build());
        }

        [GroupCommand, Priority(0)]
        [RequirePermissions(Permissions.EmbedLinks)]
        public async Task ExecuteGroupAsync(CommandContext ctx,
                                           [RemainingText, Description(TranslationKey.desc_meme_name)] string name)
        {
            name = name.ToLowerInvariant();
            Meme? meme = await this.Service.GetAsync(ctx.Guild.Id, name);
            if (meme is null)
                throw new CommandFailedException(ctx, TranslationKey.cmd_err_meme_404);

            await ctx.RespondAsync(embed: new DiscordEmbedBuilder {
                Title = meme.Name,
                ImageUrl = meme.Url,
                Color = this.ModuleColor,
                Url = meme.Url,
            }.Build());
        }
        #endregion

        #region meme add
        [Command("add"), Priority(1)]
        [Aliases("register", "reg", "a", "+", "+=", "<<", "<", "<-", "<=")]
        [RequireUserPermissions(Permissions.ManageGuild)]
        public async Task AddMemeAsync(CommandContext ctx,
                                      [Description(TranslationKey.desc_meme_name)] string name,
                                      [Description(TranslationKey.desc_meme_url)] Uri? url = null)
        {
            if (url is null) {
                if (!ctx.Message.Attachments.Any() || !Uri.TryCreate(ctx.Message.Attachments[0].Url, UriKind.Absolute, out url))
                    throw new InvalidCommandUsageException(ctx, TranslationKey.cmd_err_image_url);
            }

            if (url.AbsoluteUri.Length > Meme.UrlLimit)
                throw new InvalidCommandUsageException(ctx, TranslationKey.cmd_err_url_limit(Meme.UrlLimit));

            if (string.IsNullOrWhiteSpace(name))
                throw new InvalidCommandUsageException(ctx, TranslationKey.cmd_err_missing_name);

            if (name.Length > Meme.NameLimit)
                throw new InvalidCommandUsageException(ctx, TranslationKey.cmd_err_name(Meme.NameLimit));

            if (!await url.ContentTypeHeaderIsImageAsync(null))
                throw new InvalidCommandUsageException(ctx, TranslationKey.cmd_err_image_url_fail(Meme.UrlLimit));

            await this.Service.AddAsync(new Meme {
                GuildId = ctx.Guild.Id,
                Name = name.ToLowerInvariant(),
                Url = url.AbsoluteUri
            });

            await ctx.InfoAsync(this.ModuleColor);
        }

        [Command("add"), Priority(0)]
        public Task AddMemeAsync(CommandContext ctx,
                                [Description(TranslationKey.desc_meme_url)] Uri url,
                                [RemainingText, Description(TranslationKey.desc_meme_name)] string name)
            => this.AddMemeAsync(ctx, name, url);
        #endregion

        #region meme create
        [Command("create")]
        [Aliases("maker", "c", "make", "m")]
        [RequirePermissions(Permissions.EmbedLinks)]
        public Task CreateMemeAsync(CommandContext ctx,
                                   [Description(TranslationKey.desc_meme_template)] string template,
                                   [Description(TranslationKey.desc_meme_text_top)] string topText,
                                   [Description(TranslationKey.desc_meme_text_bot)] string bottomText)
        {
            string url = MemeGenService.GenerateMemeUrl(template, topText, bottomText);
            return ctx.RespondWithLocalizedEmbedAsync(emb => {
                emb.WithTitle(template);
                emb.WithColor(this.ModuleColor);
                emb.WithUrl(url);
                emb.WithImageUrl(url);
                emb.WithLocalizedFooter(TranslationKey.fmt_powered_by(MemeGenService.Provider), null);
            });
        }
        #endregion

        #region meme delete
        [Command("delete")]
        [Aliases("unregister", "remove", "rm", "del", "d", "-", "-=", ">", ">>", "->", "=>")]
        [RequireUserPermissions(Permissions.ManageGuild)]
        public async Task DeleteMemeAsync(CommandContext ctx,
                                         [RemainingText, Description(TranslationKey.desc_meme_name)] string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new InvalidCommandUsageException(ctx, TranslationKey.cmd_err_missing_name);

            int removed = await this.Service.RemoveAsync(ctx.Guild.Id, name.ToLowerInvariant());
            await ctx.InfoAsync(this.ModuleColor, TranslationKey.fmt_meme_del(removed));
        }
        #endregion

        #region meme deleteall
        [Command("deleteall"), UsesInteractivity]
        [Aliases("removeall", "rmrf", "rma", "clearall", "clear", "delall", "da", "cl", "-a", "--", ">>>")]
        [RequireUserPermissions(Permissions.Administrator)]
        public async Task ClearMemesAsync(CommandContext ctx)
        {
            if (!await ctx.WaitForBoolReplyAsync(TranslationKey.q_meme_rem_all))
                return;

            await this.Service.ClearAsync(ctx.Guild.Id);
            await ctx.InfoAsync(this.ModuleColor);
        }
        #endregion

        #region meme list
        [Command("list")]
        [Aliases("print", "show", "view", "ls", "l", "p")]
        public async Task ListAsync(CommandContext ctx)
        {
            IReadOnlyList<Meme> memes = await this.Service.GetAllAsync(ctx.Guild.Id);
            if (!memes.Any())
                throw new CommandFailedException(ctx, TranslationKey.cmd_err_memes_none);

            await ctx.PaginateAsync(TranslationKey.str_memes, memes, meme => Formatter.MaskedUrl(meme.Name, meme.Uri), this.ModuleColor);
        }
        #endregion

        #region meme templates
        [Command("templates")]
        [Aliases("template", "ts", "t")]
        public async Task TemplatesAsync(CommandContext ctx,
                                        [RemainingText, Description(TranslationKey.desc_meme_template)] string? template = null)
        {
            if (string.IsNullOrWhiteSpace(template)) {
                IReadOnlyList<MemeTemplate> templates = await MemeGenService.GetMemeTemplatesAsync();
                if (templates is null)
                    throw new CommandFailedException(ctx, TranslationKey.cmd_err_meme_template_fail);

                await ctx.PaginateAsync(
                    TranslationKey.str_meme_templates,
                    templates.OrderBy(t => t.Id),
                    t => $"{Formatter.Bold(t.Name)}: {Formatter.MaskedUrl(t.Id, new Uri(t.Url))}",
                    this.ModuleColor
                );
            } else {
                MemeTemplate? t = await MemeGenService.GetMemeTemplateAsync(template);
                if (t is null)
                    throw new CommandFailedException(ctx, TranslationKey.cmd_err_meme_template_404);

                await ctx.RespondAsync(embed: new DiscordEmbedBuilder {
                    Title = $"{t.Name}: {t.Id}",
                    ImageUrl = t.Url,
                    Color = this.ModuleColor,
                    Url = t.Url,
                }.Build());
            }
        }
        #endregion
    }
}
