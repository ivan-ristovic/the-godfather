using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Data.Common;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Humanizer;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Events;
using TheGodfather.Attributes;
using TheGodfather.Common;
using TheGodfather.Database;
using TheGodfather.Exceptions;
using TheGodfather.Extensions;
using TheGodfather.Modules.Administration.Common;
using TheGodfather.Modules.Owner.Common;
using TheGodfather.Modules.Owner.Extensions;
using TheGodfather.Modules.Owner.Services;
using TheGodfather.Services;
using TheGodfather.Services.Common;

namespace TheGodfather.Modules.Owner
{
    [Group("owner"), Module(ModuleType.Owner), Hidden]
    [Aliases("admin", "o")]
    public partial class OwnerModule : TheGodfatherModule
    {
        #region announce
        [Command("announce"), UsesInteractivity]
        [Aliases("ann")]
        [RequireOwner]
        public async Task AnnounceAsync(CommandContext ctx,
                                       [RemainingText, Description("desc-announcement")] string message)
        {
            if (!await ctx.WaitForBoolReplyAsync("q-announcement", args: Formatter.Strip(message)))
                return;

            LocalizationService lcs = ctx.Services.GetRequiredService<LocalizationService>();
            var emb = new LocalizedEmbedBuilder(lcs, ctx.Guild?.Id);
            emb.WithLocalizedTitle("str-announcement");
            emb.WithDescription(message);
            emb.WithColor(DiscordColor.Red);

            var eb = new StringBuilder();
            foreach (TheGodfatherShard shard in TheGodfather.ActiveShards) {
                foreach (DiscordGuild guild in shard.Client.Guilds.Select(kvp => kvp.Value)) {
                    try {
                        await guild.GetDefaultChannel().SendMessageAsync(embed: emb.Build());
                    } catch {
                        eb.AppendLine(lcs.GetString(ctx.Guild?.Id, "cmd-err-announce", shard.Id, guild.Name, guild.Id));
                    }
                }
            }

            if (eb.Length > 0)
                await ctx.ImpInfoAsync(this.ModuleColor, "fmt-err", eb.ToString());
            else
                await ctx.InfoAsync(this.ModuleColor);
        }
        #endregion

        #region avatar
        [Command("avatar")]
        [Aliases("setavatar", "setbotavatar", "profilepic", "a")]
        [RequireOwner]
        public async Task SetBotAvatarAsync(CommandContext ctx,
                                           [Description("desc-image-url")] Uri url)
        {
            if (!await url.ContentTypeHeaderIsImageAsync(DiscordLimits.AvatarSizeLimit))
                throw new CommandFailedException(ctx, "cmd-err-image-url-fail", DiscordLimits.AvatarSizeLimit);

            try {
                using MemoryStream ms = await HttpService.GetMemoryStreamAsync(url);
                await ctx.Client.UpdateCurrentUserAsync(avatar: ms);
            } catch (WebException e) {
                throw new CommandFailedException(ctx, e, "err-url-image-fail");
            }

            await ctx.InfoAsync(this.ModuleColor);
        }
        #endregion

        #region name
        [Command("name")]
        [Aliases("botname", "setbotname", "setname")]
        [RequireOwner]
        public async Task SetBotNameAsync(CommandContext ctx,
                                         [RemainingText, Description("desc-name")] string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new InvalidCommandUsageException(ctx, "cmd-err-missing-name");

            if (name.Length > DiscordLimits.NameLimit)
                throw new InvalidCommandUsageException(ctx, "cmd-err-name", DiscordLimits.NameLimit);

            await ctx.Client.UpdateCurrentUserAsync(username: name);
            await ctx.InfoAsync(this.ModuleColor);
        }
        #endregion

        #region dbquery
        [Command("dbquery"), Priority(1)]
        [Aliases("sql", "dbq", "q", "query")]
        [RequireOwner]
        public async Task DatabaseQuery(CommandContext ctx)
        {
            if (!ctx.Message.Attachments.Any())
                throw new CommandFailedException(ctx, "cmd-err-dbq-sql-att");

            DiscordAttachment? attachment = ctx.Message.Attachments.FirstOrDefault(att => att.FileName.EndsWith(".sql"));
            if (attachment is null)
                throw new CommandFailedException(ctx, "cmd-err-dbq-sql-att-none");

            string query;
            try {
                query = await HttpService.GetStringAsync(attachment.Url).ConfigureAwait(false);
            } catch (Exception e) {
                throw new CommandFailedException(ctx, e, "err-attachment");
            }

            await this.DatabaseQuery(ctx, query);
        }

        [Command("dbquery"), Priority(0)]
        public async Task DatabaseQuery(CommandContext ctx,
                                       [RemainingText, Description("desc-sql")] string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                throw new InvalidCommandUsageException(ctx, "cmd-err-dbq-sql-none");

            var res = new List<IReadOnlyDictionary<string, string>>();
            using (TheGodfatherDbContext db = ctx.Services.GetRequiredService<DbContextBuilder>().CreateContext())
            using (RelationalDataReader dr = await db.Database.ExecuteSqlQueryAsync(query, db)) {
                DbDataReader reader = dr.DbDataReader;
                while (await reader.ReadAsync()) {
                    var dict = new Dictionary<string, string>();

                    for (int i = 0; i < reader.FieldCount; i++)
                        dict[reader.GetName(i)] = reader[i] is DBNull ? "NULL" : reader[i]?.ToString() ?? "NULL";

                    res.Add(new ReadOnlyDictionary<string, string>(dict));
                }
            }

            if (!res.Any() || !res.First().Any()) {
                await ctx.InfoAsync(this.ModuleColor, Emojis.Information, "str-dbq-none");
                return;
            }

            int maxlen = 1 + res
                .First()
                .Select(r => r.Key)
                .OrderByDescending(r => r.Length)
                .First()
                .Length;

            await ctx.PaginateAsync(
                "str-dbq-res",
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
        [Aliases("evaluate", "compile", "run", "e", "c", "r")]
        [RequireOwner]
        public async Task EvaluateAsync(CommandContext ctx,
                                       [RemainingText, Description("desc-code")] string code)
        {
            if (string.IsNullOrWhiteSpace(code))
                throw new InvalidCommandUsageException(ctx, "cmd-err-cmd-add-cb");

            LocalizationService lcs = ctx.Services.GetRequiredService<LocalizationService>();
            DiscordMessage msg = await ctx.RespondWithLocalizedEmbedAsync(emb => {
                emb.WithLocalizedTitle("str-eval");
                emb.WithColor(this.ModuleColor);
            });

            Script<object>? snippet = CSharpCompilationService.Compile(code, out ImmutableArray<Diagnostic> diag, out Stopwatch compileTime);
            if (snippet is null) {
                await msg.DeleteAsync();
                throw new InvalidCommandUsageException(ctx, "cmd-err-cmd-add-cb");
            }

            var emb = new LocalizedEmbedBuilder(lcs, ctx.Guild?.Id);
            
            if (diag.Any(d => d.Severity == DiagnosticSeverity.Error)) {
                emb.WithLocalizedTitle("str-eval-fail-compile");
                emb.WithLocalizedDescription("fmt-eval-fail-compile", compileTime.ElapsedMilliseconds, diag.Length);
                emb.WithColor(DiscordColor.Red);

                foreach (Diagnostic d in diag.Take(3)) {
                    FileLinePositionSpan ls = d.Location.GetLineSpan();
                    emb.AddLocalizedTitleField("fmt-eval-err", Formatter.InlineCode(d.GetMessage()), titleArgs: new[] { ls.StartLinePosition.Line, ls.StartLinePosition.Character });
                }

                if (diag.Length > 3)
                    emb.AddLocalizedField("str-eval-omit", "fmt-eval-omit", contentArgs: new object[] { diag.Length - 3 });

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
                emb.WithLocalizedTitle("str-eval-fail-run");
                emb.WithLocalizedDescription("fmt-eval-fail-run", runTime.ElapsedMilliseconds, exc.GetType(), exc.Message);
                emb.WithColor(DiscordColor.Red);
                await UpdateOrRespondAsync();
            } else {
                emb.WithLocalizedTitle("str-eval-succ");
                emb.WithColor(this.ModuleColor);
                if (res.ReturnValue is { }) {
                    emb.AddLocalizedTitleField("str-result", res.ReturnValue, false);
                    emb.AddLocalizedTitleField("str-result-type", res.ReturnValue.GetType(), true);
                } else {
                    emb.AddLocalizedField("str-result", "str-eval-value-none", inline: true);
                }
                emb.AddLocalizedTitleField("str-eval-time-compile", compileTime.ElapsedMilliseconds, true);
                emb.AddLocalizedTitleField("str-eval-time-run", runTime.ElapsedMilliseconds, true);
                if (res.ReturnValue is { })
                await UpdateOrRespondAsync();
            }


            Task UpdateOrRespondAsync()
                => msg is { } ? msg.ModifyAsync(embed: emb.Build()) : ctx.RespondAsync(embed: emb.Build());
        }
        #endregion

        #region generatecommandlist
        [Command("generatecommandlist")]
        [Aliases("gendocs", "docs", "cmdlist", "gencmdlist", "gencmds", "gencmdslist")]
        [RequireOwner]
        public async Task GenerateCommandListAsync(CommandContext ctx,
                                                  [RemainingText, Description("desc-folder")] string? path = null)
        {
            if (string.IsNullOrWhiteSpace(path))
                path = "Documentation";

            DirectoryInfo current;
            DirectoryInfo parts;
            try {
                if (Directory.Exists(path))
                    Directory.Delete(path, recursive: true);
                current = Directory.CreateDirectory(path);
                parts = Directory.CreateDirectory(Path.Combine(current.FullName, "Parts"));
            } catch (IOException e) {
                LogExt.Error(ctx, e, "Failed to delete/create documentation directory");
                throw new CommandFailedException(ctx, "cmd-err-doc-clean");
            }

            LocalizationService lcs = ctx.Services.GetRequiredService<LocalizationService>();
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
                sb.AppendLine(Formatter.Italic(lcs.GetString(null, $"{mattr.Module.ToLocalizedDescriptionKey()}-raw")));
                sb.AppendLine().AppendLine();

                foreach (Command cmd in cmdlist) {
                    if (cmd is CommandGroup || cmd.Parent is null)
                        sb.Append("## ").Append(cmd is CommandGroup ? "Group: " : "").AppendLine(cmd.QualifiedName);
                    else
                        sb.Append("### ").AppendLine(cmd.QualifiedName);

                    sb.AppendLine("<details><summary markdown='span'>Expand for additional information</summary><p>").AppendLine();

                    if (cmd.IsHidden)
                        sb.AppendLine(Formatter.Italic("Hidden.")).AppendLine();

                    sb.AppendLine(Formatter.Italic(lcs.GetCommandDescription(null, cmd.QualifiedName))).AppendLine();

                    if (cmd.Aliases.Any()) {
                        sb.AppendLine(Formatter.Bold("Aliases:"));
                        sb.Append('`').AppendJoin(", ", cmd.Aliases).Append('`').AppendLine();
                    }

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

                    if (execChecks.Any(chk => chk is RequireOwnerAttribute))
                        sb.AppendLine(Formatter.Bold("Owner-only.")).AppendLine();
                    if (execChecks.Any(chk => chk is RequirePrivilegedUserAttribute))
                        sb.AppendLine(Formatter.Bold("Privileged users only.")).AppendLine().AppendLine();

                    if (perms.Any()) {
                        sb.AppendLine(Formatter.Bold("Requires permissions:"));
                        sb.Append('`').AppendJoin(", ", perms).Append('`').AppendLine().AppendLine();
                    }
                    if (uperms.Any()) {
                        sb.AppendLine(Formatter.Bold("Requires user permissions:"));
                        sb.Append('`').AppendJoin(", ", uperms).Append('`').AppendLine().AppendLine();
                    }
                    if (bperms.Any()) {
                        sb.AppendLine(Formatter.Bold("Requires bot permissions:"));
                        sb.Append('`').AppendJoin(", ", bperms).Append('`').AppendLine().AppendLine();
                    }

                    foreach (CommandOverload overload in cmd.Overloads.OrderByDescending(o => o.Priority)) {
                        if (!overload.Arguments.Any())
                            continue;

                        sb.AppendLine(Formatter.Bold(cmd.Overloads.Count > 1 ? $"Overload {overload.Priority}:" : "Arguments:")).AppendLine();
                        foreach (CommandArgument arg in overload.Arguments) {
                            if (arg.IsOptional)
                                sb.Append("(optional) ");

                            string type = $"[{ctx.CommandsNext.GetUserFriendlyTypeName(arg.Type)}";
                            if (arg.IsCatchAll)
                                type += "...";
                            type += "]";

                            sb.Append(Formatter.InlineCode(type));
                            sb.Append(" : ");

                            sb.Append('*');
                            if (string.IsNullOrWhiteSpace(arg.Description))
                                sb.Append("No description provided.");
                            else
                                sb.Append(lcs.GetString(null, arg.Description));
                            sb.Append('*');

                            if (arg.IsOptional) {
                                sb.Append(" (def: `");
                                if (arg.DefaultValue is null)
                                    sb.Append("None`)");
                                else
                                    sb.Append(arg.DefaultValue).Append("`)");
                            }

                            sb.AppendLine().AppendLine();
                        }
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
                    throw new CommandFailedException(ctx, "cmd-err-doc-save", filename);
                }

                sb.Clear();
            }

            sb.AppendLine("# Command modules:");
            foreach ((ModuleAttribute mattr, List<Command> cmdlist) in modules) {
                string mname = mattr.Module.ToString();
                sb.Append("  - ").Append('[').Append(mname).Append(']').Append('(').Append(parts.Name).Append('/').Append(mname).Append(".md").AppendLine(")");
            }

            string mainDocFilename = Path.Combine(current.FullName, $"README.md");
            try {
                File.WriteAllText(mainDocFilename, sb.ToString());
            } catch (IOException e) {
                LogExt.Error(ctx, e, "Failed to delete/create documentation file {Filename}", mainDocFilename);
                throw new CommandFailedException(ctx, "cmd-err-doc-save", mainDocFilename);
            }

            await ctx.InfoAsync(this.ModuleColor);
        }
        #endregion

        #region COMMAND_LEAVEGUILDS
        [Command("leaveguilds")]
        [Description("Leaves the given guilds.")]
        [Aliases("leave", "gtfo")]

        [RequireOwner]
        public async Task LeaveGuildsAsync(CommandContext ctx,
                                          [Description("Guild ID list.")] params ulong[] gids)
        {
            if (gids is null || !gids.Any())
                throw new InvalidCommandUsageException("IDs missing.");

            var eb = new StringBuilder();
            foreach (ulong gid in gids) {
                try {
                    if (ctx.Client.Guilds.TryGetValue(gid, out DiscordGuild guild))
                        await guild.LeaveAsync();
                    else
                        eb.AppendLine($"Warning: I am not a member of the guild with ID: {Formatter.InlineCode(gid.ToString())}!");
                } catch {
                    eb.AppendLine($"Error: Failed to leave guild with ID: {Formatter.InlineCode(gid.ToString())}!");
                }
            }

            if (gids.All(gid => gid != ctx.Guild?.Id)) {
                if (eb.Length > 0)
                    await this.InformFailureAsync(ctx, $"Action finished with following errors:\n\n{eb.ToString()}");
                else
                    await this.InformAsync(ctx, Emojis.Information, "Successfully left all given guilds!", important: false);
            }
        }
        #endregion

        #region COMMAND_LOG
        [Command("log"), Priority(1)]
        [Description("Upload the bot log file or add a remark to it.")]
        [Aliases("getlog", "remark", "rem")]

        [RequireOwner]
        public async Task LogAsync(CommandContext ctx,
                                  [Description("Bypass current configuration and search file anyway?")] bool bypassConfig = false)
        {
            // TODO rework needed since Serilog introduction
            await ctx.RespondAsync("This command is broken dude, remember?");

            BotConfig cfg = ctx.Services.GetService<BotConfigService>().CurrentConfiguration;

            if (!bypassConfig && !cfg.LogToFile)
                throw new CommandFailedException("Logs aren't dumped to any files.");
            var fi = new FileInfo(cfg.LogPath);
            if (fi.Exists && fi.Length > 8 * 1024 * 1024)
                throw new CommandFailedException("The file is too big to upload!");
            using var fs = new FileStream(cfg.LogPath, FileMode.Open);
            await ctx.RespondWithFileAsync(fs);
        }

        [Command("log"), Priority(0)]
        public Task LogAsync(CommandContext ctx,
                            [Description("Log level.")] string level,
                            [RemainingText, Description("Remark.")] string text)
        {
            if (!Enum.TryParse(level.Titleize(), out LogEventLevel logLevel))
                throw new CommandFailedException($"Invalid log level!");
            Log.Write(logLevel, "{LogRemark}", text);
            return this.InformAsync(ctx, "Done!", important: false);
        }
        #endregion

        #region COMMAND_SENDMESSAGE
        [Command("sendmessage")]
        [Description("Sends a message to a user or channel.")]
        [Aliases("send", "s")]

        [RequirePrivilegedUser]
        public async Task SendAsync(CommandContext ctx,
                                   [Description("u/c (for user or channel.)")] string desc,
                                   [Description("User/Channel ID.")] ulong xid,
                                   [RemainingText, Description("Message.")] string message)
        {
            if (string.IsNullOrWhiteSpace(message))
                throw new InvalidCommandUsageException();

            if (desc == "u") {
                DiscordDmChannel dm = await ctx.Client.CreateDmChannelAsync(xid);
                if (dm is null)
                    throw new CommandFailedException("I can't talk to that user...");
                await dm.SendMessageAsync(message);
            } else if (desc == "c") {
                DiscordChannel channel = await ctx.Client.GetChannelAsync(xid);
                await channel.SendMessageAsync(message);
            } else {
                throw new InvalidCommandUsageException("Descriptor can only be 'u' or 'c'.");
            }

            await this.InformAsync(ctx, $"Successfully sent the given message!", important: false);
        }
        #endregion

        #region COMMAND_SHUTDOWN
        [Command("shutdown"), Priority(1)]
        [Description("Triggers the dying in the vineyard scene (power off the bot).")]
        [Aliases("disable", "poweroff", "exit", "quit")]

        [RequirePrivilegedUser]
        public Task ExitAsync(CommandContext _,
                             [Description("Time until shutdown.")] TimeSpan timespan,
                             [Description("Exit code.")] int exitCode = 0)
            => TheGodfather.Stop(exitCode, timespan);

        [Command("shutdown"), Priority(0)]
        public Task ExitAsync(CommandContext _,
                             [Description("Exit code.")] int exitCode = 0)
            => TheGodfather.Stop(exitCode);
        #endregion

        #region COMMAND_SUDO
        [Command("sudo")]
        [Description("Executes a command as another user.")]
        [Aliases("execas", "as")]

        [RequirePrivilegedUser]
        public Task SudoAsync(CommandContext ctx,
                             [Description("Member to execute as.")] DiscordMember member,
                             [RemainingText, Description("Command text to execute.")] string command)
        {
            if (string.IsNullOrWhiteSpace(command))
                throw new InvalidCommandUsageException("Missing command to execute.");

            Command cmd = ctx.CommandsNext.FindCommand(command, out string args);
            if (cmd.ExecutionChecks.Any(c => c is RequireOwnerAttribute || c is RequirePrivilegedUserAttribute))
                throw new CommandFailedException("Cannot sudo privileged commands!");
            CommandContext fctx = ctx.CommandsNext.CreateFakeContext(member, ctx.Channel, command, ctx.Prefix, cmd, args);
            return cmd is null ? Task.CompletedTask : ctx.CommandsNext.ExecuteCommandAsync(fctx);
        }
        #endregion

        #region COMMAND_TOGGLEIGNORE
        [Command("toggleignore")]
        [Description("Toggle bot's reaction to commands.")]
        [Aliases("ti")]
        [RequirePrivilegedUser]
        public Task ToggleIgnoreAsync(CommandContext ctx)
        {
            BotActivityService bas = ctx.Services.GetService<BotActivityService>();
            bool ignoreEnabled = bas.ToggleListeningStatus();
            return this.InformAsync(ctx, $"Listening status set to: {Formatter.Bold(ignoreEnabled.ToString())}", important: false);
        }
        #endregion

        #region COMMAND_UPDATE
        [Command("update")]
        [Description("Update and restart the bot.")]
        [Aliases("upd", "u")]
        [RequireOwner]
        public Task UpdateAsync(CommandContext ctx)
        {
            ProcessStartInfo psi;
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) {
                psi = new ProcessStartInfo {
                    FileName = "bash",
                    Arguments = $"install.sh {Process.GetCurrentProcess().Id}",
                    UseShellExecute = false,
                    RedirectStandardOutput = false,
                    CreateNoWindow = true
                };
            } else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) {
                psi = new ProcessStartInfo {
                    FileName = "install.bat",
                    UseShellExecute = false,
                    RedirectStandardOutput = false,
                    CreateNoWindow = true
                };
            } else {
                throw new CommandFailedException("Cannot determine host OS (OSX is not supported)!");
            }

            var proc = new Process { StartInfo = psi };
            proc.Start();
            return this.ExitAsync(ctx);
        }
        #endregion

        #region UPTIME
        [Command("uptime")]
        [RequirePrivilegedUser]
        public Task UptimeAsync(CommandContext ctx)
        {
            BotActivityService bas = ctx.Services.GetRequiredService<BotActivityService>();
            UptimeInformation uptimeInfo = bas.ShardUptimeInformation[ctx.Client.ShardId];
            TimeSpan processUptime = uptimeInfo.ProgramUptime;
            TimeSpan socketUptime = uptimeInfo.SocketUptime;

            return this.InformAsync(ctx, Emojis.Information,
                Formatter.Bold($"Uptime information:") +
                $"\n\n{Formatter.Bold("Shard:")} {ctx.Client.ShardId}\n" +
                $"{Formatter.Bold("Bot uptime:")} {processUptime.Days} days, {processUptime.ToString(@"hh\:mm\:ss")}\n" +
                $"{Formatter.Bold("Socket uptime:")} {socketUptime.Days} days, {socketUptime.ToString(@"hh\:mm\:ss")}"
            );
        }
        #endregion
    }
}
