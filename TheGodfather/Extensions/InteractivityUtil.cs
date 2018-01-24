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
        {
            return string.Equals(mctx.Message.Content, "yes", StringComparison.OrdinalIgnoreCase)
                || string.Equals(mctx.Message.Content, "y", StringComparison.OrdinalIgnoreCase)
                || string.Equals(mctx.Message.Content, "1", StringComparison.OrdinalIgnoreCase)
                || string.Equals(mctx.Message.Content, "ya", StringComparison.OrdinalIgnoreCase)
                || string.Equals(mctx.Message.Content, "ja", StringComparison.OrdinalIgnoreCase)
                || string.Equals(mctx.Message.Content, "si", StringComparison.OrdinalIgnoreCase)
                || string.Equals(mctx.Message.Content, "da", StringComparison.OrdinalIgnoreCase);
        }

        public static bool IsConfirmation(string m)
        {
            return string.Equals(m, "yes", StringComparison.OrdinalIgnoreCase)
                || string.Equals(m, "y", StringComparison.OrdinalIgnoreCase)
                || string.Equals(m, "1", StringComparison.OrdinalIgnoreCase)
                || string.Equals(m, "ya", StringComparison.OrdinalIgnoreCase)
                || string.Equals(m, "ja", StringComparison.OrdinalIgnoreCase)
                || string.Equals(m, "si", StringComparison.OrdinalIgnoreCase)
                || string.Equals(m, "da", StringComparison.OrdinalIgnoreCase);
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
                                                                 int ammount = 10,
                                                                 TimeSpan? timeout = null)
        {
            var list = collection.ToList();
            var interactivity = ctx.Client.GetInteractivity();
            var pages = new List<Page>();
            int pagesnum = (list.Count - 1) / ammount;
            for (int i = 0; i <= pagesnum; i++) {
                int start = ammount * i;
                int count = start + ammount > list.Count ? list.Count - start : ammount;
                pages.Add(new Page() {
                    Embed = new DiscordEmbedBuilder() {
                        Title = $"{title} (page {i + 1})",
                        Description = string.Join("\n", list.GetRange(start, count).Select(formatter)),
                        Color = color != null ? color.Value : DiscordColor.Black
                    }.Build()
                });
            }
            await interactivity.SendPaginatedMessage(ctx.Channel, ctx.User, pages, TimeSpan.FromSeconds(30));
        }
    }
}
