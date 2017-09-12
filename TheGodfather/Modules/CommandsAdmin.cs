#region USING_DIRECTIVES
using System;
using System.IO;
using System.Threading.Tasks;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
#endregion

namespace TheGodfatherBot
{
    [Group("admin"), Description("Administrative owner commands."), Hidden]
    [RequireOwner]
    public class CommandsAdmin
    {
        #region COMMAND_CLEARLOG
        [Command("clearlog"), Description("Clear application logs.")]
        [Aliases("clearlogs", "deletelogs", "deletelog")]
        public async Task ChangeNickname(CommandContext ctx)
        {
            try {
                TheGodfather.CloseLogFile();
                File.Delete("log.txt");
                TheGodfather.OpenLogFile();
            } catch (Exception e) {
                ctx.Client.DebugLogger.LogMessage(LogLevel.Error, "TheGodfather", e.Message, DateTime.Now);
                throw e;
            }

            await ctx.RespondAsync("Logs cleared.");
        }
        #endregion
       
        #region COMMAND_SHUTDOWN
        [Command("shutdown"), Description("Triggers the dying in the vineyard scene.")]
        [Aliases("disable", "poweroff", "exit", "quit")]
        public async Task ShutDown(CommandContext ctx)
        {
            await ctx.RespondAsync("https://www.youtube.com/watch?v=4rbfuw0UN2A");
            await ctx.Client.DisconnectAsync();
            Environment.Exit(0);
        }
        #endregion

        #region COMMAND_SUDO
        [Command("sudo"), Description("Executes a command as another user."), Hidden]
        public async Task Sudo(CommandContext ctx, 
                              [Description("Member to execute as.")] DiscordMember member, 
                              [RemainingText, Description("Command text to execute.")] string command)
        {
            await ctx.Client.GetCommandsNext().SudoAsync(member, ctx.Channel, command);
        }
        #endregion

        [Group("status", CanInvokeWithoutSubcommand = false)]
        [RequireOwner]
        public class CommandsStatus
        {
            #region COMMAND_STATUS_ADD
            [Command("add")]
            [Description("Add a status to running queue.")]
            [Aliases("+")]
            public async Task AddStatus(CommandContext ctx,
                                       [RemainingText, Description("Status.")] string status)
            {
                if (string.IsNullOrWhiteSpace(status))
                    throw new ArgumentException("Invalid status");

                TheGodfather._statuses.Add(status);
                await ctx.RespondAsync("Status added!");
            }
            #endregion

            #region COMMAND_STATUS_DELETE
            [Command("delete")]
            [Description("Remove status from running queue.")]
            [Aliases("-", "remove")]
            public async Task DeleteStatus(CommandContext ctx,
                                          [RemainingText, Description("Status.")] string status)
            {
                if (string.IsNullOrWhiteSpace(status))
                    throw new ArgumentException("Invalid status");

                if (status == "!help")
                    throw new ArgumentException("Cannot delete that status!");

                TheGodfather._statuses.Remove(status);
                await ctx.RespondAsync("Status removed!");
            }
            #endregion

            #region COMMAND_STATUS_LIST
            [Command("list")]
            [Description("List all statuses.")]
            public async Task ListStatuses(CommandContext ctx)
            {
                string s = "Statuses:\n\n";
                foreach (var status in TheGodfather._statuses)
                    s += status + " ";
                await ctx.RespondAsync(s);
            }
            #endregion
        }
    }
}
