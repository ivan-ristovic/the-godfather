#region USING_DIRECTIVES
using System;
using System.Linq;
using System.Threading.Tasks;

using TheGodfather.Attributes;
using TheGodfather.Exceptions;
using TheGodfather.Extensions;
using TheGodfather.Services;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
#endregion

namespace TheGodfather.Modules.Search
{
    [Group("urbandict")]
    [Description("Urban Dictionary commands. If invoked without subcommand, searches Urban Dictionary for a given query.")]
    [Aliases("ud", "urban")]
    [UsageExample("!urbandict blonde")]
    [Cooldown(2, 5, CooldownBucketType.User), Cooldown(4, 5, CooldownBucketType.Channel)]
    [ListeningCheck]
    public class UrbanDictModule : TheGodfatherBaseModule
    {

        [GroupCommand]
        public async Task ExecuteGroupAsync(CommandContext ctx,
                                           [RemainingText, Description("Query.")] string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                throw new InvalidCommandUsageException("Query missing.");

            var data = await UrbanDictService.GetDefinitionForTermAsync(query)
                .ConfigureAwait(false);

            if (data == null) {
                await ReplyWithEmbedAsync(ctx, "No results found!", ":negative_squared_cross_mark:")
                    .ConfigureAwait(false);
                return;
            }

            await InteractivityUtil.SendPaginatedCollectionAsync(
                ctx,
                $"Urban Dictionary definitions for \"{query}\"",
                data.List,
                res => $"Definition by {Formatter.Bold(res.Author)}:\n\n" +
                       $"{Formatter.Italic((res.Definition.Length < 1000 ? res.Definition : res.Definition.Take(1000) + "..."))}\n\n" +
                       $"{res.Permalink}",
                DiscordColor.CornflowerBlue,
                1
            ).ConfigureAwait(false);
        }
    }
}