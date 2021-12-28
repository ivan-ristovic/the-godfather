using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Data.Common;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Serilog.Events;
using TheGodfather.Modules.Owner.Common;
using TheGodfather.Modules.Owner.Extensions;
using TheGodfather.Modules.Owner.Services;
using TheGodfather.Services.Common;

namespace TheGodfather.Modules.Owner;

[Group("owner")][Module(ModuleType.Owner)][Hidden]
[Aliases("admin", "o")]
public sealed class OwnerModule : TheGodfatherModule
{
    #region announce
    [Command("announce")][UsesInteractivity]
    [Aliases("ann")]
    [RequireOwner]
    public async Task AnnounceAsync(CommandContext ctx,
        [RemainingText][Description(TranslationKey.desc_announcement)] string message)
    {
        if (!await ctx.WaitForBoolReplyAsync(TranslationKey.q_announcement(Formatter.Strip(message))))
            return;

        var emb = new LocalizedEmbedBuilder(this.Localization, ctx.Guild?.Id);
        emb.WithLocalizedTitle(TranslationKey.str_announcement);
        emb.WithDescription(message);
        emb.WithColor(DiscordColor.Red);

        var eb = new StringBuilder();
        IEnumerable<(int, IEnumerable<DiscordGuild>)> shardGuilds = TheGodfather.Bot!.Client.ShardClients
            .Select(kvp => (kvp.Key, kvp.Value.Guilds.Values));
        foreach ((int shardId, IEnumerable<DiscordGuild> guilds) in shardGuilds)
        foreach (DiscordGuild guild in guilds)
            try {
                await guild.GetDefaultChannel().SendMessageAsync(emb.Build());
            } catch {
                eb.AppendLine(this.Localization.GetString(ctx.Guild?.Id, TranslationKey.cmd_err_announce(shardId, guild.Name, guild.Id)));
            }

        if (eb.Length > 0)
            await ctx.ImpInfoAsync(this.ModuleColor, TranslationKey.fmt_err(eb.ToString()));
        else
            await ctx.InfoAsync(this.ModuleColor);
    }
    #endregion

    #region avatar
    [Command("avatar")]
    [Aliases("setavatar", "setbotavatar", "profilepic", "a")]
    [RequireOwner]
    public async Task SetBotAvatarAsync(CommandContext ctx,
        [Description(TranslationKey.desc_image_url)] Uri url)
    {
        if (!await url.ContentTypeHeaderIsImageAsync(DiscordLimits.AvatarSizeLimit))
            throw new CommandFailedException(ctx, TranslationKey.cmd_err_image_url_fail(DiscordLimits.AvatarSizeLimit));

        try {
            await using MemoryStream ms = await HttpService.GetMemoryStreamAsync(url);
            await ctx.Client.UpdateCurrentUserAsync(avatar: ms);
        } catch (WebException e) {
            throw new CommandFailedException(ctx, e, TranslationKey.err_url_image_fail);
        }

        await ctx.InfoAsync(this.ModuleColor);
    }
    #endregion

    #region name
    [Command("name")]
    [Aliases("botname", "setbotname", "setname")]
    [RequireOwner]
    public async Task SetBotNameAsync(CommandContext ctx,
        [RemainingText][Description(TranslationKey.desc_name_new)] string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new InvalidCommandUsageException(ctx, TranslationKey.cmd_err_missing_name);

        if (name.Length > DiscordLimits.NameLimit)
            throw new InvalidCommandUsageException(ctx, TranslationKey.cmd_err_name(DiscordLimits.NameLimit));

        await ctx.Client.UpdateCurrentUserAsync(name);
        await ctx.InfoAsync(this.ModuleColor);
    }
    #endregion

    #region dbquery
    [Command("dbquery")][Priority(1)]
    [Aliases("sql", "dbq", "q", "query")]
    [RequireOwner]
    public async Task DatabaseQuery(CommandContext ctx)
    {
        if (!ctx.Message.Attachments.Any())
            throw new CommandFailedException(ctx, TranslationKey.cmd_err_dbq_sql_att);

        DiscordAttachment? attachment = ctx.Message.Attachments.FirstOrDefault(att => att.FileName.EndsWith(".sql"));
        if (attachment is null)
            throw new CommandFailedException(ctx, TranslationKey.cmd_err_dbq_sql_att_none);

        string query;
        try {
            query = await HttpService.GetStringAsync(attachment.Url).ConfigureAwait(false);
        } catch (Exception e) {
            throw new CommandFailedException(ctx, e, TranslationKey.err_attachment);
        }

        await this.DatabaseQuery(ctx, query);
    }

    [Command("dbquery")][Priority(0)]
    public async Task DatabaseQuery(CommandContext ctx,
        [RemainingText][Description(TranslationKey.desc_sql)] string query)
    {
        if (string.IsNullOrWhiteSpace(query))
            throw new InvalidCommandUsageException(ctx, TranslationKey.cmd_err_dbq_sql_none);

        var res = new List<IReadOnlyDictionary<string, string>>();
        await using (TheGodfatherDbContext db = ctx.Services.GetRequiredService<DbContextBuilder>().CreateContext())
        await using (RelationalDataReader dr = await db.Database.ExecuteSqlQueryAsync(query, db)) {
            DbDataReader reader = dr.DbDataReader;
            while (await reader.ReadAsync()) {
                var dict = new Dictionary<string, string>();

                for (int i = 0; i < reader.FieldCount; i++)
                    dict[reader.GetName(i)] = reader[i] is DBNull ? "NULL" : reader[i].ToString() ?? "NULL";

                res.Add(new ReadOnlyDictionary<string, string>(dict));
            }
        }

        if (!res.Any() || !res.First().Any()) {
            await ctx.ImpInfoAsync(this.ModuleColor, Emojis.Information, TranslationKey.str_dbq_none);
            return;
        }

        int maxlen = 1 + res
            .First()
            .Select(r => r.Key)
            .OrderByDescending(r => r.Length)
            .First()
            .Length;

        await ctx.PaginateAsync(
            TranslationKey.str_dbq_res,
            res.Take(25),
            row => {
                var sb = new StringBuilder();
                foreach ((string col, string val) in row)
                    sb.Append(col).Append(new string(' ', maxlen - col.Length)).Append("| ").AppendLine(val);
                return Formatter.BlockCode(sb.ToString());
            },
            this.ModuleColor,
            1
        );
    }
    #endregion

    #region eval
    [Command("eval")]
    [Aliases("evaluate", "compile", "run", "e", "c", "r", "exec")]
    [RequireOwner]
    public async Task EvaluateAsync(CommandContext ctx,
        [RemainingText][Description(TranslationKey.desc_code)] string code)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new InvalidCommandUsageException(ctx, TranslationKey.cmd_err_cmd_add_cb);

        DiscordMessage msg = await ctx.RespondWithLocalizedEmbedAsync(emb => {
            emb.WithLocalizedTitle(TranslationKey.str_eval);
            emb.WithColor(this.ModuleColor);
        });

        Script<object>? snippet = CSharpCompilationService.Compile(code, out ImmutableArray<Diagnostic> diag, out Stopwatch compileTime);
        if (snippet is null) {
            await msg.DeleteAsync();
            throw new InvalidCommandUsageException(ctx, TranslationKey.cmd_err_cmd_add_cb);
        }

        var emb = new LocalizedEmbedBuilder(this.Localization, ctx.Guild?.Id);

        if (diag.Any(d => d.Severity == DiagnosticSeverity.Error)) {
            emb.WithLocalizedTitle(TranslationKey.str_eval_fail_compile);
            emb.WithLocalizedDescription(TranslationKey.fmt_eval_fail_compile(compileTime.ElapsedMilliseconds, diag.Length));
            emb.WithColor(DiscordColor.Red);

            foreach (Diagnostic d in diag.Take(3)) {
                FileLinePositionSpan ls = d.Location.GetLineSpan();
                emb.AddLocalizedField(TranslationKey.fmt_eval_err(ls.StartLinePosition.Line, ls.StartLinePosition.Character), Formatter.BlockCode(d.GetMessage()));
            }

            if (diag.Length > 3)
                emb.AddLocalizedField(TranslationKey.str_eval_omit, TranslationKey.fmt_eval_omit(diag.Length - 3));

            await UpdateOrRespondAsync();
            return;
        }

        Exception? exc = null;
        ScriptState<object>? res = null;
        var runTime = Stopwatch.StartNew();
        try {
            res = await snippet.RunAsync(new EvaluationEnvironment(ctx));
        } catch (Exception e) {
            exc = e;
        }
        runTime.Stop();

        if (exc is { } || res is null) {
            emb.WithLocalizedTitle(TranslationKey.str_eval_fail_run);
            emb.WithLocalizedDescription(TranslationKey.fmt_eval_fail_run(runTime.ElapsedMilliseconds, exc?.GetType(), exc?.Message));
            emb.WithColor(DiscordColor.Red);
        } else {
            emb.WithLocalizedTitle(TranslationKey.str_eval_succ);
            emb.WithColor(this.ModuleColor);
            if (res.ReturnValue is { }) {
                emb.AddLocalizedField(TranslationKey.str_result, res.ReturnValue);
                emb.AddLocalizedField(TranslationKey.str_result_type, res.ReturnValue.GetType(), true);
            } else {
                emb.AddLocalizedField(TranslationKey.str_result, TranslationKey.str_eval_value_none, inline: true);
            }
            emb.AddLocalizedField(TranslationKey.str_eval_time_compile, compileTime.ElapsedMilliseconds, true);
            emb.AddLocalizedField(TranslationKey.str_eval_time_run, runTime.ElapsedMilliseconds, true);
        }

        await UpdateOrRespondAsync();


        Task UpdateOrRespondAsync()
            => msg is { } ? msg.ModifyAsync(emb.Build()) : ctx.RespondAsync(emb.Build());
    }
    #endregion

    #region generatecommandlist
    [Command("generatecommandlist")]
    [Aliases("gendocs", "generatecommandslist", "docs", "cmdlist", "gencmdlist", "gencmds", "gencmdslist")]
    [RequireOwner]
    public async Task GenerateCommandListAsync(CommandContext ctx,
        [RemainingText][Description(TranslationKey.desc_folder)] string? path = null)
    {
        if (string.IsNullOrWhiteSpace(path))
            path = "Documentation";

        DirectoryInfo current;
        DirectoryInfo parts;
        try {
            if (Directory.Exists(path))
                Directory.Delete(path, true);
            current = Directory.CreateDirectory(path);
            parts = Directory.CreateDirectory(Path.Combine(current.FullName, "Parts"));
        } catch (IOException e) {
            LogExt.Error(ctx, e, "Failed to delete/create documentation directory");
            throw new CommandFailedException(ctx, TranslationKey.cmd_err_doc_clean);
        }

        CommandService cs = ctx.Services.GetRequiredService<CommandService>();

        var sb = new StringBuilder();
        sb.AppendLine("# Command list");
        sb.AppendLine();

        IReadOnlyList<Command> commands = ctx.CommandsNext.GetRegisteredCommands();
        var modules = commands
            .GroupBy(c => ModuleAttribute.AttachedTo(c))
            .OrderBy(g => g.Key.Module)
            .ToDictionary(g => g.Key, g => g.OrderBy(c => c.QualifiedName).ToList());

        foreach ((ModuleAttribute mattr, List<Command> cmdlist) in modules) {
            sb.Append("# Module: ").Append(mattr.Module.ToString()).AppendLine();
            sb.AppendLine(Formatter.Italic(this.Localization.GetStringUnsafe(null, $"{mattr.Module.ToLocalizedDescriptionKey()}-raw")));
            sb.AppendLine().AppendLine();

            foreach (Command cmd in cmdlist) {
                if (cmd is CommandGroup || cmd.Parent is null)
                    sb.Append("## ").Append(cmd is CommandGroup ? "Group: " : "").AppendLine(cmd.QualifiedName);
                else
                    sb.Append("### ").AppendLine(cmd.QualifiedName);

                sb.AppendLine("<details><summary markdown='span'>Expand for additional information</summary><p>").AppendLine();

                if (cmd.IsHidden)
                    sb.AppendLine(Formatter.Italic("Hidden.")).AppendLine();

                sb.AppendLine(Formatter.Italic(this.Localization.GetCommandDescription(null, cmd.QualifiedName))).AppendLine();

                IEnumerable<CheckBaseAttribute> execChecks = cmd.ExecutionChecks.AsEnumerable();
                CommandGroup? parent = cmd.Parent;
                while (parent is { }) {
                    execChecks = execChecks.Union(parent.ExecutionChecks);
                    parent = parent.Parent;
                }

                IEnumerable<string> perms = execChecks
                    .Where(chk => chk is RequirePermissionsAttribute)
                    .Cast<RequirePermissionsAttribute>()
                    .Select(chk => chk.Permissions.ToPermissionString())
                    .Union(execChecks
                        .Where(chk => chk is RequireOwnerOrPermissionsAttribute)
                        .Cast<RequireOwnerOrPermissionsAttribute>()
                        .Select(chk => chk.Permissions.ToPermissionString())
                    );
                IEnumerable<string> uperms = execChecks
                    .Where(chk => chk is RequireUserPermissionsAttribute)
                    .Cast<RequireUserPermissionsAttribute>()
                    .Select(chk => chk.Permissions.ToPermissionString());
                IEnumerable<string> bperms = execChecks
                    .Where(chk => chk is RequireBotPermissionsAttribute)
                    .Cast<RequireBotPermissionsAttribute>()
                    .Select(chk => chk.Permissions.ToPermissionString());

                if (execChecks.Any(chk => chk is RequireGuildAttribute))
                    sb.AppendLine(Formatter.Bold("Guild only.")).AppendLine();
                if (execChecks.Any(chk => chk is RequireDirectMessageAttribute))
                    sb.AppendLine(Formatter.Bold("DM only.")).AppendLine();
                if (execChecks.Any(chk => chk is RequireOwnerAttribute))
                    sb.AppendLine(Formatter.Bold("Owner-only.")).AppendLine();
                if (execChecks.Any(chk => chk is RequirePrivilegedUserAttribute))
                    sb.AppendLine(Formatter.Bold("Privileged users only.")).AppendLine();

                if (perms.Any()) {
                    sb.AppendLine(Formatter.Bold("Requires permissions:"));
                    sb.Append('`').AppendJoin(", ", perms).Append('`').AppendLine();
                }
                if (uperms.Any()) {
                    sb.AppendLine(Formatter.Bold("Requires user permissions:"));
                    sb.Append('`').AppendJoin(", ", uperms).Append('`').AppendLine();
                }
                if (bperms.Any()) {
                    sb.AppendLine(Formatter.Bold("Requires bot permissions:"));
                    sb.Append('`').AppendJoin(", ", bperms).Append('`').AppendLine();
                }
                sb.AppendLine();

                if (cmd.Aliases.Any()) {
                    sb.AppendLine(Formatter.Bold("Aliases:"));
                    sb.Append('`').AppendJoin(", ", cmd.Aliases).Append('`').AppendLine().AppendLine();
                }

                foreach (CommandOverload overload in cmd.Overloads.OrderByDescending(o => o.Priority)) {
                    sb.AppendLine(Formatter.Bold($"Overload {overload.Priority}:"));
                    if (overload.Arguments.Any())
                        foreach (CommandArgument arg in overload.Arguments) {
                            sb.Append("- ");

                            if (arg.IsOptional)
                                sb.Append("(optional) ");

                            sb.Append(@"\[`").Append(ctx.CommandsNext.GetUserFriendlyTypeName(arg.Type));
                            if (arg.IsCatchAll)
                                sb.Append("...");
                            sb.Append(@"`\]: *");
                            if (string.IsNullOrWhiteSpace(arg.Description))
                                sb.Append("No description provided.");
                            else
                                sb.Append(this.Localization.GetStringUnsafe(null, arg.Description));
                            sb.Append('*');

                            if (arg.IsOptional) {
                                sb.Append(" (def: `");
                                if (arg.DefaultValue is null)
                                    sb.Append("None`)");
                                else
                                    sb.Append(arg.DefaultValue).Append("`)");
                            }

                            sb.AppendLine();
                        }
                    else
                        sb.AppendLine().AppendLine(Formatter.Italic("No arguments."));

                    sb.AppendLine();
                }

                if (cmd is not CommandGroup || (cmd is CommandGroup group && group.IsExecutableWithoutSubcommands)) {
                    IReadOnlyList<string> examples = cs.GetCommandUsageExamples(null, cmd.QualifiedName);
                    if (examples.Any())
                        sb.AppendLine(Formatter.Bold("Examples:")).AppendLine().AppendLine(Formatter.BlockCode(examples.JoinWith(), "xml"));
                }

                sb.AppendLine("</p></details>").AppendLine().AppendLine("---").AppendLine();
            }

            string filename = Path.Combine(parts.FullName, $"{mattr.Module}.md");
            try {
                File.WriteAllText(filename, sb.ToString());
            } catch (IOException e) {
                LogExt.Error(ctx, e, "Failed to delete/create documentation file {Filename}", filename);
                throw new CommandFailedException(ctx, TranslationKey.cmd_err_doc_save(filename));
            }

            sb.Clear();
        }

        sb.AppendLine("# Command modules:");
        foreach ((ModuleAttribute mattr, List<Command> _) in modules) {
            string mname = mattr.Module.ToString();
            sb.Append("  - ").Append('[').Append(mname).Append(']').Append('(').Append(parts.Name).Append('/').Append(mname).Append(".md").AppendLine(")");
        }

        string mainDocFilename = Path.Combine(current.FullName, "README.md");
        try {
            File.WriteAllText(mainDocFilename, sb.ToString());
        } catch (IOException e) {
            LogExt.Error(ctx, e, "Failed to delete/create documentation file {Filename}", mainDocFilename);
            throw new CommandFailedException(ctx, TranslationKey.cmd_err_doc_save(mainDocFilename));
        }

        await ctx.InfoAsync(this.ModuleColor);
    }
    #endregion

    #region leaveguilds
    [Command("leaveguilds")][Priority(1)]
    [Aliases("leave", "gtfo")]
    [RequireOwner]
    public Task LeaveGuildsAsync(CommandContext ctx,
        [Description(TranslationKey.desc_guilds)] params DiscordGuild[] guilds)
        => this.LeaveGuildsAsync(ctx, guilds.Select(g => g.Id).ToArray());

    [Command("leaveguilds")][Priority(0)]
    public async Task LeaveGuildsAsync(CommandContext ctx,
        [Description(TranslationKey.desc_guilds)] params ulong[] gids)
    {
        if (gids is null || !gids.Any())
            throw new InvalidCommandUsageException(ctx, TranslationKey.cmd_err_ids_none);

        var eb = new StringBuilder();
        foreach (ulong gid in gids)
            try {
                if (ctx.Client.Guilds.TryGetValue(gid, out DiscordGuild? guild))
                    await guild.LeaveAsync();
                else
                    eb.AppendLine(this.Localization.GetString(ctx.Guild?.Id, TranslationKey.cmd_err_guild_leave(gid)));
            } catch {
                eb.AppendLine(this.Localization.GetString(ctx.Guild?.Id, TranslationKey.cmd_err_guild_leave_fail(gid)));
            }

        if (ctx.Guild is { } && !gids.Contains(ctx.Guild.Id)) {
            if (eb.Length > 0)
                await ctx.FailAsync(TranslationKey.fmt_err(eb));
            else
                await ctx.InfoAsync(this.ModuleColor);
        } else {
            await ctx.InfoAsync(this.ModuleColor);
        }
    }
    #endregion

    #region log
    [Command("log")][Priority(1)][UsesInteractivity]
    [Aliases("getlog", "remark", "rem")]
    [RequireOwner]
    public async Task LogAsync(CommandContext ctx,
        [Description(TranslationKey.desc_log_bp)] bool bypassConfig = false)
    {
        BotConfig cfg = ctx.Services.GetRequiredService<BotConfigService>().CurrentConfiguration;

        if (!bypassConfig && !cfg.LogToFile)
            throw new CommandFailedException(ctx, TranslationKey.cmd_err_log_off);

        var fi = new FileInfo(cfg.LogPath);
        if (fi.Exists) {
            fi = new FileInfo(cfg.LogPath);
            if (fi.Length > DiscordLimits.AttachmentLimit)
                throw new CommandFailedException(ctx, TranslationKey.cmd_err_log_size(fi.Name, fi.Length.Megabytes().Humanize()));
        } else {
            DirectoryInfo? di = fi.Directory;
            if (di?.Exists ?? false) {
                var fis = di.GetFiles()
                        .OrderByDescending(fi => fi.CreationTime)
                        .Select((fi, i) => (fi, i))
                        .ToDictionary(tup => tup.i, tup => tup.fi)
                    ;
                if (!fis.Any())
                    throw new CommandFailedException(ctx, TranslationKey.cmd_err_log_404(cfg.LogPath));

                await ctx.PaginateAsync(
                    TranslationKey.q_log_select,
                    fis,
                    kvp => Formatter.InlineCode($"{kvp.Key:D3}: {kvp.Value.Name}"),
                    this.ModuleColor
                );

                int? index = await ctx.Client.GetInteractivity().WaitForOptionReplyAsync(ctx, fis.Count);
                if (index is null)
                    return;

                if (!fis.TryGetValue(index.Value, out fi))
                    throw new CommandFailedException(ctx, TranslationKey.cmd_err_log_404(cfg.LogPath));
            } else {
                throw new CommandFailedException(ctx, TranslationKey.cmd_err_log_404(cfg.LogPath));
            }
        }

        await using FileStream fs = fi.OpenRead();
        await ctx.RespondAsync(new DiscordMessageBuilder().WithFile(fs));
    }

    [Command("log")][Priority(0)]
    public Task LogAsync(CommandContext ctx,
        [Description(TranslationKey.desc_log_lvl)] LogEventLevel level,
        [RemainingText][Description(TranslationKey.desc_log_msg)] string text)
    {
        Log.Write(level, "{LogRemark}", text);
        return ctx.InfoAsync(this.ModuleColor);
    }
    #endregion

    #region sendmessage
    [Command("sendmessage")]
    [Aliases("send", "sendmsg", "s")]
    [RequirePrivilegedUser]
    public async Task SendAsync(CommandContext ctx,
        [Description(TranslationKey.desc_send)] string desc,
        [Description(TranslationKey.desc_id)] ulong xid,
        [RemainingText][Description(TranslationKey.desc_send_msg)] string message)
    {
        if (string.IsNullOrWhiteSpace(message))
            throw new InvalidCommandUsageException(ctx, TranslationKey.cmd_err_text_none);

        if (string.Equals(desc, "u", StringComparison.InvariantCultureIgnoreCase)) {
            DiscordDmChannel? dm = await ctx.Client.CreateDmChannelAsync(xid);
            if (dm is null)
                throw new CommandFailedException(ctx, TranslationKey.cmd_err_dm_create);
            await dm.SendMessageAsync(message);
        } else if (string.Equals(desc, "c", StringComparison.InvariantCultureIgnoreCase)) {
            DiscordChannel channel = await ctx.Client.GetChannelAsync(xid);
            await channel.SendMessageAsync(message);
        } else {
            throw new InvalidCommandUsageException(ctx);
        }

        await ctx.InfoAsync(this.ModuleColor);
    }
    #endregion

    #region shutdown
    [Command("shutdown")][Priority(1)]
    [Aliases("disable", "poweroff", "exit", "quit")]
    [RequirePrivilegedUser]
    public Task ExitAsync(CommandContext _,
        [Description(TranslationKey.desc_exit_time)] TimeSpan timespan,
        [Description(TranslationKey.desc_exit_code)] int exitCode = 0)
        => TheGodfather.Stop(exitCode, timespan);

    [Command("shutdown")][Priority(0)]
    public Task ExitAsync(CommandContext _,
        [Description(TranslationKey.desc_exit_code)] int exitCode = 0)
        => TheGodfather.Stop(exitCode);
    #endregion

    #region sudo
    [Command("sudo")]
    [Aliases("execas", "as")]
    [RequireGuild][RequirePrivilegedUser]
    public async Task SudoAsync(CommandContext ctx,
        [Description(TranslationKey.desc_member)] DiscordMember member,
        [RemainingText][Description(TranslationKey.desc_cmd_full)] string command)
    {
        if (string.IsNullOrWhiteSpace(command))
            throw new InvalidCommandUsageException(ctx);

        Command? cmd = ctx.CommandsNext.FindCommand(command, out string args);
        if (cmd is null)
            throw new CommandFailedException(ctx, TranslationKey.cmd_404(command));
        if (cmd.ExecutionChecks.Any(c => c is RequireOwnerAttribute or RequirePrivilegedUserAttribute))
            throw new CommandFailedException(ctx, TranslationKey.cmd_err_sudo);
        CommandContext fctx = ctx.CommandsNext.CreateFakeContext(member, ctx.Channel, command, ctx.Prefix, cmd, args);
        if ((await cmd.RunChecksAsync(fctx, false)).Any())
            throw new CommandFailedException(ctx, TranslationKey.cmd_err_sudo_chk);

        await ctx.CommandsNext.ExecuteCommandAsync(fctx);
    }
    #endregion

    #region toggleignore
    [Command("toggleignore")]
    [Aliases("ti")]
    [RequirePrivilegedUser]
    public Task ToggleIgnoreAsync(CommandContext ctx)
    {
        BotActivityService bas = ctx.Services.GetRequiredService<BotActivityService>();
        bool ignoreEnabled = bas.ToggleListeningStatus();
        return ctx.InfoAsync(this.ModuleColor, ignoreEnabled ? TranslationKey.str_off : TranslationKey.str_on);
    }
    #endregion

    #region restart
    [Command("restart")]
    [Aliases("reboot")]
    [RequirePrivilegedUser]
    public Task RestartAsync(CommandContext ctx)
        => this.ExitAsync(ctx, 100);
    #endregion

    #region update
    [Command("update")]
    [RequireOwner]
    public Task UpdateAsync(CommandContext ctx)
        => this.ExitAsync(ctx, 101);
    #endregion

    #region uptime
    [Command("uptime")]
    [RequirePrivilegedUser]
    public Task UptimeAsync(CommandContext ctx)
    {
        BotActivityService bas = ctx.Services.GetRequiredService<BotActivityService>();
        TimeSpan processUptime = bas.UptimeInformation.ProgramUptime;
        TimeSpan socketUptime = bas.UptimeInformation.SocketUptime;

        return ctx.RespondWithLocalizedEmbedAsync(emb => {
            emb.WithLocalizedTitle(TranslationKey.str_uptime_info);
            emb.WithDescription($"{TheGodfather.ApplicationName} {TheGodfather.ApplicationVersion}");
            emb.AddLocalizedField(TranslationKey.str_shard, $"{ctx.Client.ShardId}/{ctx.Client.ShardCount - 1}", true);
            emb.AddLocalizedField(TranslationKey.str_uptime_bot, processUptime.ToString(@"dd\.hh\:mm\:ss"), true);
            emb.AddLocalizedField(TranslationKey.str_uptime_socket, socketUptime.ToString(@"dd\.hh\:mm\:ss"), true);
            emb.WithColor(this.ModuleColor);
        });
    }
    #endregion
}