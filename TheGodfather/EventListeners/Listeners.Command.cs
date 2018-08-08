#region USING_DIRECTIVES
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.CommandsNext.Exceptions;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;

using Npgsql;

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

            switch (ex) {
                case CommandNotFoundException cne:
                    if (!shard.SharedData.GuildConfigurations[e.Context.Guild.Id].SuggestionsEnabled)
                        return;

                    sb.Clear();
                    sb.AppendLine(Formatter.Bold($"Command {Formatter.InlineCode(cne.CommandName)} not found. Did you mean..."));
                    var ordered = TheGodfatherShard.Commands
                        .OrderBy(tup => cne.CommandName.LevenshteinDistance(tup.Item1))
                        .Take(3);
                    foreach ((string alias, Command cmd) in ordered)
                        emb.AddField($"{alias} ({cmd.QualifiedName})", cmd.Description);

                    break;
                case ArgumentException _:
                    sb.AppendLine("Invalid command usage! Details:").AppendLine();
                    sb.AppendLine(Formatter.BlockCode(ex.Message));
                    sb.AppendLine($"Type {Formatter.Bold($"help {e.Command.QualifiedName}")} for a command manual.");
                    break;
                case CommandFailedException _:
                    sb.Append($"{ex.Message} {ex.InnerException?.Message}");
                    break;
                case NpgsqlException dbex:
                    sb.Append($"Database operation failed. Details: {dbex.Message}");
                    break;
                case ChecksFailedException cfex:
                    switch (cfex.FailedChecks.First()) {
                        case CooldownAttribute _:
                            // await e.Context.Message.CreateReactionAsync(StaticDiscordEmoji.NoEntry);
                            return;
                        case UsesInteractivityAttribute _:
                            sb.Append($"I am waiting for your answer and you cannot execute commands until you either answer, or the timeout is reached.");
                            break;
                        default:
                            sb.AppendLine($"Command {Formatter.Bold(e.Command.QualifiedName)} cannot be executed because:").AppendLine();
                            foreach (CheckBaseAttribute attr in cfex.FailedChecks) {
                                switch (attr) {
                                    case RequirePermissionsAttribute perms:
                                        sb.AppendLine($"- One of us does not have the required permissions ({perms.Permissions.ToPermissionString()})!");
                                        break;
                                    case RequireUserPermissionsAttribute uperms:
                                        sb.AppendLine($"- You do not have sufficient permissions ({uperms.Permissions.ToPermissionString()})!");
                                        break;
                                    case RequireBotPermissionsAttribute bperms:
                                        sb.AppendLine($"- I do not have sufficient permissions ({bperms.Permissions.ToPermissionString()})!");
                                        break;
                                    case RequirePrivilegedUserAttribute _:
                                        sb.AppendLine($"- That command is reserved for my owner and privileged users!");
                                        break;
                                    case RequireOwnerAttribute _:
                                        sb.AppendLine($"- That command is reserved only for my owner!");
                                        break;
                                    case RequireNsfwAttribute _:
                                        sb.AppendLine($"- That command is allowed only in NSFW channels!");
                                        break;
                                    case RequirePrefixesAttribute pattr:
                                        sb.AppendLine($"- That command can only be invoked only with the following prefixes: {string.Join(" ", pattr.Prefixes)}!");
                                        break;
                                }
                            }
                            break;
                    }
                    break;
                case ConcurrentOperationException _:
                    sb.Append($"A concurrency error - please report this. Details: {ex.Message}");
                    break;
                case UnauthorizedException _:
                    sb.Append("I am not authorized to do that.");
                    break;
                case TargetInvocationException _:
                    sb.Append($"{ex.InnerException?.Message ?? "Target invocation error occured. Please check the arguments provided and try again."}");
                    break;
                case TaskCanceledException _:
                    return;
                default:
                    sb.AppendLine($"Command {Formatter.Bold(e.Command.QualifiedName)} errored!").AppendLine();
                    sb.AppendLine($"Exception: {Formatter.InlineCode(ex.GetType().ToString())}");
                    sb.AppendLine($"Details: {Formatter.Italic(ex.Message)}");
                    if (ex.InnerException != null) {
                        sb.AppendLine($"Inner exception: {Formatter.InlineCode(ex.InnerException.GetType().ToString())}");
                        sb.AppendLine($"Details: {Formatter.Italic(ex.InnerException.Message ?? "No details provided")}");
                    }
                    break;
            }

            emb.Description = sb.ToString();
            await e.Context.RespondAsync(embed: emb.Build());
        }
    }
}
