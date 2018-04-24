#region USING_DIRECTIVES
using System;
using System.Linq;
using System.Threading.Tasks;

using TheGodfather.Common.Attributes;
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
    [Group("urbandict"), Module(ModuleType.Searches)]
    [Description("Urban Dictionary commands. If invoked without subcommand, searches Urban Dictionary for a given query.")]
    [Aliases("ud", "urban")]
    [UsageExample("!urbandict blonde")]
    [Cooldown(3, 5, CooldownBucketType.Channel)]
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
                await ctx.RespondWithFailedEmbedAsync("No results found!")
                    .ConfigureAwait(false);
                return;
            }

            await ctx.SendPaginatedCollectionAsync(
                $"Urban Dictionary definitions for \"{query}\"",
                data.List,
                res => $"Definition by {Formatter.Bold(res.Author)}:\n\n" +
                       $"{Formatter.Italic((res.Definition.Length < 1000 ? res.Definition : res.Definition.Substring(0, 1000) + "...").Trim())}\n\n" +
                       $"{res.Permalink}",
                DiscordColor.CornflowerBlue,
                1
            ).ConfigureAwait(false);
        }
    }
}