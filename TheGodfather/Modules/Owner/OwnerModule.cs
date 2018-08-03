#region USING_DIRECTIVES
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;

using TheGodfather.Common.Attributes;
using TheGodfather.Exceptions;
using TheGodfather.Extensions;
using TheGodfather.Modules.Owner.Common;
using TheGodfather.Services;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using TheGodfather.Services.Database;
#endregion

namespace TheGodfather.Modules.Owner
{
    [Group("owner"), Module(ModuleType.Owner)]
    [Description("Owner-only bot administration commands.")]
    [Aliases("admin", "o")]
    [Hidden]
    [Cooldown(3, 5, CooldownBucketType.Global)]
    public partial class OwnerModule : TheGodfatherModule
    {

        public OwnerModule(SharedData shared, DBService db) : base(shared, db) { }


        #region COMMAND_ANNOUNCE
        [Command("announce"), Module(ModuleType.Owner)]
        [Description("Send a message to all guilds the bot is in.")]
        [Aliases("a", "ann")]
        [UsageExamples("!owner announce SPAM SPAM")]
        [RequireOwner]
        [NotBlocked, UsesInteractivity]
        public async Task ClearLogAsync(CommandContext ctx,
                                       [RemainingText, Description("Message to send.")] string message)
        {
            if (!await ctx.WaitForBoolReplyAsync($"Are you sure you want to announce the messsage:\n\n{message}").ConfigureAwait(false))
                return;

            var errors = new StringBuilder();
            foreach (var shard in TheGodfather.ActiveShards) {
                foreach (var guild in shard.Client.Guilds.Values) {
                    try {
                        await guild.GetDefaultChannel().SendMessageAsync()
                            .ConfigureAwait(false);
                    } catch {
                        errors.AppendLine($"Warning: Failed to send a message to {guild.ToString()}");
                    }
                }
            }

            await InformAsync(ctx, $"Message sent!\n\n{errors.ToString()}")
                .ConfigureAwait(false);
        }
        #endregion

        #region COMMAND_BOTAVATAR
        [Command("botavatar"), Module(ModuleType.Owner)]
        [Description("Set bot avatar.")]
        [Aliases("setbotavatar", "setavatar")]
        [UsageExamples("!owner botavatar http://someimage.png")]
        [RequireOwner]
        [NotBlocked]
        public async Task SetBotAvatarAsync(CommandContext ctx,
                                           [Description("URL.")] Uri url)
        {
            if (!await IsValidImageUriAsync(url).ConfigureAwait(false))
                throw new CommandFailedException("URL must point to an image and use http or https protocols.");

            try {
                var stream = await _http.GetStreamAsync(url)
                   .ConfigureAwait(false);
                using (var ms = new MemoryStream()) {
                    await stream.CopyToAsync(ms)
                        .ConfigureAwait(false);
                    await ctx.Client.UpdateCurrentUserAsync(avatar: ms)
                        .ConfigureAwait(false);
                }
            } catch (WebException e) {
                throw new CommandFailedException("Web exception thrown while fetching the image.", e);
            } catch (Exception e) {
                throw new CommandFailedException("An error occured.", e);
            }

            await InformAsync(ctx)
                .ConfigureAwait(false);
        }
        #endregion

        #region COMMAND_BOTNAME
        [Command("botname"), Module(ModuleType.Owner)]
        [Description("Set bot name.")]
        [Aliases("setbotname", "setname")]
        [UsageExamples("!owner setname TheBotfather")]
        [RequireOwner]
        [NotBlocked]
        public async Task SetBotNameAsync(CommandContext ctx,
                                         [RemainingText, Description("New name.")] string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new InvalidCommandUsageException("Name missing.");

            await ctx.Client.UpdateCurrentUserAsync(username: name)
                .ConfigureAwait(false);
            await InformAsync(ctx)
                .ConfigureAwait(false);
        }
        #endregion

        #region COMMAND_CLEARLOG
        [Command("clearlog"), Module(ModuleType.Owner)]
        [Description("Clear application logs.")]
        [Aliases("clearlogs", "deletelogs", "deletelog")]
        [UsageExamples("!owner clearlog")]
        [RequireOwner]
        [NotBlocked, UsesInteractivity]
        public async Task ClearLogAsync(CommandContext ctx)
        {
            if (!await ctx.WaitForBoolReplyAsync("Are you sure you want to clear the logs?").ConfigureAwait(false))
                return;

            if (!Shared.LogProvider.Clear())
                throw new CommandFailedException("Failed to delete log file!");

            await InformAsync(ctx)
                .ConfigureAwait(false);
        }
        #endregion

        #region COMMAND_DBQUERY
        [Command("dbquery"), Module(ModuleType.Owner)]
        [Description("Execute SQL query on the bot database.")]
        [Aliases("sql", "dbq", "q")]
        [UsageExamples("!owner dbquery SELECT * FROM gf.msgcount;")]
        [RequireOwner]
        [NotBlocked]
        public async Task DatabaseQuery(CommandContext ctx,
                                       [RemainingText, Description("SQL Query.")] string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                throw new InvalidCommandUsageException("Query missing.");

            var res = await Database.ExecuteRawQueryAsync(query)
                .ConfigureAwait(false);

            if (!res.Any() || !res.First().Any()) {
                await ctx.RespondAsync("No results.")
                    .ConfigureAwait(false);
                return;
            }

            var maxlen = res.First().Select(r => r.Key).OrderByDescending(r => r.Length).First().Length + 1;

            await ctx.SendCollectionInPagesAsync(
                $"Results:",
                res.Take(25),
                row => {
                    var sb = new StringBuilder();
                    foreach (var col in row)
                        sb.Append(col.Key).Append(new string(' ', maxlen - col.Key.Length)).Append("| ").AppendLine(col.Value);
                    return Formatter.BlockCode(sb.ToString());
                },
                DiscordColor.Azure,
                1
            ).ConfigureAwait(false);
        }
        #endregion

        #region COMMAND_EVAL
        // Original code created by Emzi, edited by me to fit own requirements
        [Command("eval"), Module(ModuleType.Owner)]
        [Description("Evaluates a snippet of C# code, in context. Surround the code in the code block.")]
        [Aliases("compile", "run", "e", "c", "r")]
        [UsageExamples("!owner eval ```await Context.RespondAsync(\"Hello!\");```")]
        [RequireOwner]
        [NotBlocked]
        public async Task EvaluateAsync(CommandContext ctx,
                                       [RemainingText, Description("Code to evaluate.")] string code)
        {
            if (string.IsNullOrWhiteSpace(code))
                throw new InvalidCommandUsageException("Code missing.");

            var cs1 = code.IndexOf("```") + 3;
            var cs2 = code.LastIndexOf("```");
            if (cs1 == -1 || cs2 == -1)
                throw new InvalidCommandUsageException("You need to wrap the code into a code block.");
            code = code.Substring(cs1, cs2 - cs1);

            var emb = new DiscordEmbedBuilder {
                Title = "Evaluating...",
                Color = DiscordColor.Aquamarine
            };
            var msg = await ctx.RespondAsync(embed: emb.Build())
                .ConfigureAwait(false);

            var globals = new EvaluationEnvironment(ctx);
            var sopts = ScriptOptions.Default
                .WithImports("System", "System.Collections.Generic", "System.Linq", "System.Net.Http", "System.Net.Http.Headers", "System.Reflection", "System.Text", "System.Text.RegularExpressions", "System.Threading.Tasks",
                    "DSharpPlus", "DSharpPlus.CommandsNext", "DSharpPlus.Entities", "DSharpPlus.Interactivity")
                .WithReferences(AppDomain.CurrentDomain.GetAssemblies().Where(xa => !xa.IsDynamic && !string.IsNullOrWhiteSpace(xa.Location)));

            var sw1 = Stopwatch.StartNew();
            var cs = CSharpScript.Create(code, sopts, typeof(EvaluationEnvironment));
            var csc = cs.Compile();
            sw1.Stop();

            if (csc.Any(xd => xd.Severity == DiagnosticSeverity.Error)) {
                emb = new DiscordEmbedBuilder {
                    Title = "Compilation failed",
                    Description = string.Concat("Compilation failed after ", sw1.ElapsedMilliseconds.ToString("#,##0"), "ms with ", csc.Length.ToString("#,##0"), " errors."),
                    Color = DiscordColor.Aquamarine
                };
                foreach (var xd in csc.Take(3)) {
                    var ls = xd.Location.GetLineSpan();
                    emb.AddField(string.Concat("Error at ", ls.StartLinePosition.Line.ToString("#,##0"), ", ", ls.StartLinePosition.Character.ToString("#,##0")), Formatter.InlineCode(xd.GetMessage()), false);
                }
                if (csc.Length > 3) {
                    emb.AddField("Some errors ommited", string.Concat((csc.Length - 3).ToString("#,##0"), " more errors not displayed"), false);
                }
                await msg.ModifyAsync(embed: emb.Build())
                    .ConfigureAwait(false);
                return;
            }

            Exception rex = null;
            ScriptState<object> css = null;
            var sw2 = Stopwatch.StartNew();
            try {
                css = await cs.RunAsync(globals)
                    .ConfigureAwait(false);
                rex = css.Exception;
            } catch (Exception ex) {
                rex = ex;
            }
            sw2.Stop();

            if (rex != null) {
                emb = new DiscordEmbedBuilder {
                    Title = "Execution failed",
                    Description = string.Concat("Execution failed after ", sw2.ElapsedMilliseconds.ToString("#,##0"), "ms with `", rex.GetType(), ": ", rex.Message, "`."),
                    Color = DiscordColor.Aquamarine
                };
                await msg.ModifyAsync(embed: emb.Build())
                    .ConfigureAwait(false);
                return;
            }

            emb = new DiscordEmbedBuilder {
                Title = "Evaluation successful",
                Color = DiscordColor.Aquamarine
            };

            emb.AddField("Result", css.ReturnValue != null ? css.ReturnValue.ToString() : "No value returned", false)
               .AddField("Compilation time", string.Concat(sw1.ElapsedMilliseconds.ToString("#,##0"), "ms"), true)
               .AddField("Execution time", string.Concat(sw2.ElapsedMilliseconds.ToString("#,##0"), "ms"), true);

            if (css.ReturnValue != null)
                emb.AddField("Return type", css.ReturnValue.GetType().ToString(), true);

            await msg.ModifyAsync(embed: emb.Build()).ConfigureAwait(false);
        }
        #endregion

        #region COMMAND_FILELOG
        [Command("filelog"), Module(ModuleType.Owner)]
        [Description("Toggle writing to log file.")]
        [Aliases("setfl", "fl", "setfilelog")]
        [UsageExamples("!owner filelog yes",
                       "!owner filelog false")]
        [RequireOwner]
        [NotBlocked]
        public async Task FileLogAsync(CommandContext ctx,
                                      [Description("True/False")] bool b = true)
        {
            Shared.LogProvider.LogToFile = b;

            await InformAsync(ctx, $"File logging set to {b}")
                .ConfigureAwait(false);
        }
        #endregion

        #region COMMAND_GENERATECOMMANDS
        [Command("generatecommandlist"), Module(ModuleType.Owner)]
        [Description("Generates a markdown command-list. You can also provide a folder for the output.")]
        [Aliases("cmdlist", "gencmdlist", "gencmds", "gencmdslist")]
        [UsageExamples("!owner generatecommandlist",
                       "!owner generatecommandlist Temp/blabla.md")]
        [RequireOwner]
        [NotBlocked]
        public async Task GenerateCommandListAsync(CommandContext ctx,
                                                  [RemainingText, Description("File path.")] string folder = null)
        {
            if (string.IsNullOrWhiteSpace(folder))
                folder = "Documentation";

            DirectoryInfo current;
            DirectoryInfo parts;
            try {
                if (Directory.Exists(folder))
                    Directory.Delete(folder, recursive: true);
                current = Directory.CreateDirectory(folder);
                parts = Directory.CreateDirectory(Path.Combine(current.FullName, "Parts"));
            } catch (Exception e) {
                Shared.LogProvider.LogException(LogLevel.Warning, e);
                throw new CommandFailedException("Failed to create directories!", e);
            }

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("# Command list");
            sb.AppendLine();

            var commands = ctx.CommandsNext.GetAllRegisteredCommands();
            var modules = commands.GroupBy(c => ModuleAttribute.ForCommand(c))
                                  .OrderBy(g => g.Key.Module)
                                  .ToDictionary(g => g.Key, g => g.OrderBy(c => c.QualifiedName).ToList());

            foreach (var module in modules) {
                sb.Append("# Module: ").Append(module.Key.Module.ToString()).AppendLine().AppendLine();
                
                foreach (var cmd in module.Value) {
                    if (cmd is CommandGroup || cmd.Parent == null)
                        sb.Append("## ").Append(cmd is CommandGroup ? "Group: " : "").AppendLine(cmd.QualifiedName);
                    else
                        sb.Append("### ").AppendLine(cmd.QualifiedName);

                    sb.AppendLine("<details><summary markdown='span'>Expand for additional information</summary><p>").AppendLine();

                    if (cmd.IsHidden)
                        sb.AppendLine(Formatter.Italic("Hidden.")).AppendLine();

                    sb.AppendLine(Formatter.Italic(cmd.Description ?? "No description provided.")).AppendLine();

                    var allchecks = cmd.ExecutionChecks.AsEnumerable();
                    var parent = cmd.Parent;
                    while (parent != null) {
                        allchecks = allchecks.Union(parent.ExecutionChecks);
                        parent = parent.Parent;
                    }
                    var permissions = allchecks.Where(chk => chk is RequirePermissionsAttribute)
                                               .Select(chk => chk as RequirePermissionsAttribute)
                                               .Select(chk => chk.Permissions.ToPermissionString());
                    var userpermissions = allchecks.Where(chk => chk is RequireUserPermissionsAttribute)
                                                   .Select(chk => chk as RequireUserPermissionsAttribute)
                                                   .Select(chk => chk.Permissions.ToPermissionString());
                    var botpermissions = allchecks.Where(chk => chk is RequireBotPermissionsAttribute)
                                                  .Select(chk => chk as RequireBotPermissionsAttribute)
                                                  .Select(chk => chk.Permissions.ToPermissionString());
                    if (allchecks.Any(chk => chk is RequireOwnerAttribute))
                        sb.AppendLine(Formatter.Bold("Owner-only.")).AppendLine();
                    if (permissions.Any()) {
                        sb.AppendLine(Formatter.Bold("Requires permissions:"));
                        sb.AppendLine(Formatter.InlineCode(string.Join(", ", permissions))).AppendLine();
                    }
                    if (userpermissions.Any()) {
                        sb.AppendLine(Formatter.Bold("Requires user permissions:"));
                        sb.AppendLine(Formatter.InlineCode(string.Join(", ", userpermissions))).AppendLine();
                    }
                    if (botpermissions.Any()) {
                        sb.AppendLine(Formatter.Bold("Requires bot permissions:"));
                        sb.AppendLine(Formatter.InlineCode(string.Join(", ", botpermissions))).AppendLine();
                    }

                    if (cmd.Aliases.Any()) {
                        sb.AppendLine(Formatter.Bold("Aliases:"));
                        sb.AppendLine(Formatter.InlineCode(string.Join(", ", cmd.Aliases))).AppendLine();
                    }

                    foreach (var overload in cmd.Overloads.OrderByDescending(o => o.Priority)) {
                        if (!overload.Arguments.Any())
                            continue;

                        sb.AppendLine(Formatter.Bold(cmd.Overloads.Count > 1 ? $"Overload {overload.Priority.ToString()}:" : "Arguments:")).AppendLine();
                        foreach (var arg in overload.Arguments) {
                            if (arg.IsOptional)
                                sb.Append("(optional) ");

                            string typestr = $"[{ctx.CommandsNext.GetUserFriendlyTypeName(arg.Type)}";
                            if (arg.IsCatchAll)
                                typestr += "...";
                            typestr += "]";

                            sb.Append(Formatter.InlineCode(typestr));
                            sb.Append(" : ");

                            sb.Append(string.IsNullOrWhiteSpace(arg.Description) ? "No description provided." : Formatter.Italic(arg.Description));

                            if (arg.IsOptional)
                                sb.Append(" (def: ").Append(Formatter.InlineCode(arg.DefaultValue != null ? arg.DefaultValue.ToString() : "None")).Append(")");

                            sb.AppendLine().AppendLine();
                        }
                    }

                    if (cmd.CustomAttributes.FirstOrDefault(chk => chk is UsageExamplesAttribute) is UsageExamplesAttribute examples) {
                        sb.AppendLine(Formatter.Bold("Examples:")).AppendLine().AppendLine("```");
                        sb.AppendLine(examples.JoinExamples());
                        sb.AppendLine("```");
                    }

                    sb.AppendLine("</p></details>").AppendLine().AppendLine("---").AppendLine();
                }

                string filename = Path.Combine(parts.FullName, $"{module.Key.Module.ToString()}.md");
                try {
                    File.WriteAllText(filename, sb.ToString());
                } catch (IOException e) {
                    throw new CommandFailedException($"IO Exception occured while saving {filename}!", e);
                }

                sb.Clear();
            }

            sb.AppendLine("# Command modules:");
            foreach (var module in modules) {
                string mname = module.Key.Module.ToString();
                sb.Append("* ").Append('[').Append(mname).Append(']').Append("(").Append(parts.Name).Append('/').Append(mname).Append(".md").AppendLine(")");
            }
            
            try {
                File.WriteAllText(Path.Combine(current.FullName, $"README.md"), sb.ToString());
            } catch (IOException e) {
                throw new CommandFailedException($"IO Exception occured while saving the main file!", e);
            }

            await InformAsync(ctx, $"Command list created at path: {Formatter.InlineCode(current.FullName)}!")
                .ConfigureAwait(false);
        }
        #endregion

        #region COMMAND_LEAVEGUILDS
        [Command("leaveguilds"), Module(ModuleType.Owner)]
        [Description("Leaves the given guilds.")]
        [Aliases("leave", "gtfo")]
        [UsageExamples("!owner leave 337570344149975050",
                       "!owner leave 337570344149975050 201315884709576708")]
        [RequireOwner]
        [NotBlocked]
        public async Task LeaveGuildsAsync(CommandContext ctx,
                                          [Description("Guild ID list.")] params ulong[] gids)
        {
            if (!gids.Any())
                throw new InvalidCommandUsageException("IDs missing.");

            var sb = new StringBuilder("Operation results:\n\n");
            foreach (var gid in gids) {
                try {
                    if (ctx.Client.Guilds.ContainsKey(gid)) {
                        var guild = ctx.Client.Guilds[gid];
                        await guild.LeaveAsync()
                            .ConfigureAwait(false);
                        sb.AppendLine($"Left: {Formatter.Bold(guild.ToString())}, Owner: {Formatter.Bold(guild.Owner.ToString())}");
                    } else {
                        sb.AppendLine($"I am not a member of the guild with ID: {Formatter.InlineCode(gid.ToString())}!");
                    }
                } catch {
                    sb.AppendLine($"Failed to leave guild with ID: {Formatter.InlineCode(gid.ToString())}!");
                }
            }
            await InformAsync(ctx, sb.ToString())
                .ConfigureAwait(false);
        }
        #endregion

        #region COMMAND_SENDMESSAGE
        [Command("sendmessage"), Module(ModuleType.Owner)]
        [Description("Sends a message to a user or channel.")]
        [Aliases("send", "s")]
        [UsageExamples("!owner send u 303463460233150464 Hi to user!",
                       "!owner send c 120233460278590414 Hi to channel!")]
        [RequirePrivilegedUser]
        [NotBlocked]
        public async Task SendAsync(CommandContext ctx,
                                   [Description("u/c (for user or channel.)")] string desc,
                                   [Description("User/Channel ID.")] ulong xid,
                                   [RemainingText, Description("Message.")] string message)
        {
            if (string.IsNullOrWhiteSpace(message))
                throw new InvalidCommandUsageException();

            if (desc == "u") {
                var dm = await ctx.Client.CreateDmChannelAsync(xid)
                    .ConfigureAwait(false);
                if (dm == null)
                    throw new CommandFailedException("I can't talk to that user...");
                await ctx.Client.SendMessageAsync(dm, content: message)
                    .ConfigureAwait(false);
            } else if (desc == "c") {
                var channel = await ctx.Client.GetChannelAsync(xid)
                    .ConfigureAwait(false);
                await ctx.Client.SendMessageAsync(channel, content: message)
                    .ConfigureAwait(false);
            } else {
                throw new InvalidCommandUsageException("Descriptor can only be 'u' or 'c'.");
            }

            await InformAsync(ctx)
                .ConfigureAwait(false);
        }
        #endregion

        #region COMMAND_SHUTDOWN
        [Command("shutdown"), Priority(1)]
        [Module(ModuleType.Owner)]
        [Description("Triggers the dying in the vineyard scene (power off the bot).")]
        [Aliases("disable", "poweroff", "exit", "quit")]
        [UsageExamples("!owner shutdown")]
        [RequirePrivilegedUser]
        [NotBlocked]
        public async Task ExitAsync(CommandContext ctx,
                                   [Description("Time until shutdown.")] TimeSpan timespan)
        {
            await Task.Delay(0).ConfigureAwait(false);
            Shared.CTS.CancelAfter(timespan);
        }

        [Command("shutdown"), Priority(0)]
        public async Task ExitAsync(CommandContext ctx)
        {
            await Task.Delay(0).ConfigureAwait(false);
            Shared.CTS.Cancel();
        }
        #endregion

        #region COMMAND_SUDO
        [Command("sudo"), Module(ModuleType.Owner)]
        [Description("Executes a command as another user.")]
        [Aliases("execas", "as")]
        [UsageExamples("!owner sudo @Someone !rate")]
        [RequirePrivilegedUser]
        [NotBlocked]
        public async Task SudoAsync(CommandContext ctx,
                                   [Description("Member to execute as.")] DiscordMember member,
                                   [RemainingText, Description("Command text to execute.")] string command)
        {
            if (string.IsNullOrWhiteSpace(command))
                throw new InvalidCommandUsageException("Missing command.");

            await ctx.Client.GetCommandsNext().SudoAsync(member, ctx.Channel, command)
                .ConfigureAwait(false);
        }
        #endregion

        #region COMMAND_TOGGLEIGNORE
        [Command("toggleignore"), Module(ModuleType.Owner)]
        [Description("Toggle bot's reaction to commands.")]
        [Aliases("ti")]
        [UsageExamples("!owner toggleignore")]
        [RequirePrivilegedUser]
        public async Task ToggleIgnoreAsync(CommandContext ctx)
        {
            Shared.ListeningStatus = !Shared.ListeningStatus;
            await InformAsync(ctx, $"Listening status set to: {Formatter.Bold(Shared.ListeningStatus.ToString())}")
                .ConfigureAwait(false);
        }
        #endregion
    }
}
