using System.Data.Common;
using System.Reflection;
using System.Text;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.CommandsNext.Exceptions;
using DSharpPlus.EventArgs;
using DSharpPlus.Exceptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TheGodfather.EventListeners.Attributes;
using TheGodfather.EventListeners.Common;
using TheGodfather.Modules.Administration.Services;
using TheGodfather.Services.Common;

namespace TheGodfather.EventListeners;

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
            bot.GetId(e.Context.Guild?.Id), e.Exception,
            new[] { "Command errored ({ExceptionName}): {ErroredCommand}", "{User}", "{Guild}", "{Channel}" },
            e.Exception?.GetType().Name ?? "Unknown", e.Command?.QualifiedName ?? "Unknown",
            e.Context.User, e.Context.Guild?.ToString() ?? "DM", e.Context.Channel
        );

        LocalizationService lcs = bot.Services.GetRequiredService<LocalizationService>();

        ulong gid = e.Context.Guild?.Id ?? 0;
        var emb = new LocalizedEmbedBuilder(lcs, gid);
        emb.WithLocalizedTitle(DiscordEventType.CommandErrored, TranslationKey.cmd_err(e.Command?.QualifiedName ?? ""));

        switch (ex) {
            case CommandNotFoundException cne:
                if (!bot.Services.GetRequiredService<GuildConfigService>().GetCachedConfig(gid).SuggestionsEnabled)
                    return e.Context.Message.CreateReactionAsync(Emojis.Question);

                emb.WithLocalizedTitle(DiscordEventType.CommandErrored, TranslationKey.cmd_404(cne.CommandName));

                CommandService cs = bot.Services.GetRequiredService<CommandService>();
                IEnumerable<KeyValuePair<string, Command>> similarCommands = bot.Commands
                    .Select(kvp => (cne.CommandName.LevenshteinDistanceTo(kvp.Key), kvp))
                    .Where(tup => tup.Item1 < 3)
                    .OrderBy(tup => tup.Item1)
                    .Select(tup => tup.kvp)
                    .Distinct(new CommandKeyValuePairComparer())
                    .Take(3)
                    .ToList();

                if (similarCommands.Any()) {
                    emb.WithLocalizedDescription(TranslationKey.q_did_you_mean);
                    foreach ((string cname, Command cmd) in similarCommands)
                        emb.AddField($"{cmd.QualifiedName} ({cname})", cs.GetCommandDescription(gid, cmd.QualifiedName));
                }

                break;
            case InvalidCommandUsageException icue:
                emb.WithDescription(icue.LocalizedMessage);
                emb.WithLocalizedFooter(TranslationKey.fmt_help_cmd(e.Command?.QualifiedName ?? ""), null);
                break;
            case ArgumentException _:
            case TargetInvocationException _:
            case InvalidOperationException _:
                string fcmdStr = $"help {e.Command?.QualifiedName ?? ""}";
                CommandsNextExtension cnext = bot.CNext[bot.GetId(e.Context.Guild?.Id ?? 0)];
                Command? command = cnext.FindCommand(fcmdStr, out string? args);
                if (command is null)
                    break;
                CommandContext fctx = cnext.CreateFakeContext(e.Context.User, e.Context.Channel, fcmdStr, e.Context.Prefix, command, args);
                return cnext.ExecuteCommandAsync(fctx);
            case ConcurrentOperationException coex:
                emb.WithLocalizedDescription(TranslationKey.err_concurrent);
                break;
            case BadRequestException brex:
                emb.WithLocalizedDescription(TranslationKey.cmd_err_bad_req(brex.JsonMessage));
                break;
            case NotFoundException nfe:
                emb.WithLocalizedDescription(TranslationKey.cmd_err_404(nfe.JsonMessage));
                break;
            case FormatException:
                emb.WithLocalizedDescription(TranslationKey.cmd_err_loc);
                break;
            case LocalizedException lex:
                emb.WithDescription(lex.LocalizedMessage);
                break;
            case ChecksFailedException cfex:
                emb.WithLocalizedTitle(DiscordEventType.CommandErrored, TranslationKey.cmd_chk(e.Command?.QualifiedName ?? "?"));
                var sb = new StringBuilder();
                switch (cfex.FailedChecks[0]) {
                    case CooldownAttribute _:
                        return Task.CompletedTask;
                    case UsesInteractivityAttribute _:
                        sb.AppendLine(lcs.GetString(gid, TranslationKey.cmd_chk_inter));
                        break;
                    default:
                        foreach (CheckBaseAttribute attr in cfex.FailedChecks) {
                            string line = attr switch {
                                RequirePermissionsAttribute p => lcs.GetString(gid, TranslationKey.cmd_chk_perms(p.Permissions.ToPermissionString())),
                                RequireUserPermissionsAttribute up => lcs.GetString(gid, TranslationKey.cmd_chk_perms_usr(up.Permissions.ToPermissionString())),
                                RequireOwnerOrPermissionsAttribute op => lcs.GetString(gid, TranslationKey.cmd_chk_perms_usr(op.Permissions.ToPermissionString())),
                                RequireBotPermissionsAttribute bp => lcs.GetString(gid, TranslationKey.cmd_chk_perms_bot(bp.Permissions.ToPermissionString())),
                                RequirePrivilegedUserAttribute _ => lcs.GetString(gid, TranslationKey.cmd_chk_perms_priv),
                                RequireOwnerAttribute _ => lcs.GetString(gid, TranslationKey.cmd_chk_perms_own),
                                RequireNsfwAttribute _ => lcs.GetString(gid, TranslationKey.cmd_chk_perms_nsfw),
                                RequirePrefixesAttribute pattr => lcs.GetString(gid, TranslationKey.cmd_chk_perms_pfix(pattr.Prefixes.Humanize(", "))),
                                RequireGuildAttribute _ => lcs.GetString(gid, TranslationKey.cmd_chk_perms_guild),
                                RequireDirectMessageAttribute _ => lcs.GetString(gid, TranslationKey.cmd_chk_dm),
                                _ => lcs.GetString(gid, TranslationKey.cmd_chk_attr(attr))
                            };
                            sb.Append("- ").AppendLine(line);
                        }
                        break;
                }
                emb.WithDescription(sb.ToString());
                break;
            case UnauthorizedException _:
                emb.WithLocalizedDescription(TranslationKey.cmd_err_403);
                break;
            case TaskCanceledException tcex:
                return Task.CompletedTask;
            case DbException _:
            case DbUpdateException _:
                emb.WithLocalizedDescription(TranslationKey.err_db);
                break;
            case CommandCancelledException:
                break;
            default:
                LogExt.Error(bot.GetId(e.Context.Guild?.Id), ex, "Unhandled error");
                break;
        }

        return e.Context.RespondAsync(emb.Build());
    }

    [AsyncEventListener(DiscordEventType.ComponentInteractionCreated)]
    public static Task ComponentInteractionCreateEventHandlerAsync(TheGodfatherBot bot, ComponentInteractionCreateEventArgs e)
    {
        LogExt.Debug(
            bot.GetId(e.Guild?.Id),
            new[] { "Component interaction created: {Interaction}", "{User}", "{Guild}", "{Channel}", "{Message}", "{Values}" },
            e.Interaction?.ToString() ?? "Unknown", e.User, e.Guild?.ToString() ?? "DM", e.Channel, e.Message, e.Values?.ToString() ?? "None"
        );
        return Task.CompletedTask;
    }
}