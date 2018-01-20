#region USING_DIRECTIVES
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

using TheGodfather.Exceptions;
using TheGodfather.Services;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
#endregion

namespace TheGodfather.Modules.Search
{
    public class UrbanDictModule
    {
        [Command("urbandict")]
        [Description("Search Urban Dictionary for a query.")]
        [Aliases("ud", "urban")]
        [Cooldown(2, 5, CooldownBucketType.User), Cooldown(4, 5, CooldownBucketType.Channel)]
        [PreExecutionCheck]
        public async Task UrbanDictAsync(CommandContext ctx,
                                        [RemainingText, Description("Query.")] string q)
        {
            if (string.IsNullOrWhiteSpace(q))
                throw new InvalidCommandUsageException("Query missing.");

            UrbanDictService.UrbanDictData data;
            try {
                data = await UrbanDictService.GetDefinitionForTermAsync(q)
                    .ConfigureAwait(false);
            } catch (Exception e) {
                throw new CommandFailedException("Error occured while connecting to Urban Dictionary.", e);
            }

            if (data.ResultType != "no_results") {
                foreach (var v in data.List) {
                    var eb = new DiscordEmbedBuilder() {
                        Title = $"Urban Dictionary definition for \"{q}\" by {v.Author}",
                        Description = v.Definition.Length < 1000 ? v.Definition : v.Definition.Take(1000) + "...",
                        Color = DiscordColor.CornflowerBlue,
                        Url = v.Permalink
                    };
                    if (!string.IsNullOrWhiteSpace(v.Example))
                        eb.AddField("Example", v.Example);

                    await ctx.RespondAsync(embed: eb.Build())
                        .ConfigureAwait(false);

                    var msg = await ctx.Client.GetInteractivity().WaitForMessageAsync(
                        m => m.Channel.Id == ctx.Channel.Id && m.Content.ToLower() == "next"
                        , TimeSpan.FromSeconds(5)
                    ).ConfigureAwait(false);
                    if (msg == null)
                        break;
                }
            } else {
                await ctx.RespondAsync("No results found!")
                    .ConfigureAwait(false);
            }
        }
    }

}