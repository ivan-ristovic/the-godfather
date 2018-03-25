#region USING_DIRECTIVES
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using TheGodfather.Extensions.Converters;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
#endregion

namespace TheGodfather.Extensions
{
    public static class DSharpPlusExtensions
    {
        public static async Task<bool> AskYesNoQuestionAsync(this CommandContext ctx, string question)
        {
            await ctx.RespondAsync(embed: new DiscordEmbedBuilder {
                Description = $"{DiscordEmoji.FromName(ctx.Client, ":question:")} {question}",
                Color = DiscordColor.Yellow
            }).ConfigureAwait(false);

            if (!await ctx.WaitForConfirmationAsync().ConfigureAwait(false)) {
                await RespondWithFailedEmbedAsync(ctx, "Alright, aborting...")
                    .ConfigureAwait(false);
                return false;
            }

            return true;
        }

        public static async Task<bool> WaitForConfirmationAsync(this CommandContext ctx, ulong uid = 0)
        {
            bool response = false;
            var mctx = await ctx.Client.GetInteractivity().WaitForMessageAsync(
                m => {
                    if (m.ChannelId != ctx.Channel.Id || m.Author.Id != (uid != 0 ? uid : ctx.User.Id))
                        return false;
                    bool? b = CustomBoolConverter.TryConvert(m.Content);
                    response = b ?? false;
                    return b.HasValue;
                }
            ).ConfigureAwait(false);

            return response;
        }

        public static string BuildReasonString(this CommandContext ctx, string reason = null)
            => $"{ctx.User.ToString()} : {reason ?? "No reason provided."} | Invoked in: {ctx.Channel.ToString()}";

        public static Task<DiscordMessage> RespondWithIconEmbedAsync(this CommandContext ctx, string msg = "Done!", string icon_emoji = null)
        {
            return ctx.RespondAsync(embed: new DiscordEmbedBuilder {
                Description = $"{(icon_emoji == null ? EmojiUtil.CheckMarkSuccess : DiscordEmoji.FromName(ctx.Client, icon_emoji))} {msg}",
                Color = DiscordColor.Green
            });
        }

        public static Task<DiscordMessage> RespondWithIconEmbedAsync(this CommandContext ctx, DiscordEmoji emoji, string msg = "Done!")
        {
            return ctx.RespondAsync(embed: new DiscordEmbedBuilder {
                Description = $"{(emoji ?? EmojiUtil.CheckMarkSuccess)} {msg}",
                Color = DiscordColor.Green
            });
        }

        public static Task<DiscordMessage> RespondWithFailedEmbedAsync(this CommandContext ctx, string msg)
        {
            return ctx.RespondAsync(embed: new DiscordEmbedBuilder {
                Description = $"{EmojiUtil.BoardPieceX} {msg}",
                Color = DiscordColor.IndianRed
            });
        }

        public static Task<DiscordMessage> SendIconEmbedAsync(this DiscordChannel chn, string msg, DiscordEmoji icon = null)
        {
            return chn.SendMessageAsync(embed: new DiscordEmbedBuilder {
                Description = $"{icon ?? ""} {msg}",
                Color = DiscordColor.Green
            });
        }

        public static Task<DiscordMessage> SendFailedEmbedAsync(this DiscordChannel chn, string msg)
            => SendIconEmbedAsync(chn, msg, EmojiUtil.BoardPieceX);

        public static async Task<DiscordDmChannel> CreateDmChannelAsync(this DiscordClient client, ulong uid)
        {
            var firstResult = client.Guilds.Values.SelectMany(e => e.Members).FirstOrDefault(e => e.Id == uid);
            if (firstResult != null)
                return await firstResult.CreateDmChannelAsync().ConfigureAwait(false);
            else
                return null;
        }

        public static async Task<DiscordUser> WaitForGameOpponentAsync(this CommandContext ctx)
        {
            var mctx = await ctx.Client.GetInteractivity().WaitForMessageAsync(
                xm => {
                    if (xm.Author.Id == ctx.User.Id || xm.Channel.Id != ctx.Channel.Id)
                        return false;
                    var split = xm.Content.ToLowerInvariant().Split(' ');
                    return split.Length == 1 && (split[0] == "me" || split[0] == "i");
                }
            ).ConfigureAwait(false);

            return mctx?.User;
        }

        public static async Task SendPaginatedCollectionAsync<T>(this CommandContext ctx,
                                                                 string title,
                                                                 IEnumerable<T> collection,
                                                                 Func<T, string> formatter,
                                                                 DiscordColor? color = null,
                                                                 int amount = 10,
                                                                 TimeSpan? timeout = null)
        {
            var list = collection.ToList();
            var pages = new List<Page>();
            int pagesnum = (list.Count - 1) / amount;
            for (int i = 0; i <= pagesnum; i++) {
                int start = amount * i;
                int count = start + amount > list.Count ? list.Count - start : amount;
                pages.Add(new Page() {
                    Embed = new DiscordEmbedBuilder() {
                        Title = $"{title} (page {i + 1}/{pagesnum + 1})",
                        Description = string.Join("\n", list.GetRange(start, count).Select(formatter)),
                        Color = color ?? DiscordColor.Black
                    }.Build()
                });
            }
            await ctx.Client.GetInteractivity().SendPaginatedMessage(ctx.Channel, ctx.User, pages, timeout)
                .ConfigureAwait(false);
        }

        public static async Task<List<string>> WaitAndParsePollOptionsAsync(this CommandContext ctx)
        {
            var interactivity = ctx.Client.GetInteractivity();
            var mctx = await interactivity.WaitForMessageAsync(
                xm => xm.Author.Id == ctx.User.Id && xm.Channel.Id == ctx.Channel.Id,
                TimeSpan.FromMinutes(1)
            ).ConfigureAwait(false);
            if (mctx == null)
                return null;

            return mctx.Message.Content.Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries).Distinct().ToList();
        }
    }
}
