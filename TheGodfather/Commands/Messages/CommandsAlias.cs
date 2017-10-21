#region USING_DIRECTIVES
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Newtonsoft.Json;

using TheGodfather.Helpers.DataManagers;
using TheGodfather.Exceptions;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
#endregion

namespace TheGodfather.Commands.Messages
{
    [Group("alias", CanInvokeWithoutSubcommand = false)]
    [Description("Alias handling commands.")]
    [Aliases("a", "aliases")]
    [Cooldown(2, 3, CooldownBucketType.User), Cooldown(5, 3, CooldownBucketType.Channel)]
    public class CommandsAlias
    {
        #region COMMAND_ALIAS_ADD
        [Command("add")]
        [Description("Add alias to list.")]
        [Aliases("+", "new")]
        [RequireUserPermissions(Permissions.ManageMessages)]
        public async Task AddAlias(CommandContext ctx,
                                  [Description("Alias name (case sensitive).")] string alias = null,
                                  [RemainingText, Description("Response.")] string response = null)
        {
            if (string.IsNullOrWhiteSpace(alias) || string.IsNullOrWhiteSpace(response))
                throw new InvalidCommandUsageException("Alias name or response missing or invalid.");

            if (ctx.Dependencies.GetDependency<AliasManager>().TryAdd(ctx.Guild.Id, alias, response))
                await ctx.RespondAsync($"Alias {Formatter.Bold(alias)} successfully added.");
            else
                throw new CommandFailedException($"Alias {Formatter.Bold(alias)} already exists!");
        }
        #endregion

        #region COMMAND_ALIAS_DELETE
        [Command("delete")]
        [Description("Remove alias from list.")]
        [Aliases("-", "remove", "del")]
        [RequireUserPermissions(Permissions.ManageMessages)]
        public async Task DeleteAlias(CommandContext ctx, 
                                     [RemainingText, Description("Alias to remove.")] string alias = null)
        {
            if (string.IsNullOrWhiteSpace(alias))
                throw new InvalidCommandUsageException("Alias name missing.");

            if (ctx.Dependencies.GetDependency<AliasManager>().TryRemove(ctx.Guild.Id, alias))
                await ctx.RespondAsync($"Alias {Formatter.Bold(alias)} successfully removed.");
            else
                throw new CommandFailedException("No such alias found!", new KeyNotFoundException());
        }
        #endregion

        #region COMMAND_ALIAS_SAVE
        [Command("save")]
        [Description("Save aliases to file.")]
        [RequireOwner]
        public async Task SaveAliases(CommandContext ctx)
        {
            if (ctx.Dependencies.GetDependency<AliasManager>().Save(ctx.Client.DebugLogger))
                await ctx.RespondAsync("Aliases successfully saved.");
            else
                throw new CommandFailedException("Failed saving aliases.", new IOException());
        }
        #endregion

        #region COMMAND_ALIAS_LIST
        [Command("list")]
        [Description("Show all aliases.")]
        public async Task ListAliases(CommandContext ctx, 
                                     [Description("Page.")] int page = 1)
        {
            var aliases = ctx.Dependencies.GetDependency<AliasManager>().Aliases;

            if (!aliases.ContainsKey(ctx.Guild.Id)) {
                await ctx.RespondAsync("No aliases registered.");
                return;
            }

            if (page < 1 || page > aliases[ctx.Guild.Id].Count / 10 + 1)
                throw new CommandFailedException("No aliases on that page.");

            string s = "";
            int starti = (page - 1) * 10;
            int endi = starti + 10 < aliases[ctx.Guild.Id].Count ? starti + 10 : aliases[ctx.Guild.Id].Count;
            var keys = aliases[ctx.Guild.Id].Keys.Take(page * 10).ToArray();
            for (var i = starti; i < endi; i++)
                s += $"{Formatter.Bold(keys[i])} : {aliases[ctx.Guild.Id][keys[i]]}\n";

            await ctx.RespondAsync(embed: new DiscordEmbedBuilder() {
                Title = $"Available aliases (page {page}/{aliases[ctx.Guild.Id].Count / 10 + 1}) :",
                Description = s,
                Color = DiscordColor.Green
            });
        }
        #endregion

        #region COMMAND_ALIAS_CLEAR
        [Command("clear")]
        [Description("Delete all aliases for the current guild.")]
        [RequireUserPermissions(Permissions.Administrator)]
        public async Task ClearAliases(CommandContext ctx)
        {
            if (ctx.Dependencies.GetDependency<AliasManager>().ClearGuildAliases(ctx.Guild.Id))
                await ctx.RespondAsync("All aliases for this guild successfully removed.");
            else
                throw new CommandFailedException("Clearing guild aliases failed");
        }
        #endregion

        #region COMMAND_ALIAS_CLEARALL
        [Command("clearall")]
        [Description("Delete all aliases stored for ALL guilds.")]
        [RequireOwner]
        public async Task ClearAllAliases(CommandContext ctx)
        {
            ctx.Dependencies.GetDependency<AliasManager>().ClearAllAliases();
            await ctx.RespondAsync("All aliases successfully removed.");
        }
        #endregion
    }
}
