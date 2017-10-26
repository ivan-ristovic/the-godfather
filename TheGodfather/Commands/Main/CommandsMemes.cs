#region USING_DIRECTIVES
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Text;
using System.Drawing.Drawing2D;
using System.Threading.Tasks;

using TheGodfather.Helpers.DataManagers;
using TheGodfather.Exceptions;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
#endregion

namespace TheGodfather.Commands.Main
{
    [Group("meme", CanInvokeWithoutSubcommand = true)]
    [Description("Manipulate memes. When invoked without name, returns a random one.")]
    [Aliases("memes", "mm")]
    [Cooldown(2, 3, CooldownBucketType.User), Cooldown(5, 3, CooldownBucketType.Channel)]
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
        [Aliases("+", "new")]
        [RequireOwner]
        public async Task AddMemeAsync(CommandContext ctx,
                                      [Description("Short name (case insensitive).")] string name = null,
                                      [Description("URL.")] string url = null)
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
        [Aliases("create", "maker", "c", "make", "m")]
        public async Task CreateMemeAsync(CommandContext ctx,
                                         [Description("Template.")] string template = null,
                                         [Description("Top Text.")] string topText = null,
                                         [Description("Bottom Text.")] string bottomText = null)
        {
            if (string.IsNullOrWhiteSpace(template))
                throw new InvalidCommandUsageException("Missing template name.");

            await ctx.TriggerTypingAsync()
                .ConfigureAwait(false);

            try {
                Bitmap image = new Bitmap($"Resources/meme-templates/{template}.jpg");
                Rectangle topLayout = new Rectangle(0, 0, image.Width, image.Height / 2);
                Rectangle botLayout = new Rectangle(0, image.Height / 2, image.Width, image.Height / 2);
                using (Graphics g = Graphics.FromImage(image)) {
                    g.InterpolationMode = InterpolationMode.High;
                    g.SmoothingMode = SmoothingMode.HighQuality;
                    g.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
                    g.CompositingQuality = CompositingQuality.HighQuality;
                    using (GraphicsPath p = new GraphicsPath()) {
                        p.AddString(topText, FontFamily.GenericSansSerif, (int)FontStyle.Regular, 70, topLayout, new StringFormat() {
                            Alignment = StringAlignment.Center,
                            LineAlignment = StringAlignment.Near,
                            FormatFlags = StringFormatFlags.FitBlackBox
                        });
                        g.DrawPath(new Pen(Color.Black, 5), p);
                        g.FillPath(Brushes.White, p);
                    }
                    using (GraphicsPath p = new GraphicsPath()) {
                        p.AddString(bottomText, FontFamily.GenericSansSerif, (int)FontStyle.Regular, 70, botLayout, new StringFormat() {
                            Alignment = StringAlignment.Center,
                            LineAlignment = StringAlignment.Far,
                            FormatFlags = StringFormatFlags.FitBlackBox
                        });
                        g.DrawPath(new Pen(Color.Black, 5), p);
                        g.FillPath(Brushes.White, p);
                    }
                    g.Flush();
                }

                string filename = $"Temp/{template}-{DateTime.Now.Ticks}.jpg";
                image.Save(filename, System.Drawing.Imaging.ImageFormat.Jpeg);

                await ctx.RespondWithFileAsync(new FileStream(filename, FileMode.Open))
                    .ConfigureAwait(false);

                if (File.Exists(filename))
                    File.Delete(filename);
            } catch (FileNotFoundException e) {
                throw new CommandFailedException("Unknown template name.", e);
            } catch (IOException e) {
                throw new CommandFailedException("Failed to load/save a meme template. Please report this.", e);
            } catch {
                // TODO catch long text exception, or maybe check text length and limit it?
            }
        }
        #endregion

        #region COMMAND_MEME_DELETE
        [Command("delete")]
        [Description("Deletes a meme from list.")]
        [Aliases("-", "del", "remove", "rm")]
        [RequireOwner]
        public async Task DeleteMemeAsync(CommandContext ctx, 
                                         [Description("Short name (case insensitive).")] string name = null)
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
        public async Task ListAsync(CommandContext ctx, 
                                   [Description("Page.")] int page = 1)
        {
            var memes = ctx.Dependencies.GetDependency<MemeManager>().Memes;

            if (page < 1 || page > memes.Count / 10 + 1)
                throw new CommandFailedException("No memes on that page.", new ArgumentOutOfRangeException());

            string desc = "";
            int starti = (page - 1) * 10;
            int endi = starti + 10 < memes.Count ? starti + 10 : memes.Count;
            var keys = memes.Keys.Take(page * 10).ToArray();
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
        [RequireOwner]
        public async Task SaveMemesAsync(CommandContext ctx)
        {
            if (ctx.Dependencies.GetDependency<MemeManager>().Save(ctx.Client.DebugLogger))
                await ctx.RespondAsync("Memes successfully saved.").ConfigureAwait(false);
            else
                throw new CommandFailedException("Failed saving memes.", new IOException());
        }
        #endregion


        #region HELPER_FUNCTIONS
        private async Task SendMemeAsync(CommandContext ctx, string url)
        {
            await ctx.TriggerTypingAsync()
                .ConfigureAwait(false);
            await ctx.RespondAsync(embed: new DiscordEmbedBuilder{ ImageUrl = url }.Build())
                .ConfigureAwait(false);
        }
        #endregion
    }
}
