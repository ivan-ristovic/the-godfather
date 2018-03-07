#region USING_DIRECTIVES
using System.Threading.Tasks;

using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
#endregion

namespace TheGodfather.Extensions
{
    public static class DSharpPlusExtensions
    {
        public static Task<DiscordMessage> RespondWithIconEmbedAsync(this CommandContext ctx, string msg = "Done!", string icon_emoji = ":white_check_mark:")
        {
            return ctx.RespondAsync(embed: new DiscordEmbedBuilder {
                Description = $"{(string.IsNullOrWhiteSpace(icon_emoji) ? "" : DiscordEmoji.FromName(ctx.Client, icon_emoji))} {msg}",
                Color = DiscordColor.Green
            });
        }

        public static Task<DiscordMessage> RespondWithFailedEmbedAsync(this CommandContext ctx, string msg)
            => RespondWithIconEmbedAsync(ctx, msg, ":negative_squared_cross_mark:");

        public static async Task<bool> AskYesNoQuestionAsync(this CommandContext ctx, string question)
        {
            await ctx.RespondAsync(embed: new DiscordEmbedBuilder {
                Description = $"{DiscordEmoji.FromName(ctx.Client, ":question:")} {question}",
                Color = DiscordColor.Yellow
            }).ConfigureAwait(false);

            if (!await InteractivityUtil.WaitForConfirmationAsync(ctx).ConfigureAwait(false)) {
                await RespondWithFailedEmbedAsync(ctx, "Alright, aborting...")
                    .ConfigureAwait(false);
                return false;
            }

            return true;
        }

        public static string BuildReasonString(this CommandContext ctx, string reason = null)
            => $"{ctx.User.ToString()} : {reason ?? "No reason provided."} | Invoked in: {ctx.Channel.ToString()}";
        
        public static Task<DiscordMessage> SendIconEmbedAsync(this DiscordChannel chn, string msg, DiscordEmoji icon = null)
        {
            return chn.SendMessageAsync(embed: new DiscordEmbedBuilder {
                Description = $"{icon ?? ""} {msg}",
                Color = DiscordColor.Green
            });
        }

        public static Task<DiscordMessage> SendFailedEmbedAsync(this DiscordChannel chn, string msg)
            => SendIconEmbedAsync(chn, msg, DiscordEmoji.FromUnicode("\u274e"));
    }
}
