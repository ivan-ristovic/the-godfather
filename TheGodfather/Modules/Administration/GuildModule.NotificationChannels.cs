#region USING_DIRECTIVES
using System.Threading.Tasks;

using TheGodfather.Attributes;
using TheGodfather.Exceptions;
using TheGodfather.Extensions;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
#endregion

namespace TheGodfather.Modules.Administration
{
    public partial class GuildModule
    {
        #region COMMAND_GUILD_GETWELCOMECHANNEL
        [Command("getwelcomechannel")]
        [Description("Get current welcome message channel for this guild.")]
        [Aliases("getwelcomec", "getwc", "getwelcome", "welcomechannel", "wc")]
        [UsageExample("!guild getwelcomechannel")]
        [RequireUserPermissions(Permissions.ManageGuild)]
        public async Task GetWelcomeChannelAsync(CommandContext ctx)
        {
            ulong cid = await Database.GetGuildWelcomeChannelIdAsync(ctx.Guild.Id)
                .ConfigureAwait(false);
            if (cid != 0) {
                var c = ctx.Guild.GetChannel(cid);
                if (c == null)
                    throw new CommandFailedException($"Welcome channel was set but does not exist anymore (id: {cid}).");
                await ctx.RespondWithIconEmbedAsync($"Default welcome message channel: {Formatter.Bold(ctx.Guild.GetChannel(cid).Name)}.")
                    .ConfigureAwait(false);
            } else {
                await ctx.RespondWithIconEmbedAsync("Default welcome message channel isn't set for this guild.")
                    .ConfigureAwait(false);
            }
        }
        #endregion

        #region COMMAND_GUILD_GETLEAVECHANNEL
        [Command("getleavechannel")]
        [Description("Get current leave message channel for this guild.")]
        [Aliases("getleavec", "getlc", "getleave", "leavechannel", "lc")]
        [UsageExample("!guild getleavechannel")]
        [RequireUserPermissions(Permissions.ManageGuild)]
        public async Task GetLeaveChannelAsync(CommandContext ctx)
        {
            ulong cid = await Database.GetGuildLeaveChannelIdAsync(ctx.Guild.Id)
                .ConfigureAwait(false);
            if (cid != 0) {
                var c = ctx.Guild.GetChannel(cid);
                if (c == null)
                    throw new CommandFailedException($"Leave channel was set but does not exist anymore (id: {cid}).");
                await ctx.RespondWithIconEmbedAsync($"Default leave message channel: {Formatter.Bold(c.Name)}.")
                    .ConfigureAwait(false);
            } else {
                await ctx.RespondWithIconEmbedAsync("Default leave message channel isn't set for this guild.")
                    .ConfigureAwait(false);
            }
        }
        #endregion

        #region COMMAND_GUILD_SETWELCOMECHANNEL
        [Command("setwelcomechannel")]
        [Description("Set welcome message channel for this guild. If the channel isn't given, uses the current one.")]
        [Aliases("setwc", "setwelcomec", "setwelcome")]
        [UsageExample("!guild setwelcomechannel")]
        [UsageExample("!guild setwelcomechannel #welcome")]
        [RequireUserPermissions(Permissions.ManageGuild)]
        public async Task SetWelcomeChannelAsync(CommandContext ctx,
                                                [Description("Channel.")] DiscordChannel channel = null)
        {
            if (channel == null)
                channel = ctx.Channel;

            if (channel.Type != ChannelType.Text)
                throw new CommandFailedException("Given channel must be a text channel.");

            await Database.SetGuildWelcomeChannelAsync(ctx.Guild.Id, channel.Id)
                .ConfigureAwait(false);
            await ctx.RespondWithIconEmbedAsync($"Default welcome message channel set to {Formatter.Bold(channel.Name)}.")
                .ConfigureAwait(false);
        }
        #endregion

        #region COMMAND_GUILD_SETLEAVECHANNEL
        [Command("setleavechannel")]
        [Description("Set leave message channel for this guild. If the channel isn't given, uses the current one.")]
        [Aliases("leavec", "setlc", "setleave")]
        [UsageExample("!guild setleavechannel")]
        [UsageExample("!guild setleavechannel #bb")]
        [RequireUserPermissions(Permissions.ManageGuild)]
        public async Task SetLeaveChannelAsync(CommandContext ctx,
                                              [Description("Channel.")] DiscordChannel channel = null)
        {
            if (channel == null)
                channel = ctx.Channel;

            if (channel.Type != ChannelType.Text)
                throw new CommandFailedException("Given channel must be a text channel.");

            await Database.SetGuildLeaveChannelAsync(ctx.Guild.Id, channel.Id)
                .ConfigureAwait(false);
            await ctx.RespondWithIconEmbedAsync($"Default leave message channel set to {Formatter.Bold(channel.Name)}.")
                .ConfigureAwait(false);
        }
        #endregion

        #region COMMAND_GUILD_DELETEWELCOMECHANNEL
        [Command("deletewelcomechannel")]
        [Description("Remove welcome message channel for this guild.")]
        [Aliases("delwelcomec", "delwc", "delwelcome", "dwc", "deletewc")]
        [UsageExample("!guild deletewelcomechannel")]
        [RequireUserPermissions(Permissions.ManageGuild)]
        public async Task RemoveWelcomeChannelAsync(CommandContext ctx)
        {
            await Database.RemoveGuildWelcomeChannelAsync(ctx.Guild.Id)
                .ConfigureAwait(false);
            await ctx.RespondWithIconEmbedAsync("Default welcome message channel removed.")
                .ConfigureAwait(false);
        }
        #endregion

        #region COMMAND_GUILD_DELETELEAVECHANNEL
        [Command("deleteleavechannel")]
        [Description("Remove leave message channel for this guild.")]
        [Aliases("delleavec", "dellc", "delleave", "dlc")]
        [UsageExample("!guild deletewelcomechannel")]
        [RequireUserPermissions(Permissions.ManageGuild)]
        public async Task DeleteLeaveChannelAsync(CommandContext ctx)
        {
            await Database.RemoveGuildLeaveChannelAsync(ctx.Guild.Id)
                .ConfigureAwait(false);
            await ctx.RespondWithIconEmbedAsync("Default leave message channel removed.")
                .ConfigureAwait(false);
        }
        #endregion
    }
}
