#region USING_DIRECTIVES
using System;
using System.Linq;
using System.Reflection;
using System.Text;
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
        [AsyncEventListener(EventTypes.CommandExecuted)]
        public static async Task CommandExecuted(TheGodfatherShard shard, CommandExecutionEventArgs e)
        {
            await Task.Delay(0).ConfigureAwait(false);
            shard.Log(LogLevel.Info,
                $"| Executed: {e.Command?.QualifiedName ?? "<unknown command>"}\n" +
                $"| {e.Context.User.ToString()}\n" +
                $"| {e.Context.Guild.ToString()}; {e.Context.Channel.ToString()}"
            );
        }

        [AsyncEventListener(EventTypes.CommandErrored)]
        public static async Task CommandErrored(TheGodfatherShard shard, CommandErrorEventArgs e)
        {
            if (!shard.IsListening || e.Exception == null)
                return;

            var ex = e.Exception;
            while (ex is AggregateException)
                ex = ex.InnerException;

            if (ex is ChecksFailedException chke && chke.FailedChecks.Any(c => c is NotBlockedAttribute))
                return;

            shard.Log(LogLevel.Info,
                $"| Tried executing: {e.Command?.QualifiedName ?? "<unknown command>"}\n" +
                $"| {e.Context.User.ToString()}\n" +
                $"| {e.Context.Guild.ToString()}; {e.Context.Channel.ToString()}\n" +
                $"| Exception: {ex.GetType()}\n" +
                (ex.InnerException != null ? $"| Inner exception: {ex.InnerException.GetType()}\n" : "") +
                $"| Message: {ex.Message ?? "<no message provided>"}\n"
            );

            var emb = new DiscordEmbedBuilder {
                Color = DiscordColor.Red
            };
            var sb = new StringBuilder($"{DiscordEmoji.FromName(e.Context.Client, ":no_entry:")} ");

            if (ex is CommandNotFoundException cne && shard.SharedData.GuildConfigurations[e.Context.Guild.Id].SuggestionsEnabled) {
                emb.WithTitle($"Command {cne.CommandName} not found. Did you mean...");
                var ordered = TheGodfatherShard.Commands
                    .OrderBy(tup => cne.CommandName.LevenshteinDistance(tup.Item1))
                    .Take(3);
                foreach (var (alias, cmd) in ordered)
                    emb.AddField($"{alias} ({cmd.QualifiedName})", cmd.Description);
            } else if (ex is InvalidCommandUsageException) {
                sb.Append($"Invalid usage! {ex.Message}");
            } else if (ex is ArgumentException) {
                sb.Append($"Argument conversion error (please check {Formatter.Bold($"help {e.Command.QualifiedName}")}).\n\nDetails: {Formatter.Italic(ex.Message)}");
            } else if (ex is CommandFailedException) {
                sb.Append($"{ex.Message} {ex.InnerException?.Message}");
            } else if (ex is Npgsql.NpgsqlException) {
                sb.Append($"Database action failed. Please {Formatter.InlineCode("report")} this.");
            } else if (ex is ChecksFailedException exc) {
                var attr = exc.FailedChecks.First();
                if (attr is CooldownAttribute) {
                    await e.Context.Message.CreateReactionAsync(DiscordEmoji.FromName(e.Context.Client, ":no_entry:"))
                        .ConfigureAwait(false);
                    return;
                }

                if (attr is UsesInteractivityAttribute) {
                    sb.Append($"I am waiting for your answer and you cannot execute commands until you either answer, or the timeout is reached.");
                } else {
                    sb.AppendLine($"Command {e.Command.QualifiedName} cannot be executed because:").AppendLine();
                    if (attr is RequirePermissionsAttribute perms)
                        sb.AppendLine($"One of us does not have the required permissions ({perms.Permissions.ToPermissionString()})!");
                    if (attr is RequireUserPermissionsAttribute uperms)
                        sb.AppendLine($"You do not have the required permissions ({uperms.Permissions.ToPermissionString()})!");
                    if (attr is RequireBotPermissionsAttribute bperms)
                        sb.AppendLine($"I do not have the required permissions ({bperms.Permissions.ToPermissionString()})!");
                    if (attr is RequirePriviledgedUserAttribute)
                        sb.AppendLine($"That command is reserved for my owner and priviledged users!");
                    if (attr is RequireOwnerAttribute)
                        sb.AppendLine($"That command is reserved for my owner only!");
                    if (attr is RequireNsfwAttribute)
                        sb.AppendLine($"That command is allowed in NSFW channels only!");
                    if (attr is RequirePrefixesAttribute pattr)
                        sb.AppendLine($"That command can only be invoked only with the following prefixes: {string.Join(" ", pattr.Prefixes)}!");
                }
            } else if (ex is UnauthorizedException) {
                sb.Append($"I am unauthorized to do that.");
            } else if (ex is TargetInvocationException) {
                sb.Append($"{ex.InnerException?.Message ?? "An unknown error occured. Please check the arguments provided and try again."}");
            } else {
                sb.AppendLine($"Command {Formatter.Bold(e.Command.QualifiedName)} errored!").AppendLine();
                sb.AppendLine($"Exception: {Formatter.InlineCode(ex.GetType().ToString())}");
                sb.AppendLine($"Details: {Formatter.Italic(ex.Message)}");
                if (ex.InnerException != null) {
                    sb.AppendLine($"Inner exception: {Formatter.InlineCode(ex.InnerException.GetType().ToString())}");
                    sb.AppendLine($"Details: {Formatter.Italic(ex.InnerException.Message ?? "No details provided")}");
                }
            }

            emb.Description = sb.ToString();
            await e.Context.RespondAsync(embed: emb.Build())
                .ConfigureAwait(false);
        }
    }
}
