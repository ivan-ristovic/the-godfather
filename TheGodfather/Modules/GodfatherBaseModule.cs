using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TheGodfather.Services;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using System.Net;

namespace TheGodfather.Modules
{
    public abstract class GodfatherBaseModule : BaseCommandModule
    {
        protected SharedData SharedData;
        protected DatabaseService DatabaseService;


        protected GodfatherBaseModule(SharedData shared, DatabaseService db)
        {
            SharedData = shared;
            DatabaseService = db;
        }


        protected async Task ReplySuccessAsync(CommandContext ctx, string msg = "Done!")
        {
            await ctx.RespondAsync(embed: new DiscordEmbedBuilder {
                Description = $"{DiscordEmoji.FromName(ctx.Client, ":white_check_mark:")} {msg}",
                Color = DiscordColor.Green
            }).ConfigureAwait(false);
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
