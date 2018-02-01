using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;

using DSharpPlus.Entities;
using DSharpPlus.Interactivity;


namespace TheGodfather.Extensions
{
    public static class InteractivityUtil
    {
        public static bool IsConfirmation(MessageContext mctx)
            => IsConfirmation(mctx.Message.Content);

        public static bool IsConfirmation(string message)
        {
            return string.Equals(message, "yes", StringComparison.OrdinalIgnoreCase)
                || string.Equals(message, "y", StringComparison.OrdinalIgnoreCase)
                || string.Equals(message, "1", StringComparison.OrdinalIgnoreCase)
                || string.Equals(message, "ya", StringComparison.OrdinalIgnoreCase)
                || string.Equals(message, "ja", StringComparison.OrdinalIgnoreCase)
                || string.Equals(message, "si", StringComparison.OrdinalIgnoreCase)
                || string.Equals(message, "da", StringComparison.OrdinalIgnoreCase);
        }

        public static async Task<bool> WaitForConfirmationAsync(CommandContext ctx)
        {
            var interactivity = ctx.Client.GetInteractivity();
            var mctx = await interactivity.WaitForMessageAsync(
                m => m.ChannelId == ctx.Channel.Id && m.Author.Id == ctx.User.Id
            ).ConfigureAwait(false);
            if (mctx != null && IsConfirmation(mctx))
                return true;
            return false;
        }
        
        public static async Task SendPaginatedCollectionAsync<T>(CommandContext ctx,
                                                                 string title,
                                                                 IEnumerable<T> collection, 
                                                                 Func<T, string> formatter,
                                                                 DiscordColor? color = null,
                                                                 int amount = 10,
                                                                 TimeSpan? timeout = null)
        {
            var list = collection.ToList();
            var interactivity = ctx.Client.GetInteractivity();
            var pages = new List<Page>();
            int pagesnum = (list.Count - 1) / amount;
            for (int i = 0; i <= pagesnum; i++) {
                int start = amount * i;
                int count = start + amount > list.Count ? list.Count - start : amount;
                pages.Add(new Page() {
                    Embed = new DiscordEmbedBuilder() {
                        Title = $"{title} (page {i + 1}/{pagesnum + 1})",
                        Description = string.Join("\n", list.GetRange(start, count).Select(formatter)),
                        Color = color != null ? color.Value : DiscordColor.Black
                    }.Build()
                });
            }
            await interactivity.SendPaginatedMessage(ctx.Channel, ctx.User, pages)
                .ConfigureAwait(false);
        }
    }
}
