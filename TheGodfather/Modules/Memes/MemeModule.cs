#region USING_DIRECTIVES
using System;
using System.IO;
using System.Net;
using System.Linq;
using System.Drawing;
using System.Drawing.Text;
using System.Drawing.Drawing2D;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

using TheGodfather.Attributes;
using TheGodfather.Services;
using TheGodfather.Exceptions;
using TheGodfather.Extensions;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;

using Imgur.API;
#endregion

namespace TheGodfather.Modules.Memes
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

        public MemeModule(DatabaseService db) : base(db: db) { }


        [GroupCommand]
        public async Task ExecuteGroupAsync(CommandContext ctx,
                                           [RemainingText, Description("Meme name.")] string name = null)
        {
            string url = null;
            string text = "DANK MEME YOU ASKED FOR";

            if (string.IsNullOrWhiteSpace(name)) {
                url = await DatabaseService.GetRandomGuildMemeAsync(ctx.Guild.Id)
                    .ConfigureAwait(false);
            } else {
                url = await DatabaseService.GetGuildMemeUrlAsync(ctx.Guild.Id, name)
                    .ConfigureAwait(false);
                if (url == null) {
                    url = await DatabaseService.GetRandomGuildMemeAsync(ctx.Guild.Id)
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

            await DatabaseService.AddGuildMemeAsync(ctx.Guild.Id, name, uri.AbsoluteUri)
                .ConfigureAwait(false);
            await ReplyWithEmbedAsync(ctx, $"Meme {Formatter.Bold(name)} successfully added!")
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
            string filename = $"Resources/meme-templates/{template}.jpg";
            if (!File.Exists(filename))
                throw new CommandFailedException("Unknown template name.");

            if (string.IsNullOrWhiteSpace(topText))
                topText = "";
            else
                topText = topText.ToUpper();
            if (string.IsNullOrWhiteSpace(bottomText))
                bottomText = "";
            else
                bottomText = bottomText.ToUpper();
            template = template.ToLower();

            string url = null;
            try {
                using (var image = new Bitmap(filename))
                using (var g = Graphics.FromImage(image)) {
                    var topLayout = new Rectangle(0, 0, image.Width, image.Height / 3);
                    var botLayout = new Rectangle(0, (int)(0.66 * image.Height), image.Width, image.Height / 3);
                    g.InterpolationMode = InterpolationMode.High;
                    g.SmoothingMode = SmoothingMode.HighQuality;
                    g.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
                    g.CompositingQuality = CompositingQuality.HighQuality;
                    using (var p = new GraphicsPath()) {
                        var font = GetBestFittingFont(g, topText, topLayout.Size, new Font("Impact", 60));
                        var fmt = new StringFormat() {
                            Alignment = StringAlignment.Center,
                            LineAlignment = StringAlignment.Near,
                            FormatFlags = StringFormatFlags.FitBlackBox
                        };
                        p.AddString(topText, font.FontFamily, (int)FontStyle.Regular, font.Size, topLayout, fmt);
                        g.DrawPath(new Pen(Color.Black, 3), p);
                        g.FillPath(Brushes.White, p);
                    }
                    using (var p = new GraphicsPath()) {
                        var font = GetBestFittingFont(g, bottomText, botLayout.Size, new Font("Impact", 60));
                        var fmt = new StringFormat() {
                            Alignment = StringAlignment.Center,
                            LineAlignment = StringAlignment.Far,
                            FormatFlags = StringFormatFlags.FitBlackBox
                        };
                        p.AddString(bottomText, font.FontFamily, (int)FontStyle.Regular, font.Size, botLayout, fmt);
                        g.DrawPath(new Pen(Color.Black, 3), p);
                        g.FillPath(Brushes.White, p);
                    }
                    g.Flush();

                    using (var ms = new MemoryStream()) {
                        image.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
                        ms.Position = 0;
                        url = await ctx.Services.GetService<ImgurService>().UploadImageAsync(ms)
                            .ConfigureAwait(false);
                    }
                }

                if (url == null)
                    throw new CommandFailedException("Uploading meme to Imgur failed.");

                await ctx.RespondAsync(embed: new DiscordEmbedBuilder() {
                    Title = url,
                    Url = url,
                    ImageUrl = url
                }).ConfigureAwait(false);
            } catch (ImgurException e) {
                throw new CommandFailedException("Uploading meme to Imgur failed.", e);
            }
        }

        private Font GetBestFittingFont(Graphics g, string text, Size rect, Font font)
        {
            SizeF realSize = g.MeasureString(text, font);
            if ((realSize.Width <= rect.Width) && (realSize.Height <= rect.Height))
                return font;

            var rows = Math.Ceiling(realSize.Width / rect.Width);

            float ScaleFontSize = font.Size / ((float)Math.Log(rows) + 1) * 1.5f;
            return new Font(font.FontFamily, ScaleFontSize, font.Style);
        }
        #endregion

        #region COMMAND_MEME_DELETE
        [Command("delete")]
        [Description("Deletes a meme from list.")]
        [Aliases("-", "del", "remove", "rm", "d", "rem")]
        [UsageExample("!meme delete pepe")]
        [RequireUserPermissions(Permissions.ManageGuild)]
        public async Task DeleteMemeAsync(CommandContext ctx,
                                         [Description("Short name (case insensitive).")] string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new InvalidCommandUsageException("Name missing.");

            await DatabaseService.RemoveGuildMemeAsync(ctx.Guild.Id, name)
                .ConfigureAwait(false);
            await ReplyWithEmbedAsync(ctx, $"Meme {Formatter.Bold(name)} successfully removed!")
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
            var memes = await ctx.Services.GetService<DatabaseService>().GetAllGuildMemesAsync(ctx.Guild.Id)
                .ConfigureAwait(false); ;

            await InteractivityUtil.SendPaginatedCollectionAsync(
                ctx,
                "Memes registered in this guild",
                memes.OrderBy(kvp => kvp.Key),
                kvp => $"{Formatter.Bold(kvp.Key)} ({kvp.Value})",
                DiscordColor.Orange
            ).ConfigureAwait(false);
        }
        #endregion
    }
}
