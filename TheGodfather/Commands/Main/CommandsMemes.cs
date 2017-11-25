#region USING_DIRECTIVES
using System;
using System.IO;
using System.Net;
using System.Linq;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Text;
using System.Drawing.Drawing2D;
using System.Threading.Tasks;

using TheGodfather.Helpers.DataManagers;
using TheGodfather.Services;
using TheGodfather.Exceptions;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;

using Imgur.API;
#endregion

namespace TheGodfather.Commands.Main
{
    [Group("meme", CanInvokeWithoutSubcommand = true)]
    [Description("Manipulate memes. When invoked without name, returns a random one.")]
    [Aliases("memes", "mm")]
    [Cooldown(2, 3, CooldownBucketType.User), Cooldown(5, 3, CooldownBucketType.Channel)]
    [PreExecutionCheck]
    public class CommandsMemes
    {
        public async Task ExecuteGroupAsync(CommandContext ctx,
                                           [RemainingText, Description("Meme name.")] string name = null)
        {
            if (string.IsNullOrWhiteSpace(name)) {
                await SendMemeAsync(ctx, ctx.Dependencies.GetDependency<MemeManager>().GetRandomMeme())
                    .ConfigureAwait(false);
                return;
            }

            string url = ctx.Dependencies.GetDependency<MemeManager>().GetUrl(name);
            if (url == null) {
                await ctx.RespondAsync("No meme registered with that name, here is a random one: ")
                    .ConfigureAwait(false);
                await Task.Delay(500)
                    .ConfigureAwait(false);
                await SendMemeAsync(ctx, ctx.Dependencies.GetDependency<MemeManager>().GetRandomMeme())
                    .ConfigureAwait(false);
            } else {
                await SendMemeAsync(ctx, url)
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

            if (ctx.Dependencies.GetDependency<MemeManager>().TryAdd(name, url))
                await ctx.RespondAsync($"Meme {Formatter.Bold(name)} successfully added!").ConfigureAwait(false);
            else
                await ctx.RespondAsync("Meme with that name already exists!").ConfigureAwait(false);
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
                Bitmap image = new Bitmap(ctx.Dependencies.GetDependency<MemeManager>().TemplateDirectory + $"{template}.jpg");
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
                var url = await ctx.Dependencies.GetDependency<ImgurService>().UploadImageAsync(fs, filename)
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

            if (ctx.Dependencies.GetDependency<MemeManager>().TryRemove(name))
                await ctx.RespondAsync($"Meme {Formatter.Bold(name)} successfully removed!").ConfigureAwait(false);
            else
                throw new CommandFailedException("Meme with that name doesn't exist!", new KeyNotFoundException());
        }
        #endregion

        #region COMMAND_MEME_LIST
        [Command("list")]
        [Description("List all registered memes.")]
        [Aliases("ls", "l")]
        public async Task ListAsync(CommandContext ctx,
                                   [Description("Page.")] int page = 1)
        {
            var memes = ctx.Dependencies.GetDependency<MemeManager>().Memes;

            if (page < 1 || page > memes.Count / 10 + 1)
                throw new CommandFailedException("No memes on that page.", new ArgumentOutOfRangeException());

            string desc = "";
            int starti = (page - 1) * 10;
            int endi = starti + 10 < memes.Count ? starti + 10 : memes.Count;
            var keys = memes.Keys.OrderBy(k => k).Take(page * 10).ToArray();
            for (var i = starti; i < endi; i++)
                desc += $"{Formatter.Bold(keys[i])} : {memes[keys[i]]}\n";

            await ctx.RespondAsync(embed: new DiscordEmbedBuilder() {
                Title = $"Available memes (page {page}/{memes.Count / 10 + 1}) :",
                Description = desc,
                Color = DiscordColor.Green
            }.Build()).ConfigureAwait(false);
        }
        #endregion

        #region COMMAND_MEME_SAVE
        [Command("save")]
        [Description("Saves all the memes.")]
        [Aliases("s")]
        [RequireOwner]
        public async Task SaveMemesAsync(CommandContext ctx)
        {
            if (ctx.Dependencies.GetDependency<MemeManager>().Save(ctx.Client.DebugLogger))
                await ctx.RespondAsync("Memes successfully saved.").ConfigureAwait(false);
            else
                throw new CommandFailedException("Failed saving memes.", new IOException());
        }
        #endregion


        [Group("templates", CanInvokeWithoutSubcommand = true)]
        [Description("Manipulate meme templates.")]
        [Aliases("template", "t")]
        [Cooldown(2, 3, CooldownBucketType.User), Cooldown(5, 3, CooldownBucketType.Channel)]
        public class CommandsMemeTemplates
        {
            public async Task ExecuteGroupAsync(CommandContext ctx)
            {
                await ListAsync(ctx)
                    .ConfigureAwait(false);
            }

            
            #region COMMAND_MEME_TEMPLATE_ADD
            [Command("add")]
            [Description("Add a new meme template.")]
            [Aliases("+", "new")]
            [RequireOwner]
            public async Task AddAsync(CommandContext ctx,
                                      [Description("Template name.")] string name,
                                      [Description("URL.")] string url)
            {
                string filename = $"Temp/tmp-template-{DateTime.Now.Ticks}.png";
                try {
                    if (!Directory.Exists("Temp"))
                        Directory.CreateDirectory("Temp");
                    using (WebClient webClient = new WebClient()) {
                        byte[] data = webClient.DownloadData(url);

                        using (MemoryStream mem = new MemoryStream(data))
                        using (Image image = Image.FromStream(mem))
                            image.Save(filename, System.Drawing.Imaging.ImageFormat.Jpeg);
                    }
                    if (File.Exists(filename))
                        File.Move(filename, $"Resources/meme-templates/{name}.jpg");
                } catch (WebException e) {
                    throw new CommandFailedException("URL error.", e);
                } catch (Exception e) {
                    throw new CommandFailedException("An error occured. Contact owner please.", e);
                }

                await ctx.RespondAsync($"Template {Formatter.Bold(name)} successfully added!")
                    .ConfigureAwait(false);
            }
            #endregion

            #region COMMAND_MEME_TEMPLATE_DELETE
            [Command("delete")]
            [Description("Add a new meme template.")]
            [Aliases("-", "remove", "del", "rm", "d")]
            [RequireOwner]
            public async Task DeleteAsync(CommandContext ctx,
                                         [Description("Template name.")] string name)
            {
                string filename = $"Resources/meme-templates/{name}.jpg";
                if (File.Exists(filename)) {
                    try {
                        File.Delete(filename);
                    } catch (Exception e) {
                        throw new CommandFailedException("An error occured. Contact owner please.", e);
                    }
                    await ctx.RespondAsync($"Template {Formatter.Bold(name)} successfully removed!")
                        .ConfigureAwait(false);
                } else {
                    throw new CommandFailedException("No such template found!");
                }
            }
            #endregion

            #region COMMAND_MEME_TEMPLATE_LIST
            [Command("list")]
            [Description("List all registered memes.")]
            [Aliases("ls", "l")]
            public async Task ListAsync(CommandContext ctx)
            {
                await ctx.RespondAsync(embed: new DiscordEmbedBuilder() {
                    Title = $"Available meme templates:",
                    Description = string.Join(", ", ctx.Dependencies.GetDependency<MemeManager>().GetAllTemplateNames()),
                    Color = DiscordColor.Green
                }.Build()).ConfigureAwait(false);
            }
            #endregion

            #region COMMAND_MEME_TEMPLATE_PREVIEW
            [Command("preview")]
            [Description("Preview a meme template.")]
            [Aliases("p", "pr", "view")]
            public async Task PreviewAsync(CommandContext ctx,
                                          [Description("Template name.")] string name)
            {
                string filename = $"Resources/meme-templates/{name.ToLower()}.jpg";
                if (!File.Exists(filename))
                    throw new CommandFailedException("Such template does not exist!");

                using (var fs = new FileStream(filename, FileMode.Open))
                    await ctx.RespondWithFileAsync(fs);
            }
            #endregion
        }


        #region HELPER_FUNCTIONS
        private async Task SendMemeAsync(CommandContext ctx, string url)
        {
            await ctx.RespondAsync(embed: new DiscordEmbedBuilder { ImageUrl = url }.Build())
                .ConfigureAwait(false);
        }
        #endregion
    }
}
