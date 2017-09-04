#region USING_DIRECTIVES
using System;
using System.Threading.Tasks;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
#endregion

namespace TheGodfatherBot
{
    [Group("admin"), Description("Administrative commands."), Hidden]
    [RequireOwner, RequirePermissions(Permissions.Administrator)]
    public class CommandsAdmin
    {
        #region COMMAND_NICK
        [Command("nick"), Description("Gives someone a new nickname.")]
        [RequirePermissions(Permissions.ManageNicknames)]
        public async Task ChangeNickname(CommandContext ctx, [Description("Member to change the nickname for.")] DiscordMember member, [RemainingText, Description("The nickname to give to that user.")] string newname)
        {
            await ctx.TriggerTypingAsync();

            try {
                await member.ModifyAsync(newname, reason: $"Changed by {ctx.User.Username} ({ctx.User.Id}).");
                var emoji = DiscordEmoji.FromName(ctx.Client, ":+1:");
                await ctx.RespondAsync(emoji.ToString());
            } catch (Exception) {
                var emoji = DiscordEmoji.FromName(ctx.Client, ":-1:");
                await ctx.RespondAsync(emoji.ToString());
            }
        }
        #endregion

        #region COMMAND_SHUTDOWN
        [Command("shutdown"), Description("Triggers the dying in the vineyard scene.")]
        [Aliases("disable", "poweroff", "exit", "quit")]
        [RequireOwner]
        public async Task ShutDown(CommandContext ctx)
        {
            await ctx.RespondAsync("https://www.youtube.com/watch?v=4rbfuw0UN2A");
            await ctx.Client.DisconnectAsync();
            Environment.Exit(0);
        }
        #endregion

        #region COMMAND_SUDO
        [Command("sudo"), Description("Executes a command as another user."), Hidden]
        [RequireOwner]
        public async Task Sudo(CommandContext ctx, [Description("Member to execute as.")] DiscordMember member, [RemainingText, Description("Command text to execute.")] string command)
        {
            await ctx.TriggerTypingAsync();
            var cmds = ctx.Client.GetCommandsNext();
            await cmds.SudoAsync(member, ctx.Channel, command);
        }
        #endregion
    }
}
