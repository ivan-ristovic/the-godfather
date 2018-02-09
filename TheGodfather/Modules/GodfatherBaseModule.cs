using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Threading.Tasks;

using TheGodfather.Services;
using TheGodfather.Extensions;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;

namespace TheGodfather.Modules
{
    public abstract class GodfatherBaseModule : BaseCommandModule
    {
        protected SharedData SharedData { get; }
        protected DatabaseService DatabaseService { get; }


        protected GodfatherBaseModule(SharedData shared = null, DatabaseService db = null)
        {
            SharedData = shared;
            DatabaseService = db;
        }


        protected async Task ReplySuccessAsync(CommandContext ctx, string msg = "Done!", string emojistr = ":white_check_mark:")
        {
            await ctx.RespondAsync(embed: new DiscordEmbedBuilder {
                Description = $"{(string.IsNullOrWhiteSpace(emojistr) ? "" : DiscordEmoji.FromName(ctx.Client, emojistr))} {msg}",
                Color = DiscordColor.Green
            }).ConfigureAwait(false);
        }

        protected async Task<bool> AskYesNoQuestionAsync(CommandContext ctx, string question)
        {
            await ctx.RespondAsync(embed: new DiscordEmbedBuilder {
                Description = $"{DiscordEmoji.FromName(ctx.Client, ":question:")} {question}",
                Color = DiscordColor.Yellow
            }).ConfigureAwait(false);

            bool answer = await InteractivityUtil.WaitForConfirmationAsync(ctx).ConfigureAwait(false);
            if (!answer)
                await ReplySuccessAsync(ctx, "Alright, aborting...")
                    .ConfigureAwait(false);

            return answer;
        }

        protected string GetReasonString(CommandContext ctx, string reason = null)
            => $"{ctx.User.ToString()} : {reason ?? "No reason provided."} | Invoked in: {ctx.Channel.ToString()}";

        protected bool IsValidURL(string url, out Uri uri)
        {
            uri = null;
            if (!Uri.TryCreate(url, UriKind.Absolute, out uri))
                return false;
            if (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps)
                return false;
            return true;
        }

        protected bool IsValidImageURL(string url, out Uri uri)
        {
            if (!IsValidImageURL(url, out uri))
                return false;

            if (WebRequest.Create(uri) is HttpWebRequest request) {
                string contentType = "";
                if (request.GetResponse() is HttpWebResponse response)
                    contentType = response.ContentType;
                if (!contentType.StartsWith("image/"))
                    return false;
            } else {
                return false;
            }

            return true;
        }
    }
}
