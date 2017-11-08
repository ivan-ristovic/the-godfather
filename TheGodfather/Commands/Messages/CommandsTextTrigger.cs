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
    [Group("trigger", CanInvokeWithoutSubcommand = false)]
    [Description("Trigger/response handling commands.")]
    [Aliases("alias", "triggers", "a", "t")]
    [Cooldown(2, 3, CooldownBucketType.User), Cooldown(5, 3, CooldownBucketType.Channel)]
    [CheckListeningAttribute]
    public class CommandsTextTrigger
    {
        #region COMMAND_TRIGGER_ADD
        [Command("add")]
        [Description("Add trigger to guild trigger list.")]
        [Aliases("+", "new")]
        [RequireUserPermissions(Permissions.ManageGuild)]
        public async Task AddAsync(CommandContext ctx,
                                  [Description("Alias name (case sensitive).")] string trigger,
                                  [RemainingText, Description("Response.")] string response)
        {
            if (string.IsNullOrWhiteSpace(trigger) || string.IsNullOrWhiteSpace(response))
                throw new InvalidCommandUsageException("Alias name or response missing or invalid.");

            if (ctx.Dependencies.GetDependency<GuildConfigManager>().TryAddTrigger(ctx.Guild.Id, trigger, response))
                await ctx.RespondAsync($"Trigger {Formatter.Bold(trigger)} successfully set.").ConfigureAwait(false);
            else
                throw new CommandFailedException($"Failed to add trigger.");
        }
        #endregion

        #region COMMAND_TRIGGER_DELETE
        [Command("delete")]
        [Description("Remove trigger from guild triggers list.")]
        [Aliases("-", "remove", "del", "rm")]
        [RequireUserPermissions(Permissions.ManageGuild)]
        public async Task DeleteAsync(CommandContext ctx, 
                                     [RemainingText, Description("Alias to remove.")] string alias)
        {
            if (string.IsNullOrWhiteSpace(alias))
                throw new InvalidCommandUsageException("Alias name missing.");

            if (ctx.Dependencies.GetDependency<GuildConfigManager>().TryRemoveTrigger(ctx.Guild.Id, alias))
                await ctx.RespondAsync($"Trigger {Formatter.Bold(alias)} successfully removed.").ConfigureAwait(false);
            else
                throw new CommandFailedException("Failed to remove trigger.");
        }
        #endregion

        #region COMMAND_TRIGGER_LIST
        [Command("list")]
        [Description("Show all triggers for the guild. Each page has 10 triggers.")]
        public async Task ListAsync(CommandContext ctx, 
                                   [Description("Page.")] int page = 1)
        {
            var triggers = ctx.Dependencies.GetDependency<GuildConfigManager>().GetAllGuildTriggers(ctx.Guild.Id);

            if (triggers == null) {
                await ctx.RespondAsync("No triggers registered for this guild.");
                return;
            }

            if (page < 1 || page > triggers.Count / 10 + 1)
                throw new CommandFailedException("No triggers on that page.");

            string desc = "";
            int starti = (page - 1) * 10;
            int endi = starti + 10 < triggers.Count ? starti + 10 : triggers.Count;
            var keys = triggers.Keys.Take(page * 10).ToArray();
            for (var i = starti; i < endi; i++)
                desc += $"{Formatter.Bold(keys[i])} : {triggers[keys[i]]}\n";

            await ctx.RespondAsync(embed: new DiscordEmbedBuilder() {
                Title = $"Available triggers (page {page}/{triggers.Count / 10 + 1}) :",
                Description = desc,
                Color = DiscordColor.Green
            }.Build()).ConfigureAwait(false);
        }
        #endregion

        #region COMMAND_TRIGGER_CLEAR
        [Command("clear")]
        [Description("Delete all triggers for the current guild.")]
        [RequireUserPermissions(Permissions.Administrator)]
        public async Task ClearAsync(CommandContext ctx)
        {
            if (ctx.Dependencies.GetDependency<GuildConfigManager>().ClearGuildTriggers(ctx.Guild.Id))
                await ctx.RespondAsync("Successfully removed all triggers for this guild.").ConfigureAwait(false);
            else
                throw new CommandFailedException("Clearing guild triggers failed");
        }
        #endregion
    }
}
