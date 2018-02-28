#region USING_DIRECTIVES
using System;
using System.Net;
using System.Threading.Tasks;

using TheGodfather.Entities;
using TheGodfather.Extensions;
using TheGodfather.Services;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
#endregion

namespace TheGodfather.Modules
{
    public abstract class TheGodfatherBaseModule : BaseCommandModule
    {
        protected SharedData Shared { get; }
        protected DBService Database { get; }


        protected TheGodfatherBaseModule(SharedData shared = null, DBService db = null)
        {
            Shared = shared;
            Database = db;
        }


        protected async Task ReplyWithEmbedAsync(CommandContext ctx, string msg = "Done!", string emojistr = ":white_check_mark:")
        {
            await ctx.RespondAsync(embed: new DiscordEmbedBuilder {
                Description = $"{(string.IsNullOrWhiteSpace(emojistr) ? "" : DiscordEmoji.FromName(ctx.Client, emojistr))} {msg}",
                Color = DiscordColor.Green
            }).ConfigureAwait(false);
        }

        protected async Task ReplyWithFailedEmbedAsync(CommandContext ctx, string msg)
            => await ReplyWithEmbedAsync(ctx, msg, ":negative_squared_cross_mark:").ConfigureAwait(false);

        protected async Task<bool> AskYesNoQuestionAsync(CommandContext ctx, string question)
        {
            await ctx.RespondAsync(embed: new DiscordEmbedBuilder {
                Description = $"{DiscordEmoji.FromName(ctx.Client, ":question:")} {question}",
                Color = DiscordColor.Yellow
            }).ConfigureAwait(false);

            bool answer = await InteractivityUtil.WaitForConfirmationAsync(ctx).ConfigureAwait(false);
            if (!answer)
                await ReplyWithEmbedAsync(ctx, "Alright, aborting...")
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
            if (!IsValidURL(url, out uri))
                return false;

            try {
                if (WebRequest.Create(uri) is HttpWebRequest request) {
                    string contentType = "";
                    if (request.GetResponse() is HttpWebResponse response)
                        contentType = response.ContentType;
                    if (!contentType.StartsWith("image/"))
                        return false;
                } else {
                    return false;
                }
            } catch (Exception e) {
                Logger.LogException(LogLevel.Debug, e);
                return false;
            }

            return true;
        }
    }
}
