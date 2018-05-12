#region USING_DIRECTIVES
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using TheGodfather.Common;
using TheGodfather.Common.Attributes;
using TheGodfather.Exceptions;
using TheGodfather.Extensions;
using TheGodfather.Services;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Net.Models;
#endregion

namespace TheGodfather.Modules.Administration
{
    public partial class GuildModule
    {
        [Group("configure"), Module(ModuleType.Administration)]
        [Description("Allows manipulation of guild settings for this bot. If invoked without subcommands, starts an interactive settings setup for this guild.")]
        [Aliases("config", "cfg", "setup")]
        [UsageExample("!guild configure")]
        [Cooldown(3, 5, CooldownBucketType.Guild)]
        [RequireUserPermissions(Permissions.ManageGuild)]
        [ListeningCheck]
        public partial class ConfigModule : TheGodfatherBaseModule
        {

            public ConfigModule(SharedData shared, DBService db) : base(shared, db) { }


            [GroupCommand]
            public async Task ExecuteGroupAsync(CommandContext ctx)
            {
                // TODO
            }


            #region COMMAND_GUILD_GETWELCOMECHANNEL
            [Command("getwelcomechannel"), Module(ModuleType.Administration)]
            [Description("Get current welcome message channel for this guild.")]
            [Aliases("getwelcomec", "getwc", "welcomechannel", "wc")]
            [UsageExample("!guild getwelcomechannel")]
            public async Task GetWelcomeChannelAsync(CommandContext ctx)
            {
                ulong cid = await Database.GetWelcomeChannelIdAsync(ctx.Guild.Id)
                    .ConfigureAwait(false);
                if (cid != 0) {
                    var c = ctx.Guild.GetChannel(cid);
                    if (c == null)
                        throw new CommandFailedException($"Welcome channel was set but does not exist anymore (id: {cid}).");
                    await ctx.RespondWithIconEmbedAsync($"Welcome message channel: {Formatter.Bold(ctx.Guild.GetChannel(cid).Name)}.")
                        .ConfigureAwait(false);
                } else {
                    await ctx.RespondWithIconEmbedAsync("Welcome message channel isn't set for this guild.")
                        .ConfigureAwait(false);
                }
            }
            #endregion

            #region COMMAND_GUILD_GETLEAVECHANNEL
            [Command("getleavechannel"), Module(ModuleType.Administration)]
            [Description("Get current leave message channel for this guild.")]
            [Aliases("getleavec", "getlc", "leavechannel", "lc")]
            [UsageExample("!guild getleavechannel")]
            public async Task GetLeaveChannelAsync(CommandContext ctx)
            {
                ulong cid = await Database.GetLeaveChannelIdAsync(ctx.Guild.Id)
                    .ConfigureAwait(false);
                if (cid != 0) {
                    var c = ctx.Guild.GetChannel(cid);
                    if (c == null)
                        throw new CommandFailedException($"Leave channel was set but does not exist anymore (id: {cid}).");
                    await ctx.RespondWithIconEmbedAsync($"Leave message channel: {Formatter.Bold(c.Name)}.")
                        .ConfigureAwait(false);
                } else {
                    await ctx.RespondWithIconEmbedAsync("Leave message channel isn't set for this guild.")
                        .ConfigureAwait(false);
                }
            }
            #endregion

            #region COMMAND_GUILD_GETWELCOMEMESSAGE
            [Command("getwelcomemessage"), Module(ModuleType.Administration)]
            [Description("Get current welcome message for this guild.")]
            [Aliases("getwelcomem", "getwm", "welcomemessage", "wm", "welcomemsg", "wmsg")]
            [UsageExample("!guild getwelcomemessage")]
            public async Task GetWelcomeMessageAsync(CommandContext ctx)
            {
                var msg = await Database.GetWelcomeMessageAsync(ctx.Guild.Id)
                    .ConfigureAwait(false);
                await ctx.RespondWithIconEmbedAsync($"Welcome message:\n\n{Formatter.Italic(msg ?? "Not set.")}")
                    .ConfigureAwait(false);
            }
            #endregion

            #region COMMAND_GUILD_GETLEAVEMESSAGE
            [Command("getleavemessage"), Module(ModuleType.Administration)]
            [Description("Get current leave message for this guild.")]
            [Aliases("getleavem", "getlm", "leavemessage", "lm", "leavemsg", "lmsg")]
            [UsageExample("!guild getwelcomemessage")]
            public async Task GetLeaveMessageAsync(CommandContext ctx)
            {
                var msg = await Database.GetLeaveMessageAsync(ctx.Guild.Id)
                    .ConfigureAwait(false);
                await ctx.RespondWithIconEmbedAsync($"Leave message:\n\n{Formatter.Italic(msg ?? "Not set.")}")
                    .ConfigureAwait(false);
            }
            #endregion

            #region COMMAND_GUILD_SETWELCOMECHANNEL
            [Command("setwelcomechannel"), Module(ModuleType.Administration)]
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
                    throw new CommandFailedException("Welcome channel must be a text channel.");

                await Database.SetWelcomeChannelAsync(ctx.Guild.Id, channel.Id)
                    .ConfigureAwait(false);
                await ctx.RespondWithIconEmbedAsync($"Welcome message channel set to {Formatter.Bold(channel.Name)}.")
                    .ConfigureAwait(false);
            }
            #endregion

            #region COMMAND_GUILD_SETLEAVECHANNEL
            [Command("setleavechannel"), Module(ModuleType.Administration)]
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
                    throw new CommandFailedException("Leave channel must be a text channel.");

                await Database.SetLeaveChannelAsync(ctx.Guild.Id, channel.Id)
                    .ConfigureAwait(false);
                await ctx.RespondWithIconEmbedAsync($"Leave message channel set to {Formatter.Bold(channel.Name)}.")
                    .ConfigureAwait(false);
            }
            #endregion

            #region COMMAND_GUILD_SETWELCOMEMESSAGE
            [Command("setwelcomemessage"), Module(ModuleType.Administration)]
            [Description("Set welcome message for this guild. Any occurances of ``%user%`` inside the string will be replaced with newly joined user mention. Invoking command without a message will reset the current welcome message to a default one.")]
            [Aliases("setwm", "setwelcomem", "setwelcomemsg", "setwmsg")]
            [UsageExample("!guild setwelcomemessage")]
            [UsageExample("!guild setwelcomemessage Welcome, %user%!")]
            [RequireUserPermissions(Permissions.ManageGuild)]
            public async Task SetWelcomeMessageAsync(CommandContext ctx,
                                                    [RemainingText, Description("Message.")] string message = null)
            {
                if (string.IsNullOrWhiteSpace(message)) {
                    await Database.RemoveWelcomeMessageAsync(ctx.Guild.Id)
                        .ConfigureAwait(false);
                    await ctx.RespondWithIconEmbedAsync("Welcome message set to default message.")
                        .ConfigureAwait(false);
                } else {
                    if (message.Length < 3 || message.Length > 120)
                        throw new CommandFailedException("Message cannot be shorter than 3 or longer than 120 characters!");

                    await Database.SetWelcomeMessageAsync(ctx.Guild.Id, message)
                        .ConfigureAwait(false);

                    await ctx.RespondWithIconEmbedAsync($"Welcome message set to: {Formatter.Bold(message ?? "Default message")}.")
                        .ConfigureAwait(false);
                }
            }
            #endregion

            #region COMMAND_GUILD_SETLEAVEMESSAGE
            [Command("setleavemessage"), Module(ModuleType.Administration)]
            [Description("Set leave message for this guild. Any occurances of ``%user%`` inside the string will be replaced with newly joined user mention. Invoking command without a message will reset the current leave message to a default one.")]
            [Aliases("setlm", "setleavem", "setleavemsg", "setlmsg")]
            [UsageExample("!guild setleavemessage")]
            [UsageExample("!guild setleavemessage Bye, %user%!")]
            [RequireUserPermissions(Permissions.ManageGuild)]
            public async Task SetLeaveMessageAsync(CommandContext ctx,
                                                  [RemainingText, Description("Message.")] string message = null)
            {
                if (string.IsNullOrWhiteSpace(message)) {
                    await Database.RemoveLeaveMessageAsync(ctx.Guild.Id)
                        .ConfigureAwait(false);
                    await ctx.RespondWithIconEmbedAsync("Leave message set to default message.")
                        .ConfigureAwait(false);
                } else {
                    if (message.Length < 3 || message.Length > 120)
                        throw new CommandFailedException("Message cannot be shorter than 3 or longer than 120 characters!");

                    await Database.SetLeaveMessageAsync(ctx.Guild.Id, message)
                        .ConfigureAwait(false);

                    await ctx.RespondWithIconEmbedAsync($"Leave message set to: {Formatter.Bold(message ?? "Default message")}.")
                        .ConfigureAwait(false);
                }
            }
            #endregion

            #region COMMAND_GUILD_DELETEWELCOMECHANNEL
            [Command("deletewelcomechannel"), Module(ModuleType.Administration)]
            [Description("Remove welcome message channel for this guild.")]
            [Aliases("delwelcomec", "delwc", "delwelcome", "dwc", "deletewc")]
            [UsageExample("!guild deletewelcomechannel")]
            [RequireUserPermissions(Permissions.ManageGuild)]
            public async Task RemoveWelcomeChannelAsync(CommandContext ctx)
            {
                await Database.RemoveWelcomeChannelAsync(ctx.Guild.Id)
                    .ConfigureAwait(false);
                await ctx.RespondWithIconEmbedAsync("Default welcome message channel removed.")
                    .ConfigureAwait(false);
            }
            #endregion

            #region COMMAND_GUILD_DELETELEAVECHANNEL
            [Command("deleteleavechannel"), Module(ModuleType.Administration)]
            [Description("Remove leave message channel for this guild.")]
            [Aliases("delleavec", "dellc", "delleave", "dlc")]
            [UsageExample("!guild deletewelcomechannel")]
            [RequireUserPermissions(Permissions.ManageGuild)]
            public async Task DeleteLeaveChannelAsync(CommandContext ctx)
            {
                await Database.RemoveLeaveChannelAsync(ctx.Guild.Id)
                    .ConfigureAwait(false);
                await ctx.RespondWithIconEmbedAsync("Default leave message channel removed.")
                    .ConfigureAwait(false);
            }
            #endregion

            #region COMMAND_GUILD_DELETEWELCOMEMESSAGE
            [Command("deletewelcomemessage"), Module(ModuleType.Administration)]
            [Description("Remove welcome message for this guild.")]
            [Aliases("delwelcomem", "delwm", "delwelcomemsg", "dwm", "deletewm", "dwmsg")]
            [UsageExample("!guild deletewelcomemessage")]
            [RequireUserPermissions(Permissions.ManageGuild)]
            public async Task RemoveWelcomeMessageAsync(CommandContext ctx)
            {
                await Database.RemoveWelcomeMessageAsync(ctx.Guild.Id)
                    .ConfigureAwait(false);
                await ctx.RespondWithIconEmbedAsync("Default welcome message removed.")
                    .ConfigureAwait(false);
            }
            #endregion

            #region COMMAND_GUILD_DELETEWELCOMEMESSAGE
            [Command("deleteleavemessage"), Module(ModuleType.Administration)]
            [Description("Remove leave message for this guild.")]
            [Aliases("delleavem", "dellm", "delleavemsg", "dlm", "deletelm", "dwlsg")]
            [UsageExample("!guild deleteleavemessage")]
            [RequireUserPermissions(Permissions.ManageGuild)]
            public async Task RemoveLeaveChannelAsync(CommandContext ctx)
            {
                await Database.RemoveLeaveMessageAsync(ctx.Guild.Id)
                    .ConfigureAwait(false);
                await ctx.RespondWithIconEmbedAsync("Default leave message removed.")
                    .ConfigureAwait(false);
            }
            #endregion
        }
    }
}
