#region USING_DIRECTIVES
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.EventHandling;
using Microsoft.Extensions.DependencyInjection;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using TheGodfather.Common;
using TheGodfather.Exceptions;
#endregion

namespace TheGodfather.Extensions
{
    internal static class CommandContextExtensions
    {
        public static string BuildInvocationDetailsString(this CommandContext ctx, string reason = null)
            => $"{ctx.User} : {reason ?? "No reason provided."} | Invoked in: {ctx.Channel}";

        public static Task SendCollectionInPagesAsync<T>(this CommandContext ctx, string title, 
            IEnumerable<T> collection, Func<T, string> selector, DiscordColor? color = null, int pageSize = 10)
        {
            var pages = new List<Page>();
            int size = collection.Count();
            int amountOfPages = (size - 1) / pageSize;
            int start = 0;
            for (int i = 0; i <= amountOfPages; i++) {
                int takeAmount = start + pageSize > size ? size - start : pageSize;
                IEnumerable<string> formattedCollectionPart = collection
                    .Skip(start)
                    .Take(takeAmount)
                    .Select(selector);
                pages.Add(new Page(embed: new DiscordEmbedBuilder() {
                    Title = $"{title} (page {i + 1}/{amountOfPages + 1})",
                    Description = string.Join("\n", formattedCollectionPart),
                    Color = color ?? DiscordColor.Black
                }));
                start += pageSize;
            }
            
            return ctx.Client.GetInteractivity().SendPaginatedMessageAsync(ctx.Channel, ctx.User, pages);
        }

        public static async Task<bool> WaitForBoolReplyAsync(this CommandContext ctx, string question, DiscordChannel channel = null, bool reply = true)
        {
            channel = channel ?? ctx.Channel;

            await ctx.RespondAsync(embed: new DiscordEmbedBuilder {
                Description = $"{StaticDiscordEmoji.Question} {question} (y/n)",
                Color = DiscordColor.Yellow
            });

            if (await ctx.Client.GetInteractivity().WaitForBoolReplyAsync(ctx))
                return true;

            if (reply)
                await channel.InformFailureAsync("Alright, aborting...");

            return false;
        }


        internal static async Task<DiscordUser> WaitForGameOpponentAsync(this CommandContext ctx)
        {
            var shared = ctx.Services.GetService<SharedData>();
            shared.AddPendingResponse(ctx.Channel.Id, ctx.User.Id);

            InteractivityResult<DiscordMessage> mctx = await ctx.Client.GetInteractivity().WaitForMessageAsync(
                xm => {
                    if (xm.Author.IsBot || xm.Author.Id == ctx.User.Id || xm.Channel.Id != ctx.Channel.Id)
                        return false;
                    string[] split = xm.Content.ToLowerInvariant().Split(' ');
                    return split.Length == 1 && (split[0] == "me" || split[0] == "i");
                }
            );

            if (!shared.TryRemovePendingResponse(ctx.Channel.Id, ctx.User.Id))
                throw new ConcurrentOperationException("Failed to remove user from waiting list. This is bad!");

            return mctx.TimedOut ? null : mctx.Result.Author;
        }

        internal static async Task<List<string>> WaitAndParsePollOptionsAsync(this CommandContext ctx)
        {
            var shared = ctx.Services.GetService<SharedData>();
            shared.AddPendingResponse(ctx.Channel.Id, ctx.User.Id);

            InteractivityResult<DiscordMessage> mctx = await ctx.Client.GetInteractivity().WaitForMessageAsync(
                xm => xm.Author.Id == ctx.User.Id && xm.Channel.Id == ctx.Channel.Id
            );

            if (!shared.TryRemovePendingResponse(ctx.Channel.Id, ctx.User.Id))
                throw new ConcurrentOperationException("Failed to remove user from waiting list. This is bad!");

            if (mctx.TimedOut)
                return null;

            return mctx.Result.Content.Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries)
                .Distinct()
                .ToList();
        }
    }
}
