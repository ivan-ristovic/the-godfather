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

using TheGodfather.Attributes;
using TheGodfather.Entities;
using TheGodfather.Exceptions;
using TheGodfather.Extensions;
using TheGodfather.Modules.Owner.Common;
using TheGodfather.Services;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
#endregion

namespace TheGodfather.Modules.Owner
{
    [Group("owner")]
    [Description("Owner-only bot administration commands.")]
    [Aliases("admin", "o")]
    [RequireOwner, Hidden]
    [Cooldown(3, 5, CooldownBucketType.Global)]
    public partial class BotOwnerModule : TheGodfatherBaseModule
    {

        public BotOwnerModule(DBService db) : base(db: db) { }


        #region COMMAND_BOTAVATAR
        [Command("botavatar")]
        [Description("Set bot avatar.")]
        [Aliases("setbotavatar", "setavatar")]
        [UsageExample("!owner botavatar http://someimage.png")]
        [ListeningCheck]
        public async Task SetBotAvatarAsync(CommandContext ctx,
                                           [Description("URL.")] string url)
        {
            if (string.IsNullOrWhiteSpace(url))
                throw new InvalidCommandUsageException("URL missing.");

            if (!IsValidImageURL(url, out Uri uri))
                throw new CommandFailedException("URL must point to an image and use http or https protocols.");

            try {
                var stream = await HTTPClient.GetStreamAsync(uri)
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

            await ReplyWithEmbedAsync(ctx)
                .ConfigureAwait(false);
        }
        #endregion

        #region COMMAND_BOTNAME
        [Command("botname")]
        [Description("Set bot name.")]
        [Aliases("setbotname", "setname")]
        [UsageExample("!owner setname TheBotfather")]
        [ListeningCheck]
        public async Task SetBotNameAsync(CommandContext ctx,
                                         [RemainingText, Description("New name.")] string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new InvalidCommandUsageException("Name missing.");

            await ctx.Client.UpdateCurrentUserAsync(username: name)
                .ConfigureAwait(false);
            await ReplyWithEmbedAsync(ctx)
                .ConfigureAwait(false);
        }
        #endregion

        #region COMMAND_CLEARLOG
        [Command("clearlog")]
        [Description("Clear application logs.")]
        [Aliases("clearlogs", "deletelogs", "deletelog")]
        [UsageExample("!owner clearlog")]
        [ListeningCheck]
        public async Task ClearLogAsync(CommandContext ctx)
        {
            if (!await AskYesNoQuestionAsync(ctx, "Are you sure you want to clear the logs?").ConfigureAwait(false))
                return;

            if (!Logger.Clear())
                throw new CommandFailedException("Failed to delete log file!");

            await ReplyWithEmbedAsync(ctx)
                .ConfigureAwait(false);
        }
        #endregion

        #region COMMAND_DBQUERY
        [Command("dbquery")]
        [Description("Clear application logs.")]
        [Aliases("sql", "dbq", "q")]
        [UsageExample("!owner dbquery SELECT * FROM gf.msgcount;")]
        [ListeningCheck]
        public async Task DatabaseQuery(CommandContext ctx,
                                        [RemainingText, Description("SQL Query.")] string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                throw new InvalidCommandUsageException("Query missing.");

            IReadOnlyList<IReadOnlyDictionary<string, string>> res;
            try {
                res = await Database.ExecuteRawQueryAsync(query)
                    .ConfigureAwait(false);
            } catch (Npgsql.NpgsqlException e) {
                throw new CommandFailedException("An error occured while attempting to execute the query.", e);
            }

            if (!res.Any() || !res.First().Any()) {
                await ctx.RespondAsync("No results.")
                    .ConfigureAwait(false);
                return;
            }

            var maxlen = res.First().Select(r => r.Key).OrderByDescending(r => r.Length).First().Length + 1;

            await InteractivityUtil.SendPaginatedCollectionAsync(
                ctx,
                $"Results ({res.Count}):",
                res,
                row => {
                    var sb = new StringBuilder();
                    foreach (var col in row)
                        sb.Append(col.Key).Append(new string(' ', maxlen - col.Key.Length)).Append("| ").AppendLine(col.Value);
                    return Formatter.BlockCode(sb.ToString());
                },
                DiscordColor.Azure,
                5
            ).ConfigureAwait(false);
        }
        #endregion

        #region COMMAND_EVAL
        // Original code created by Emzi, edited by me to fit own requirements
        [Command("eval")]
        [Description("Evaluates a snippet of C# code, in context. Surround the code in the code block.")]
        [Aliases("compile", "run", "e", "c", "r")]
        [UsageExample("!owner eval ```await Context.RespondAsync(\"Hello!\");```")]
        [ListeningCheck]
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
        [Command("filelog")]
        [Description("Toggle writing to log file.")]
        [Aliases("setfl", "fl", "setfilelog")]
        [UsageExample("!owner filelog yes")]
        [UsageExample("!owner filelog false")]
        [ListeningCheck]
        public async Task FileLogAsync(CommandContext ctx,
                                      [Description("True/False")] bool b = true)
        {
            Logger.LogToFile = b;

            await ReplyWithEmbedAsync(ctx, $"File logging set to {b}")
                .ConfigureAwait(false);
        }
        #endregion

        #region COMMAND_GENERATECOMMANDS
        [Command("generatecommandlist")]
        [Description("Generates a markdown command-list. You can also provide a file path for the output.")]
        [Aliases("cmdlist", "gencmdlist", "gencmds", "gencmdslist")]
        [UsageExample("!owner generatecommandlist")]
        [UsageExample("!owner generatecommandlist Temp/blabla.md")]
        [ListeningCheck]
        public async Task GenerateCommandListAsync(CommandContext ctx,
                                                  [RemainingText, Description("File path.")] string filepath = null)
        {
            if (string.IsNullOrWhiteSpace(filepath))
                filepath = "Temp/cmds.md";

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("# Command list");
            sb.AppendLine();

            List<Command> commands = new List<Command>();
            foreach (var cmd in ctx.CommandsNext.RegisteredCommands.Values.Distinct()) {
                if (cmd is CommandGroup grp)
                    AddCommandsFromGroupRecursive(grp, commands);
                else
                    commands.Add(cmd);
            }
            commands.Sort((c1, c2) => string.Compare(c1.QualifiedName, c2.QualifiedName, true));

            foreach (var cmd in commands) {
                if (cmd is CommandGroup || cmd.Parent == null)
                    sb.Append("## ").Append(cmd is CommandGroup ? "Group: " : "").AppendLine(cmd.QualifiedName);
                else
                    sb.Append("### ").AppendLine(cmd.QualifiedName);

                if (cmd.IsHidden)
                    sb.AppendLine(Formatter.Italic("Hidden.")).AppendLine();

                sb.AppendLine(Formatter.Italic(cmd.Description ?? "No description provided.")).AppendLine();

                var allchecks = cmd.ExecutionChecks.Union(cmd.Parent?.ExecutionChecks ?? Enumerable.Empty<CheckBaseAttribute>());
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
                    sb.AppendLine(Formatter.Bold("Owner-only.") + "\n");
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
                sb.AppendLine();

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

                        sb.AppendLine("\n");
                    }
                }

                var examples = cmd.CustomAttributes.Where(chk => chk is UsageExampleAttribute)
                                                   .Select(chk => chk as UsageExampleAttribute);
                if (examples.Any()) {
                    sb.AppendLine(Formatter.Bold("Examples:")).AppendLine().AppendLine("```");
                    foreach (var example in examples)
                        sb.AppendLine(example.Example);
                    sb.AppendLine("```");
                }

                sb.AppendLine("---").AppendLine();
            }

            try {
                File.WriteAllText(filepath, sb.ToString());
            } catch (IOException e) {
                throw new CommandFailedException("IO Exception occured!", e);
            }

            await ReplyWithEmbedAsync(ctx, $"Command list created at path: {Formatter.InlineCode(filepath)}!")
                .ConfigureAwait(false);
        }

        private void AddCommandsFromGroupRecursive(CommandGroup group, List<Command> commands)
        {
            if (group.IsExecutableWithoutSubcommands)
                commands.Add(group as Command);
            foreach (var child in group.Children) {
                if (child is CommandGroup grp)
                    AddCommandsFromGroupRecursive(grp, commands);
                else
                    commands.Add(child);
            }
        }
        #endregion

        #region COMMAND_LEAVEGUILDS
        [Command("leaveguilds")]
        [Description("Leaves the given guilds.")]
        [Aliases("leave", "gtfo")]
        [UsageExample("!owner leave 337570344149975050")]
        [UsageExample("!owner leave 337570344149975050 201315884709576708")]
        [ListeningCheck]
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
            await ReplyWithEmbedAsync(ctx, sb.ToString())
                .ConfigureAwait(false);
        }
        #endregion

        #region COMMAND_SENDMESSAGE
        [Command("sendmessage")]
        [Description("Sends a message to a user or channel.")]
        [Aliases("send", "s")]
        [UsageExample("!owner send u 303463460233150464 Hi to user!")]
        [UsageExample("!owner send c 120233460278590414 Hi to channel!")]
        [ListeningCheck]
        public async Task SendAsync(CommandContext ctx,
                                   [Description("u/c (for user or channel.)")] string desc,
                                   [Description("User/Channel ID.")] ulong xid,
                                   [RemainingText, Description("Message.")] string message)
        {
            if (string.IsNullOrWhiteSpace(message))
                throw new InvalidCommandUsageException();

            if (desc == "u") {
                var dm = await InteractivityUtil.CreateDmChannelAsync(ctx.Client, xid)
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

            await ReplyWithEmbedAsync(ctx)
                .ConfigureAwait(false);
        }
        #endregion

        #region COMMAND_SHUTDOWN
        [Command("shutdown")]
        [Description("Triggers the dying in the vineyard scene (power off the bot).")]
        [Aliases("disable", "poweroff", "exit", "quit")]
        [UsageExample("!owner shutdown")]
        [ListeningCheck]
        public async Task ExitAsync(CommandContext ctx)
        {
            await ctx.RespondAsync("https://www.youtube.com/watch?v=4rbfuw0UN2A")
                .ConfigureAwait(false);
            Environment.Exit(0);
        }
        #endregion

        #region COMMAND_SUDO
        [Command("sudo")]
        [Description("Executes a command as another user.")]
        [Aliases("execas", "as")]
        [UsageExample("!owner sudo @Someone !rate")]
        [ListeningCheck]
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
        [Command("toggleignore")]
        [Description("Toggle bot's reaction to commands.")]
        [Aliases("ti")]
        [UsageExample("!owner toggleignore")]
        public async Task ToggleIgnoreAsync(CommandContext ctx)
        {
            TheGodfatherShard.Listening = !TheGodfatherShard.Listening;
            await ReplyWithEmbedAsync(ctx, $"Listening status set to: {Formatter.Bold(TheGodfatherShard.Listening.ToString())}")
                .ConfigureAwait(false);
        }
        #endregion
    }
}
