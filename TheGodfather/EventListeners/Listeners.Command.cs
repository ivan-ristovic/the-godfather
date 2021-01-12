using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.CommandsNext.Exceptions;
using DSharpPlus.Exceptions;
using Humanizer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using TheGodfather.Attributes;
using TheGodfather.Common;
using TheGodfather.EventListeners.Attributes;
using TheGodfather.EventListeners.Common;
using TheGodfather.Exceptions;
using TheGodfather.Extensions;
using TheGodfather.Modules.Administration.Services;
using TheGodfather.Services;
using TheGodfather.Services.Common;

namespace TheGodfather.EventListeners
{
    internal static partial class Listeners
    {
        [AsyncEventListener(DiscordEventType.CommandExecuted)]
        public static Task CommandExecutionEventHandler(TheGodfatherBot bot, CommandExecutionEventArgs e)
        {
            if (e.Command is null || e.Command.QualifiedName.StartsWith("help"))
                return Task.CompletedTask;
            LogExt.Information(
                bot.GetId(e.Context.Guild?.Id),
                new[] { "Executed: {ExecutedCommand}", "{User}", "{Guild}", "{Channel}" },
                e.Command.QualifiedName, e.Context.User, e.Context.Guild?.ToString() ?? "DM", e.Context.Channel
            );
            return Task.CompletedTask;
        }

        [AsyncEventListener(DiscordEventType.CommandErrored)]
        public static Task CommandErrorEventHandlerAsync(TheGodfatherBot bot, CommandErrorEventArgs e)
        {
            if (e.Exception is null)
                return Task.CompletedTask;

            Exception ex = e.Exception;
            while (ex is AggregateException or TargetInvocationException && ex.InnerException is { })
                ex = ex.InnerException;

            if (ex is ChecksFailedException chke && chke.FailedChecks.Any(c => c is NotBlockedAttribute))
                return e.Context.Message.CreateReactionAsync(Emojis.X);

            LogExt.Debug(
                bot.GetId(e.Context.Guild?.Id),
                new[] { "Command errored ({ExceptionName}): {ErroredCommand}", "{User}", "{Guild}", "{Channel}", "Message: {Message}" },
                e.Exception?.GetType().Name ?? "Unknown", e.Command?.QualifiedName ?? "Unknown",
                e.Context.User, e.Context.Guild?.ToString() ?? "DM", e.Context.Channel, ex.Message
            );

            LocalizationService lcs = bot.Services.GetRequiredService<LocalizationService>();

            ulong gid = e.Context.Guild?.Id ?? 0;
            var emb = new LocalizedEmbedBuilder(lcs, gid);
            emb.WithLocalizedTitle(DiscordEventType.CommandErrored, "cmd-err", desc: null, e.Command?.QualifiedName ?? "");

            switch (ex) {
                case CommandNotFoundException cne:
                    if (!bot.Services.GetRequiredService<GuildConfigService>().GetCachedConfig(gid).SuggestionsEnabled)
                        return e.Context.Message.CreateReactionAsync(Emojis.Question);

                    emb.WithLocalizedTitle(DiscordEventType.CommandErrored, "cmd-404", desc: null, cne.CommandName);

                    CommandService cs = bot.Services.GetRequiredService<CommandService>();
                    IEnumerable<KeyValuePair<string, Command>> similarCommands = bot.Commands
                        .Select(kvp => (cne.CommandName.LevenshteinDistanceTo(kvp.Key), kvp))
                        .Where(tup => tup.Item1 < 3)
                        .OrderBy(tup => tup.Item1)
                        .Select(tup => tup.kvp)
                        .Distinct(new CommandKeyValuePairComparer())
                        .Take(3);

                    if (similarCommands.Any()) {
                        emb.WithLocalizedDescription("q-did-you-mean");
                        foreach ((string cname, Command cmd) in similarCommands)
                            emb.AddField($"{cmd.QualifiedName} ({cname})", cs.GetCommandDescription(gid, cmd.QualifiedName));
                    }

                    break;
                case InvalidCommandUsageException icue:
                    emb.WithDescription(icue.LocalizedMessage);
                    emb.WithLocalizedFooter("fmt-help-cmd", iconUrl: null, e.Command?.QualifiedName ?? "");
                    break;
                case ArgumentException _:
                case TargetInvocationException _:
                case InvalidOperationException _:
                    string fcmdStr = $"help {e.Command?.QualifiedName ?? ""}";
                    CommandsNextExtension cnext = bot.CNext[bot.GetId(e.Context.Guild?.Id ?? 0)];
                    Command command = cnext.FindCommand(fcmdStr, out string args);
                    CommandContext fctx = cnext.CreateFakeContext(e.Context.User, e.Context.Channel, fcmdStr, e.Context.Prefix, command, args);
                    return cnext.ExecuteCommandAsync(fctx);
                case ConcurrentOperationException coex:
                    emb.WithLocalizedDescription("err-concurrent");
                    break;
                case BadRequestException brex:
                    emb.WithLocalizedDescription("cmd-err-bad-req", brex.JsonMessage);
                    break;
                case NotFoundException nfe:
                    emb.WithLocalizedDescription("cmd-err-404", nfe.JsonMessage);
                    break;
                case FormatException nfe:
                    emb.WithLocalizedDescription("cmd-err-loc");
                    break;
                case LocalizedException lex:
                    emb.WithDescription(lex.LocalizedMessage);
                    break;
                case ChecksFailedException cfex:
                    emb.WithLocalizedTitle(DiscordEventType.CommandErrored, "cmd-chk", desc: null, e.Command?.QualifiedName ?? "?");
                    var sb = new StringBuilder();
                    switch (cfex.FailedChecks[0]) {
                        case CooldownAttribute _:
                            return Task.CompletedTask;
                        case UsesInteractivityAttribute _:
                            sb.AppendLine(lcs.GetString(gid, "cmd-chk-inter"));
                            break;
                        default:
                            foreach (CheckBaseAttribute attr in cfex.FailedChecks) {
                                string line = attr switch {
                                    RequirePermissionsAttribute p => lcs.GetString(gid, "cmd-chk-perms", p.Permissions.ToPermissionString()),
                                    RequireUserPermissionsAttribute up => lcs.GetString(gid, "cmd-chk-perms-usr", up.Permissions.ToPermissionString()),
                                    RequireOwnerOrPermissionsAttribute op => lcs.GetString(gid, "cmd-chk-perms-usr", op.Permissions.ToPermissionString()),
                                    RequireBotPermissionsAttribute bp => lcs.GetString(gid, "cmd-chk-perms-bot", bp.Permissions.ToPermissionString()),
                                    RequirePrivilegedUserAttribute _ => lcs.GetString(gid, "cmd-chk-perms-priv"),
                                    RequireOwnerAttribute _ => lcs.GetString(gid, "cmd-chk-perms-own"),
                                    RequireNsfwAttribute _ => lcs.GetString(gid, "cmd-chk-perms-nsfw"),
                                    RequirePrefixesAttribute pattr => lcs.GetString(gid, "cmd-chk-perms-pfix", pattr.Prefixes.Humanize(", ")),
                                    RequireGuildAttribute _ => lcs.GetString(gid, "cmd-chk-perms-guild"),
                                    RequireDirectMessageAttribute _ => lcs.GetString(gid, "cmd-chk-perms-dm"),
                                    _ => lcs.GetString(gid, "cmd-chk-perms-attr", attr),
                                };
                                sb.Append("- ").AppendLine(line);
                            }
                            break;
                    }
                    emb.WithDescription(sb.ToString());
                    break;
                case UnauthorizedException _:
                    emb.WithLocalizedDescription("cmd-err-403");
                    break;
                case TaskCanceledException tcex:
                    LogExt.Warning(bot.GetId(e.Context.Guild?.Id), "Task cancelled");
                    return Task.CompletedTask;
                case NpgsqlException _:
                case DbUpdateException _:
                    emb.WithLocalizedDescription("err-db");
                    break;
                default:
                    LogExt.Error(bot.GetId(e.Context.Guild?.Id), ex, "Unhandled error");
                    break;
            }

            return e.Context.RespondAsync(embed: emb.Build());
        }
    }
}
