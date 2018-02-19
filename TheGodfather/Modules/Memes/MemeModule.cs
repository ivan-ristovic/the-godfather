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
            if (string.IsNullOrWhiteSpace(name)) {
                await SendRandomMemeAsync(ctx.Client, ctx.Guild.Id, ctx.Channel.Id)
                    .ConfigureAwait(false);
                return;
            }

            string url = await DatabaseService.GetGuildMemeUrlAsync(ctx.Guild.Id, name)
                .ConfigureAwait(false);
            if (url == null) {
                await ctx.RespondAsync("No meme registered with that name, here is a random one: ")
                    .ConfigureAwait(false);
                await Task.Delay(500)
                    .ConfigureAwait(false);
                await SendRandomMemeAsync(ctx.Client, ctx.Guild.Id, ctx.Channel.Id)
                    .ConfigureAwait(false);
            } else {
                await SendMemeAsync(ctx.Client, ctx.Channel.Id, url)
                    .ConfigureAwait(false);
            }
        }


        #region COMMAND_MEME_ADD
        [Command("add")]
        [Description("Add a new meme to the list.")]
        [Aliases("+", "new", "a")]
        [RequireOwner]
        public async Task AddMemeAsync(CommandContext ctx,
                                      [Description("Short name (case insensitive).")] string name,
                                      [Description("URL.")] string url)
        {
            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(url))
                throw new InvalidCommandUsageException("Name or URL missing or invalid.");

            if (name.Length > 30 || url.Length > 120)
                throw new CommandFailedException("Name/URL is too long. Name must be shorter than 30 characters, and URL must be shorter than 120 characters.");

            Uri uriResult;
            bool result = Uri.TryCreate(url, UriKind.Absolute, out uriResult)
                && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);

            await ctx.Services.GetService<DatabaseService>().AddGuildMemeAsync(ctx.Guild.Id, name, url)
                .ConfigureAwait(false);
            await ctx.RespondAsync($"Meme {Formatter.Bold(name)} successfully added!")
                .ConfigureAwait(false);
        }
        #endregion

        #region COMMAND_MEME_CREATE
        [Command("create")]
        [Description("Creates a new meme from blank template.")]
        [Aliases("maker", "c", "make", "m")]
        public async Task CreateMemeAsync(CommandContext ctx,
                                         [Description("Template.")] string template,
                                         [Description("Top Text.")] string topText,
                                         [Description("Bottom Text.")] string bottomText)
        {
            if (string.IsNullOrWhiteSpace(template))
                throw new InvalidCommandUsageException("Missing template name.");

            if (string.IsNullOrWhiteSpace(topText))
                topText = "";
            else
                topText = topText.ToUpper();
            if (string.IsNullOrWhiteSpace(bottomText))
                bottomText = "";
            else
                bottomText = bottomText.ToUpper();
            template = template.ToLower();

            await ctx.TriggerTypingAsync()
                .ConfigureAwait(false);

            try {
                Bitmap image = new Bitmap($"Resources/meme-templates/{template}.jpg");
                Rectangle topLayout = new Rectangle(0, 0, image.Width, image.Height / 3);
                Rectangle botLayout = new Rectangle(0, (int)(0.66 * image.Height), image.Width, image.Height / 3);
                using (Graphics g = Graphics.FromImage(image)) {
                    g.InterpolationMode = InterpolationMode.High;
                    g.SmoothingMode = SmoothingMode.HighQuality;
                    g.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
                    g.CompositingQuality = CompositingQuality.HighQuality;
                    using (GraphicsPath p = new GraphicsPath()) {
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
                    using (GraphicsPath p = new GraphicsPath()) {
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
                }

                string filename = $"Temp/memegen-{template}-{DateTime.Now.Ticks}.jpg";
                if (!Directory.Exists("Temp"))
                    Directory.CreateDirectory("Temp");
                image.Save(filename, System.Drawing.Imaging.ImageFormat.Jpeg);
                image.Dispose();

                var fs = new FileStream(filename, FileMode.Open);
                var url = await ctx.Services.GetService<ImgurService>().UploadImageAsync(fs, filename)
                    .ConfigureAwait(false);
                fs.Close();

                await ctx.RespondAsync(embed: new DiscordEmbedBuilder() {
                    Title = url ?? "Uploading failed",
                    Url = url ?? "https://cms-assets.tutsplus.com/uploads/users/30/posts/25489/image/pac-404.png",
                    ImageUrl = url ?? "https://cms-assets.tutsplus.com/uploads/users/30/posts/25489/image/pac-404.png"
                }).ConfigureAwait(false);

                if (File.Exists(filename))
                    File.Delete(filename);
            } catch (ArgumentException e) {
                throw new CommandFailedException("Unknown template name.", e);
            } catch (IOException e) {
                throw new CommandFailedException("Failed to load/save a meme template. Please report this.", e);
            } catch (ImgurException e) {
                throw new CommandFailedException("Uploading image to Imgur failed.", e);
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
        [Aliases("-", "del", "remove", "rm", "d")]
        [RequireOwner]
        public async Task DeleteMemeAsync(CommandContext ctx,
                                         [Description("Short name (case insensitive).")] string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new InvalidCommandUsageException("Name missing.");

            await ctx.Services.GetService<DatabaseService>().RemoveGuildMemeAsync(ctx.Guild.Id, name)
                .ConfigureAwait(false);
            await ctx.RespondAsync($"Meme {Formatter.Bold(name)} successfully removed!")
                .ConfigureAwait(false);
        }
        #endregion

        #region COMMAND_MEME_LIST
        [Command("list")]
        [Description("List all registered memes.")]
        [Aliases("ls", "l")]
        public async Task ListAsync(CommandContext ctx,
                                   [Description("Page.")] int page = 1)
        {
            var memes = await ctx.Services.GetService<DatabaseService>().GetAllGuildMemesAsync(ctx.Guild.Id)
                .ConfigureAwait(false); ;

            if (page < 1 || page > memes.Count / 10 + 1)
                throw new CommandFailedException("No memes on that page.", new ArgumentOutOfRangeException());

            string desc = "";
            int starti = (page - 1) * 20;
            int endi = starti + 20 < memes.Count ? starti + 20 : memes.Count;
            var keys = memes.Keys.OrderBy(k => k).Take(page * 20).ToArray();
            for (var i = starti; i < endi; i++)
                desc += $"{Formatter.Bold(keys[i])} : {memes[keys[i]]}\n";

            await ctx.RespondAsync(embed: new DiscordEmbedBuilder() {
                Title = $"Available memes (page {page}/{memes.Count / 20 + 1}) :",
                Description = desc,
                Color = DiscordColor.Green
            }.Build()).ConfigureAwait(false);
        }
        #endregion



        #region HELPER_FUNCTIONS
        private async Task SendMemeAsync(DiscordClient client, ulong cid, string url)
        {
            var chn = await client.GetChannelAsync(cid)
                .ConfigureAwait(false);
            await chn.SendMessageAsync(embed: new DiscordEmbedBuilder { ImageUrl = url }.Build())
                .ConfigureAwait(false);
        }

        private async Task SendRandomMemeAsync(DiscordClient client, ulong gid, ulong cid)
        {
            var chn = await client.GetChannelAsync(cid)
                .ConfigureAwait(false);

            string url = await DatabaseService.GetRandomGuildMemeAsync(gid)
                .ConfigureAwait(false);

            if (url == null) {
                await chn.SendMessageAsync("No memes registered for this guild!")
                    .ConfigureAwait(false);
            } else {
                await chn.SendMessageAsync(embed: new DiscordEmbedBuilder { ImageUrl = url }.Build())
                    .ConfigureAwait(false);
            }
        }
        #endregion
    }
}
