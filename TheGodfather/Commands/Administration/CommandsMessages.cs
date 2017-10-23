#region USING_DIRECTIVES
using System;
using System.Linq;
using System.Threading.Tasks;

using TheGodfather.Exceptions;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;
#endregion

namespace TheGodfather.Commands.Administration
{
    [Group("messages", CanInvokeWithoutSubcommand = false)]
    [Description("Commands to manipulate messages on the channel.")]
    [Aliases("m", "msg", "msgs")]
    [Cooldown(2, 5, CooldownBucketType.User)]
    public class CommandsMessages
    {
        #region COMMAND_MESSAGES_DELETE
        [Command("delete")]
        [Description("Deletes the specified ammount of most-recent messages from the channel.")]
        [Aliases("-", "prune", "del", "d")]
        [RequirePermissions(Permissions.ManageMessages)]
        [RequireUserPermissions(Permissions.Administrator)]
        public async Task DeleteMessagesAsync(CommandContext ctx, 
                                             [Description("Ammount.")] int n = 5)
        {
            if (n <= 0 || n > 1000)
                throw new CommandFailedException("Invalid number of messages to delete (must be in range [1, 1000].", new ArgumentOutOfRangeException());

            var msgs = await ctx.Channel.GetMessagesAsync(n)
                .ConfigureAwait(false);
            await ctx.Channel.DeleteMessagesAsync(msgs)
                .ConfigureAwait(false);
        }
        #endregion

        #region COMMAND_MESSAGES_DELETE_FROM
        [Command("deletefrom")]
        [Description("Deletes given ammount of most-recent messages from given user.")]
        [Aliases("-user", "deluser", "du", "dfu", "delfrom")]
        [RequirePermissions(Permissions.ManageMessages)]
        [RequireUserPermissions(Permissions.Administrator)]
        public async Task DeleteMessagesFromUserAsync(CommandContext ctx, 
                                                     [Description("User.")] DiscordUser u = null,
                                                     [Description("Ammount.")] int n = 5)
        {
            if (u == null)
                throw new InvalidCommandUsageException("User missing.");
            if (n <= 0 || n > 1000)
                throw new CommandFailedException("Invalid number of messages to delete (must be in range [1, 1000].", new ArgumentOutOfRangeException());

            var msgs = await ctx.Channel.GetMessagesAsync()
                .ConfigureAwait(false);
            await ctx.Channel.DeleteMessagesAsync(msgs.Where(m => m.Author.Id == u.Id).Take(n))
                .ConfigureAwait(false);
        }
        #endregion

        #region COMMAND_MESSAGES_LISTPINNED
        [Command("listpinned")]
        [Description("List latest ammount of pinned messages.")]
        [Aliases("lp", "listpins", "listpin", "pinned")]
        public async Task ListPinnedMessagesAsync(CommandContext ctx,
                                                 [Description("Ammount.")] int n = 1)
        {
            if (n < 1 || n > 20)
                throw new CommandFailedException("Invalid ammount (1-20).");

            var pinned = await ctx.Channel.GetPinnedMessagesAsync()
                .ConfigureAwait(false);

            var em = new DiscordEmbedBuilder() {
                Title = $"Pinned messages in {ctx.Channel.Name}:"
            };
            foreach (var msg in pinned.Take(n))
                em.AddField($"{msg.Author.Username} ({msg.CreationTimestamp})", msg.Content != null && msg.Content.Trim() != "" ? msg.Content : "<embed>");

            await ctx.RespondAsync(embed: em.Build())
                .ConfigureAwait(false);
        }
        #endregion
        
        #region COMMAND_MESSAGES_PIN
        [Command("pin")]
        [Description("Pins the last sent message.")]
        [Aliases("p")]
        [RequirePermissions(Permissions.ManageMessages)]
        public async Task PinMessageAsync(CommandContext ctx)
        {
            var msgs = await ctx.Channel.GetMessagesAsync(2)
                .ConfigureAwait(false);
            try {
                await msgs.Last().PinAsync()
                    .ConfigureAwait(false);
            } catch (BadRequestException e) {
                throw new CommandFailedException("That message cannot be pinned!", e);
            }
        }
        #endregion

        #region COMMAND_MESSAGES_UNPIN
        [Command("unpin")]
        [Description("Unpins the message at given index (starting from 0).")]
        [Aliases("up")]
        [RequirePermissions(Permissions.ManageMessages)]
        public async Task UnpinMessageAsync(CommandContext ctx,
                                           [Description("Index (starting from 0")] int i = 0)
        {
            var pinned = await ctx.Channel.GetPinnedMessagesAsync()
                .ConfigureAwait(false);

            if (i < 0 || i > pinned.Count)
                throw new CommandFailedException("Invalid index (must be in range [0-" + (pinned.Count - 1) + "]!");

            await pinned.ElementAt(i).UnpinAsync()
                .ConfigureAwait(false);
            await ctx.RespondAsync("Message successfully unpinned!")
                .ConfigureAwait(false);
        }
        #endregion

        #region COMMAND_MESSAGES_UNPINALL
        [Command("unpinall")]
        [Description("Unpins all pinned messages.")]
        [Aliases("upa")]
        [RequirePermissions(Permissions.ManageMessages)]
        public async Task UnpinAllMessagesAsync(CommandContext ctx)
        {
            var pinned = await ctx.Channel.GetPinnedMessagesAsync()
                .ConfigureAwait(false);
            foreach (var m in pinned)
                await m.UnpinAsync()
                    .ConfigureAwait(false);
            await ctx.RespondAsync("All messages successfully unpinned!")
                .ConfigureAwait(false);
        }
        #endregion
    }
}