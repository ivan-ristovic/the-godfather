#region USING_DIRECTIVES
using System;
using System.Linq;
using System.Threading.Tasks;

using TheGodfather.Common;
using TheGodfather.Common.Attributes;
using TheGodfather.Exceptions;
using TheGodfather.Extensions;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Exceptions;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;
#endregion

namespace TheGodfather.EventListeners
{
    internal static class CommandListeners
    {
        [AsyncExecuter(EventTypes.CommandExecuted)]
        public static async Task CommandExecuted(TheGodfatherShard shard, CommandExecutionEventArgs e)
        {
            await Task.Delay(0).ConfigureAwait(false);

            shard.Log(LogLevel.Info,
                $"Executed: {e.Command?.QualifiedName ?? "<unknown command>"}<br>" +
                $"{e.Context.User.ToString()}<br>" +
                $"{e.Context.Guild.ToString()}; {e.Context.Channel.ToString()}"
            );
        }

        [AsyncExecuter(EventTypes.CommandErrored)]
        public static async Task CommandErrored(TheGodfatherShard shard, CommandErrorEventArgs e)
        {
            if (!TheGodfather.Listening || e.Exception == null)
                return;

            var ex = e.Exception;
            while (ex is AggregateException)
                ex = ex.InnerException;

            if (ex is ChecksFailedException chke && chke.FailedChecks.Any(c => c is NotBlockedAttribute))
                return;

            shard.Log(LogLevel.Info,
                $"Tried executing: {e.Command?.QualifiedName ?? "<unknown command>"}<br>" +
                $"{e.Context.User.ToString()}<br>" +
                $"{e.Context.Guild.ToString()}; {e.Context.Channel.ToString()}<br>" +
                $"Exception: {ex.GetType()}<br>" +
                (ex.InnerException != null ? $"Inner exception: {ex.InnerException.GetType()}<br>" : "") +
                $"Message: {ex.Message.Replace("\n", "<br>") ?? "<no message>"}<br>"
            );

            var emoji = DiscordEmoji.FromName(e.Context.Client, ":no_entry:");
            var emb = new DiscordEmbedBuilder {
                Color = DiscordColor.Red
            };

            if (shard.Shared.GuildConfigurations[e.Context.Guild.Id].SuggestionsEnabled && ex is CommandNotFoundException cne) {
                emb.WithTitle($"Command {cne.CommandName} not found. Did you mean...");
                var ordered = TheGodfatherShard.CommandNames
                    .OrderBy(tup => cne.CommandName.LevenshteinDistance(tup.Item1))
                    .Take(3);
                foreach (var (alias, cmd) in ordered)
                    emb.AddField($"{alias} ({cmd.QualifiedName})", cmd.Description);
            } else if (ex is InvalidCommandUsageException)
                emb.Description = $"{emoji} Invalid usage! {ex.Message}";
            else if (ex is ArgumentException)
                emb.Description = $"{emoji} Argument conversion error (please check {Formatter.Bold($"help {e.Command.QualifiedName}")}).\n\nDetails: {Formatter.Italic(ex.Message)}";
            else if (ex is CommandFailedException)
                emb.Description = $"{emoji} {ex.Message} {(ex.InnerException != null ? "Details: " + ex.InnerException.Message : "")}";
            else if (ex is DatabaseServiceException)
                emb.Description = $"{emoji} {ex.Message} Details: {ex.InnerException?.Message ?? "<none>"}";
            else if (ex is NotSupportedException)
                emb.Description = $"{emoji} Not supported. {ex.Message}";
            else if (ex is InvalidOperationException)
                emb.Description = $"{emoji} Invalid operation. {ex.Message}";
            else if (ex is NotFoundException)
                emb.Description = $"{emoji} 404: Not found.";
            else if (ex is BadRequestException)
                emb.Description = $"{emoji} Bad request. Please check if the parameters are valid.";
            else if (ex is Npgsql.NpgsqlException)
                emb.Description = $"{emoji} Serbian database failed to respond... Please {Formatter.InlineCode("report")} this.";
            else if (ex is ChecksFailedException exc) {
                var attr = exc.FailedChecks.First();
                if (attr is CooldownAttribute)
                    return;
                else if (attr is InteractivitySensitiveAttribute)
                    emb.Description = $"{emoji} Please answer me first!";
                else if (attr is RequirePermissionsAttribute perms)
                    emb.Description = $"{emoji} Permissions to execute that command ({perms.Permissions.ToPermissionString()}) aren't met!";
                else if (attr is RequireUserPermissionsAttribute uperms)
                    emb.Description = $"{emoji} You do not have the required permissions ({uperms.Permissions.ToPermissionString()}) to run this command!";
                else if (attr is RequireBotPermissionsAttribute bperms)
                    emb.Description = $"{emoji} I do not have the required permissions ({bperms.Permissions.ToPermissionString()}) to run this command!";
                else if (attr is RequirePriviledgedUserAttribute)
                    emb.Description = $"{emoji} That command is reserved for the bot owner and priviledged users!";
                else if (attr is RequireOwnerAttribute)
                    emb.Description = $"{emoji} That command is reserved for the bot owner only!";
                else if (attr is RequireNsfwAttribute)
                    emb.Description = $"{emoji} That command is allowed in NSFW channels only!";
                else if (attr is RequirePrefixesAttribute pattr)
                    emb.Description = $"{emoji} That command is allowedto be invoked with the following prefixes: {string.Join(", ", pattr.Prefixes)}!";
            } else if (ex is UnauthorizedException)
                emb.Description = $"{emoji} I am not authorized to do that.";
            else
                return;

            await e.Context.RespondAsync(embed: emb.Build())
                .ConfigureAwait(false);
        }
    }
}
