#region USING_DIRECTIVES
using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

using TheGodfather.Attributes;
using TheGodfather.Exceptions;
using TheGodfather.Extensions;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
#endregion

namespace TheGodfather.Modules.Memes
{
    public partial class MemeModule
    {
        [Group("templates")]
        [Description("Manipulate meme templates. If invoked without subcommand, lists all templates.")]
        [Aliases("template", "t")]
        [UsageExample("!meme templates")]
        [Cooldown(2, 3, CooldownBucketType.User), Cooldown(5, 3, CooldownBucketType.Channel)]
        public class MemeTemplatesModule : TheGodfatherBaseModule
        {

            [GroupCommand]
            public async Task ExecuteGroupAsync(CommandContext ctx)
                => await ListAsync(ctx).ConfigureAwait(false);


            #region COMMAND_MEME_TEMPLATE_ADD
            [Command("add")]
            [Description("Add a new meme template.")]
            [Aliases("+", "new", "a")]
            [UsageExample("!meme template add evilracoon https://imgflip.com/s/meme/Evil-Plotting-Raccoon.jpg")]
            [RequireOwner]
            public async Task AddAsync(CommandContext ctx,
                                      [Description("Template name.")] string name,
                                      [Description("URL.")] string url)
            {
                if (name.Contains(' '))
                    throw new CommandFailedException("A template name must not contain spaces!");

                if (!IsValidImageURL(url, out Uri uri))
                    throw new CommandFailedException("URL must point to an image.");

                try {
                    using (var wc = new WebClient()) {
                        var data = wc.DownloadData(uri.AbsoluteUri);
                        using (var ms = new MemoryStream(data))
                        using (var image = Image.FromStream(ms))
                            image.Save($"Resources/meme-templates/{name.ToLowerInvariant()}.jpg", System.Drawing.Imaging.ImageFormat.Jpeg);
                    }
                } catch (WebException e) {
                    throw new CommandFailedException("Web exception thrown while fetching the image.", e);
                } catch (Exception e) {
                    throw new CommandFailedException("An error occured.", e);
                }

                await ReplyWithEmbedAsync(ctx, $"Template {Formatter.Bold(name)} successfully added!")
                    .ConfigureAwait(false);
            }
            #endregion

            #region COMMAND_MEME_TEMPLATE_DELETE
            [Command("delete")]
            [Description("Add a new meme template.")]
            [Aliases("-", "remove", "del", "rm", "d", "rem")]
            [UsageExample("!meme template delete evilracoon")]
            [RequireOwner]
            public async Task DeleteAsync(CommandContext ctx,
                                         [Description("Template name.")] string name)
            {
                string filename = $"Resources/meme-templates/{name.ToLowerInvariant()}.jpg";
                if (File.Exists(filename)) {
                    try {
                        File.Delete(filename);
                    } catch (Exception e) {
                        throw new CommandFailedException("An error occured. ", e);
                    }
                    await ReplyWithEmbedAsync(ctx, $"Template {Formatter.Bold(name)} successfully removed!")
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
            [UsageExample("!meme template list")]
            public async Task ListAsync(CommandContext ctx)
            {
                string dir = "Resources/meme-templates/";
                var templates = Directory.GetFiles(dir)
                    .Select(s => s.Substring(dir.Length, s.Length - dir.Length - 4))
                    .ToList();
                templates.Sort();

                await InteractivityUtil.SendPaginatedCollectionAsync(
                    ctx,
                    "Available meme templates",
                    templates,
                    t => t,
                    DiscordColor.Red
                ).ConfigureAwait(false);
            }
            #endregion

            #region COMMAND_MEME_TEMPLATE_PREVIEW
            [Command("preview")]
            [Description("Preview a meme template.")]
            [Aliases("p", "pr", "view")]
            [UsageExample("!meme template preview evilracoon")]
            public async Task PreviewAsync(CommandContext ctx,
                                          [Description("Template name.")] string name)
            {
                string filename = $"Resources/meme-templates/{name.ToLowerInvariant()}.jpg";
                if (!File.Exists(filename))
                    throw new CommandFailedException("Such template does not exist!");

                using (var fs = new FileStream(filename, FileMode.Open))
                    await ctx.RespondWithFileAsync(fs)
                        .ConfigureAwait(false);
            }
            #endregion
        }
    }
}
