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
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;
using Humanizer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using Serilog;
using TheGodfather.Attributes;
using TheGodfather.Common;
using TheGodfather.EventListeners.Attributes;
using TheGodfather.EventListeners.Common;
using TheGodfather.Exceptions;
using TheGodfather.Extensions;
using TheGodfather.Modules.Administration.Services;
using TheGodfather.Services;

namespace TheGodfather.EventListeners
{
    internal static partial class Listeners
    {
        [AsyncEventListener(DiscordEventType.CommandExecuted)]
        public static Task CommandExecutionEventHandler(TheGodfatherShard shard, CommandExecutionEventArgs e)
        {
            if (e.Command is null || e.Command.QualifiedName == "help")
                return Task.CompletedTask;

            LogExt.Information(
                shard.Id,
                new[] { "Executed: {ExecutedCommand}", "{User}", "{Guild}", "{Channel}" },
                e.Command.QualifiedName, e.Context.User, e.Context.Guild?.ToString() ?? "DM", e.Context.Channel
            );
            return Task.CompletedTask;
        }

        [AsyncEventListener(DiscordEventType.CommandErrored)]
        public static Task CommandErrorEventHandlerAsync(TheGodfatherShard shard, CommandErrorEventArgs e)
        {
            LogExt.Verbose(e.Context, e.Exception, "Command errored");
            if (e.Exception is null)
                return Task.CompletedTask;

            Exception ex = e.Exception;
            while (ex is AggregateException || ex is TargetInvocationException)
                ex = ex.InnerException ?? ex;

            if (ex is ChecksFailedException chke && chke.FailedChecks.Any(c => c is NotBlockedAttribute))
                return e.Context.Message.CreateReactionAsync(Emojis.X);

            LogExt.Information(e.Context, ex, "Tried executing {AttemptedCommand}", e.Command?.QualifiedName ?? "Unknown command");

            LocalizationService lcs = shard.Services.GetRequiredService<LocalizationService>();

            var emb = new LocalizedEmbedBuilder(lcs, e.Context.Guild.Id);
            emb.WithLocalizedTitle(DiscordEventType.CommandErrored, "cmd-err", desc: null, e.Command?.QualifiedName ?? "");

            switch (ex) {
                case CommandNotFoundException cne:
                    if (!shard.Services.GetRequiredService<GuildConfigService>().GetCachedConfig(e.Context.Guild.Id)?.SuggestionsEnabled ?? true)
                        return e.Context.Message.CreateReactionAsync(Emojis.Question);

                    emb.WithLocalizedTitle(DiscordEventType.CommandErrored, "cmd-404", desc: null, cne.CommandName);

                    CommandService cs = shard.Services.GetRequiredService<CommandService>();
                    IEnumerable<string> ordered = cs.AvailableCommands
                        .OrderBy(c => cne.CommandName.LevenshteinDistance(c))
                        .Take(3);
                    foreach (string cmd in ordered)
                        emb.AddField(cmd, cs.GetCommandDescription(e.Context.Guild.Id, cmd));

                    break;
                case InvalidCommandUsageException icue:
                    emb.WithDescription(icue.LocalizedMessage);
                    emb.WithLocalizedFooter("msg-help-cmd", iconUrl: null, e.Command?.QualifiedName ?? "");
                    break;
                case ArgumentException _:
                case TargetInvocationException _:
                    if (shard.CNext is null)
                        throw new InvalidOperationException("CNext is null");
                    string fcmdStr = $"help {e.Command?.QualifiedName ?? ""}";
                    Command command = shard.CNext.FindCommand(fcmdStr, out string args);
                    CommandContext fctx = shard.CNext.CreateFakeContext(e.Context.User, e.Context.Channel, fcmdStr, e.Context.Prefix, command, args);
                    return shard.CNext.ExecuteCommandAsync(fctx);
                case BadRequestException brex:
                    emb.WithLocalizedDescription("cmd-err-bad-req", brex.JsonMessage);
                    break;
                case NotFoundException nfe:
                    emb.WithLocalizedDescription("cmd-err-404", nfe.JsonMessage);
                    break;
                case LocalizedException lex:
                    emb.WithDescription(lex.LocalizedMessage);
                    break;
                case ChecksFailedException cfex:
                    emb.WithLocalizedTitle(DiscordEventType.CommandErrored, "cmd-chk", desc: null, e.Command?.QualifiedName ?? "?");
                    var sb = new StringBuilder();
                    switch (cfex.FailedChecks.First()) {
                        case CooldownAttribute _:
                            break;
                        case UsesInteractivityAttribute _:
                            sb.AppendLine(lcs.GetString(e.Context.Guild.Id, "cmd-chk-inter"));
                            break;
                        default:
                            foreach (CheckBaseAttribute attr in cfex.FailedChecks) {
                                string line = attr switch
                                {
                                    RequirePermissionsAttribute p => lcs.GetString(e.Context.Guild.Id, "cmd-chk-perms", p.Permissions.ToPermissionString()),
                                    RequireUserPermissionsAttribute up => lcs.GetString(e.Context.Guild.Id, "cmd-chk-perms-usr", up.Permissions.ToPermissionString()),
                                    RequireOwnerOrPermissionsAttribute op => lcs.GetString(e.Context.Guild.Id, "cmd-chk-perms-usr", op.Permissions.ToPermissionString()),
                                    RequireBotPermissionsAttribute bp => lcs.GetString(e.Context.Guild.Id, "cmd-chk-perms-bot", bp.Permissions.ToPermissionString()),
                                    RequirePrivilegedUserAttribute _ => lcs.GetString(e.Context.Guild.Id, "cmd-chk-perms-priv"),
                                    RequireOwnerAttribute _ => lcs.GetString(e.Context.Guild.Id, "cmd-chk-perms-own"),
                                    RequireNsfwAttribute _ => lcs.GetString(e.Context.Guild.Id, "cmd-chk-perms-nsfw"),
                                    RequirePrefixesAttribute pattr => lcs.GetString(e.Context.Guild.Id, "cmd-chk-perms-pfix", pattr.Prefixes.Humanize(", ")),
                                    RequireGuildAttribute _ => lcs.GetString(e.Context.Guild.Id, "cmd-chk-perms-guild"),
                                    RequireDirectMessageAttribute _ => lcs.GetString(e.Context.Guild.Id, "cmd-chk-perms-dm"),
                                    _ => lcs.GetString(e.Context.Guild.Id, "cmd-chk-perms-attr", attr),
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
                    LogExt.Warning(shard.Id, "Task cancelled");
                    return Task.CompletedTask;
                case NpgsqlException _:
                case DbUpdateException _:
                    emb.WithLocalizedDescription("err-db");
                    break;
                default:
                    LogExt.Warning(shard.Id, "Unhandled error");
                    break;
            }

            return e.Context.RespondAsync(embed: emb.Build());
        }
    }
}
