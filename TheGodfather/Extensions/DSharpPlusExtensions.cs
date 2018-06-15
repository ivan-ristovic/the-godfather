#region USING_DIRECTIVES
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

using TheGodfather.Common;
using TheGodfather.Common.Collections;
using TheGodfather.Common.Converters;

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
                Description = $"{StaticDiscordEmoji.Question} {question}",
                Color = DiscordColor.Yellow
            }).ConfigureAwait(false);

            if (!await ctx.Client.GetInteractivity().WaitForYesNoAnswerAsync(ctx).ConfigureAwait(false)) {
                await RespondWithFailedEmbedAsync(ctx, "Alright, aborting...")
                    .ConfigureAwait(false);
                return false;
            }

            return true;
        }

        public static async Task<bool> AskYesNoQuestionAsync(this DiscordChannel channel, CommandContext ctx, string question)
        {
            await channel.SendMessageAsync(embed: new DiscordEmbedBuilder {
                Description = $"{StaticDiscordEmoji.Question} {question}",
                Color = DiscordColor.Yellow
            }).ConfigureAwait(false);

            if (!await ctx.Client.GetInteractivity().WaitForYesNoAnswerAsync(ctx).ConfigureAwait(false)) {
                await channel.SendFailedEmbedAsync("Alright, aborting...")
                    .ConfigureAwait(false);
                return false;
            }

            return true;
        }

        public static Task<bool> WaitForYesNoAnswerAsync(this InteractivityExtension interactivity, CommandContext ctx, ulong uid = 0)
            => interactivity.WaitForYesNoAnswerAsync(ctx.Channel.Id, uid != 0 ? uid : ctx.User.Id, ctx.Services.GetService<SharedData>());

        public static async Task<bool> WaitForYesNoAnswerAsync(this InteractivityExtension interactivity, ulong cid, ulong uid, SharedData shared = null)
        {
            if (shared != null)
                shared.AddAwaitingUser(cid, uid);

            bool response = false;
            var mctx = await interactivity.WaitForMessageAsync(
                m => {
                    if (m.ChannelId != cid || m.Author.Id != uid)
                        return false;
                    bool? b = CustomBoolConverter.TryConvert(m.Content);
                    response = b ?? false;
                    return b.HasValue;
                }
            ).ConfigureAwait(false);

            if (shared != null)
                shared.RemoveAwaitingUser(cid, uid);

            return response;
        }

        public static string BuildReasonString(this CommandContext ctx, string reason = null)
        => $"{ctx.User.ToString()} : {reason ?? "No reason provided."} | Invoked in: {ctx.Channel.ToString()}";

        public static Task<DiscordMessage> RespondWithIconEmbedAsync(this CommandContext ctx, string msg = "Done!", string icon_emoji = null)
        {
            return ctx.RespondAsync(embed: new DiscordEmbedBuilder {
                Description = $"{(icon_emoji == null ? StaticDiscordEmoji.CheckMarkSuccess : DiscordEmoji.FromName(ctx.Client, icon_emoji))} {msg}",
                Color = DiscordColor.Green
            });
        }

        public static Task<DiscordMessage> RespondWithIconEmbedAsync(this CommandContext ctx, DiscordEmoji emoji, string msg = "Done!")
        {
            return ctx.RespondAsync(embed: new DiscordEmbedBuilder {
                Description = $"{(emoji ?? StaticDiscordEmoji.CheckMarkSuccess)} {msg}",
                Color = DiscordColor.Green
            });
        }

        public static Task<DiscordMessage> RespondWithFailedEmbedAsync(this CommandContext ctx, string msg)
        {
            return ctx.RespondAsync(embed: new DiscordEmbedBuilder {
                Description = $"{StaticDiscordEmoji.BoardPieceX} {msg}",
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
            => SendIconEmbedAsync(chn, msg, StaticDiscordEmoji.BoardPieceX);

        public static Task<DiscordDmChannel> CreateDmChannelAsync(this DiscordClient client, ulong uid)
        {
            var firstResult = client.Guilds.Values.SelectMany(e => e.Members).FirstOrDefault(e => e.Id == uid);
            if (firstResult != null)
                return firstResult.CreateDmChannelAsync();
            else
                return null;
        }

        public static async Task<DiscordUser> WaitForGameOpponentAsync(this CommandContext ctx)
        {
            var shared = ctx.Services.GetService<SharedData>();
            shared.AddAwaitingUser(ctx.Channel.Id, ctx.User.Id);

            var mctx = await ctx.Client.GetInteractivity().WaitForMessageAsync(
                xm => {
                    if (xm.Author.Id == ctx.User.Id || xm.Channel.Id != ctx.Channel.Id)
                        return false;
                    var split = xm.Content.ToLowerInvariant().Split(' ');
                    return split.Length == 1 && (split[0] == "me" || split[0] == "i");
                }
            ).ConfigureAwait(false);

            shared.RemoveAwaitingUser(ctx.Channel.Id, ctx.User.Id);

            return mctx?.User;
        }

        public static Task SendPaginatedCollectionAsync<T>(this CommandContext ctx,
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

            return ctx.Client.GetInteractivity().SendPaginatedMessage(ctx.Channel, ctx.User, pages, timeout);
        }

        public static async Task<List<string>> WaitAndParsePollOptionsAsync(this CommandContext ctx)
        {
            var shared = ctx.Services.GetService<SharedData>();
            shared.AddAwaitingUser(ctx.Channel.Id, ctx.User.Id);

            var mctx = await ctx.Client.GetInteractivity().WaitForMessageAsync(
                xm => xm.Author.Id == ctx.User.Id && xm.Channel.Id == ctx.Channel.Id,
                TimeSpan.FromMinutes(1)
            ).ConfigureAwait(false);
            if (mctx == null)
                return null;

            shared.RemoveAwaitingUser(ctx.Channel.Id, ctx.User.Id);

            return mctx.Message.Content.Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries).Distinct().ToList();
        }

        public static async Task<DiscordAuditLogEntry> GetFirstAuditLogEntryAsync(this DiscordGuild guild, AuditLogActionType type)
        {
            try {
                var entries = await guild.GetAuditLogsAsync(1, action_type: type)
                    .ConfigureAwait(false);
                return entries.Any() ? entries.FirstOrDefault() : null;
            } catch {
                return null;
            }
        }
    }
}
