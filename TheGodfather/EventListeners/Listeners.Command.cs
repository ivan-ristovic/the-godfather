#region USING_DIRECTIVES
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Exceptions;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;
using System;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TheGodfather.Common;
using TheGodfather.Common.Attributes;
using TheGodfather.Exceptions;
using TheGodfather.Extensions;
#endregion

namespace TheGodfather.EventListeners
{
    internal static partial class Listeners
    {
        [AsyncEventListener(DiscordEventType.CommandExecuted)]
        public static Task CommandExecutionEventHandler(TheGodfatherShard shard, CommandExecutionEventArgs e)
        {
            shard.Log(LogLevel.Info,
                $"| Executed: {e.Command?.QualifiedName ?? "<unknown command>"}\n" +
                $"| {e.Context.User.ToString()}\n" +
                $"| {e.Context.Guild.ToString()}; {e.Context.Channel.ToString()}"
            );
            return Task.CompletedTask;
        }

        [AsyncEventListener(DiscordEventType.CommandErrored)]
        public static async Task CommandErrorEventHandlerAsync(TheGodfatherShard shard, CommandErrorEventArgs e)
        {
            if (e.Exception == null)
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
            var sb = new StringBuilder(StaticDiscordEmoji.NoEntry).Append(" ");

            if (ex is CommandNotFoundException cne && shard.SharedData.GuildConfigurations[e.Context.Guild.Id].SuggestionsEnabled) {
                emb.WithTitle($"Command {Formatter.Bold(cne.CommandName)} not found. Did you mean...");
                var ordered = TheGodfatherShard.Commands
                    .OrderBy(tup => cne.CommandName.LevenshteinDistance(tup.Item1))
                    .Take(3);
                foreach ((string alias, Command cmd) in ordered)
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
                    await e.Context.Message.CreateReactionAsync(StaticDiscordEmoji.NoEntry);
                    return;
                }

                if (attr is UsesInteractivityAttribute) {
                    sb.Append($"I am waiting for your answer and you cannot execute commands until you either answer, or the timeout is reached.");
                } else {
                    sb.AppendLine($"Command {Formatter.Bold(e.Command.QualifiedName)} cannot be executed because:").AppendLine();
                    foreach (CheckBaseAttribute failed in exc.FailedChecks) {
                        if (failed is RequirePermissionsAttribute perms)
                            sb.AppendLine($"- One of us does not have the required permissions ({perms.Permissions.ToPermissionString()})!");
                        else if (failed is RequireUserPermissionsAttribute uperms)
                            sb.AppendLine($"- You do not have sufficient permissions ({uperms.Permissions.ToPermissionString()})!");
                        else if (failed is RequireBotPermissionsAttribute bperms)
                            sb.AppendLine($"- I do not have sufficient permissions ({bperms.Permissions.ToPermissionString()})!");
                        else if (failed is RequirePriviledgedUserAttribute)
                            sb.AppendLine($"- That command is reserved for my owner and priviledged users!");
                        else if (failed is RequireOwnerAttribute)
                            sb.AppendLine($"- That command is reserved only for my owner!");
                        else if (failed is RequireNsfwAttribute)
                            sb.AppendLine($"- That command is allowed only in NSFW channels!");
                        else if (failed is RequirePrefixesAttribute pattr)
                            sb.AppendLine($"- That command can only be invoked only with the following prefixes: {string.Join(" ", pattr.Prefixes)}!");
                    }
                }
            } else if (ex is UnauthorizedException) {
                sb.Append($"I am not authorized to do that.");
            } else if (ex is TargetInvocationException) {
                sb.Append($"{ex.InnerException?.Message ?? "Target invocation error occured. Please check the arguments provided and try again."}");
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
            await e.Context.RespondAsync(embed: emb.Build());
        }
    }
}
