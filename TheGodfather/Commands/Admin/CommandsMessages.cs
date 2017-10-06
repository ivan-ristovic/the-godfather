#region USING_DIRECTIVES
using System;
using System.Linq;
using System.Threading.Tasks;

using TheGodfatherBot.Exceptions;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
#endregion

namespace TheGodfatherBot.Commands.Admin
{
    [Group("messages", CanInvokeWithoutSubcommand = false)]
    [Description("Commands to manipulate messages on the channel.")]
    [RequirePermissions(Permissions.ManageMessages)]
    [Aliases("m", "msg", "msgs")]
    public class CommandsMessages
    {
        #region COMMAND_DELETE
        [Command("delete")]
        [Description("Deletes the specified ammount of most-recent messages from the channel.")]
        [Aliases("-", "prune", "del", "d")]
        [RequirePermissions(Permissions.Administrator)]
        public async Task Delete(CommandContext ctx, 
                                [Description("Ammount.")] int n = 1)
        {
            if (n <= 0 || n > 10000)
                throw new CommandFailedException("Invalid number of messages to delete (must be in range [1, 10000].", new ArgumentOutOfRangeException());

            await ctx.Channel.GetMessagesAsync(n).ContinueWith(
                async t => await ctx.Channel.DeleteMessagesAsync(t.Result)
            );
        }
        #endregion

        #region COMMAND_DELETE_FROM
        [Command("deleteu")]
        [Description("Deletes given ammount of most-recent messages from given user.")]
        [Aliases("-user", "deluser", "du")]
        [RequirePermissions(Permissions.Administrator)]
        public async Task DeleteUserMessages(CommandContext ctx, 
                                            [Description("User.")] DiscordUser u = null,
                                            [Description("Ammount.")] int n = 1)
        {
            if (u == null)
                throw new InvalidCommandUsageException("User missing.");
            if (n <= 0 || n > 10000)
                throw new CommandFailedException("Invalid number of messages to delete (must be in range [1, 10000].", new ArgumentOutOfRangeException());

            await ctx.Channel.DeleteMessagesAsync(
                ctx.Channel.GetMessagesAsync().Result.Where(m => m.Author.Id == u.Id).Take(n)
            );
        }
        #endregion
    }
}