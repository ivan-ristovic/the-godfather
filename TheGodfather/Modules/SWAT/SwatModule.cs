#region USING_DIRECTIVES
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using TheGodfather.Attributes;
using TheGodfather.Entities.SWAT;
using TheGodfather.Exceptions;
using TheGodfather.Extensions;
using TheGodfather.Services;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
#endregion

namespace TheGodfather.Modules.SWAT
{
    [Group("swat")]
    [Description("SWAT4 related commands.")]
    [Aliases("s4", "swat4")]
    [Cooldown(2, 5, CooldownBucketType.User), Cooldown(3, 5, CooldownBucketType.Guild)]
    [ListeningCheck]
    public partial class SwatModule : TheGodfatherBaseModule
    {

        public SwatModule(SharedData shared, DatabaseService db) : base(shared, db) { }


        #region COMMAND_IP
        [Command("ip")]
        [Description("Return IP of the registered server by name.")]
        [Aliases("getip")]
        [UsageExample("!s4 ip wm")]
        public async Task QueryAsync(CommandContext ctx,
                                    [Description("Registered name.")] string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new InvalidCommandUsageException("Name missing.");

            var server = await DatabaseService.GetSwatServerFromDatabaseAsync(name.ToLowerInvariant())
                .ConfigureAwait(false);
            if (server == null)
                throw new CommandFailedException("Server with such name isn't found in the database.");

            await ReplyWithEmbedAsync(ctx, $"IP: {Formatter.Bold($"{server.IP}:{server.JoinPort}")}")
                .ConfigureAwait(false);
        }
        #endregion

        #region COMMAND_QUERY
        [Command("query")]
        [Description("Return server information.")]
        [Aliases("q", "info", "i")]
        [UsageExample("!s4 q 109.70.149.158")]
        [UsageExample("!s4 q 109.70.149.158:10480")]
        [UsageExample("!s4 q wm")]
        public async Task QueryAsync(CommandContext ctx,
                                    [Description("Registered name or IP.")] string ip,
                                    [Description("Query port")] int queryport = 10481)
        {
            if (string.IsNullOrWhiteSpace(ip))
                throw new InvalidCommandUsageException("IP missing.");

            if (queryport <= 0 || queryport > 65535)
                throw new InvalidCommandUsageException("Port range invalid (must be in range [1-65535])!");

            var server = await DatabaseService.GetSwatServerAsync(ip, queryport, name: ip.ToLowerInvariant())
                .ConfigureAwait(false);

            var info = await SwatServerInfo.QueryIPAsync(server.IP, server.QueryPort)
                .ConfigureAwait(false);

            if (info != null)
                await ctx.RespondAsync(embed: info.EmbedData()).ConfigureAwait(false);
            else
                await ReplyWithFailedEmbedAsync(ctx, "No reply from server.").ConfigureAwait(false);
        }
        #endregion

        #region COMMAND_SETTIMEOUT
        [Command("settimeout")]
        [Description("Set checking timeout.")]
        [UsageExample("!swat settimeout 500")]
        [RequireOwner]
        public async Task SetTimeoutAsync(CommandContext ctx,
                                         [Description("Timeout (in ms).")] int timeout)
        {
            if (timeout < 100 || timeout > 1000)
                throw new InvalidCommandUsageException("Timeout not in valid range [100-1000] ms.");
            SwatServerInfo.CheckTimeout = timeout;
            await ctx.RespondAsync("Timeout changed to: " + Formatter.Bold(timeout.ToString()))
                .ConfigureAwait(false);
        }
        #endregion

        #region COMMAND_SERVERLIST
        [Command("serverlist")]
        [Description("Print the serverlist with current player numbers.")]
        [UsageExample("!swat serverlist")]
        public async Task ServerlistAsync(CommandContext ctx)
        {
            var em = new DiscordEmbedBuilder() {
                Title = "Servers",
                Color = DiscordColor.DarkBlue
            };

            var servers = await DatabaseService.GetAllSwatServersAsync()
                .ConfigureAwait(false);

            if (servers == null || !servers.Any())
                throw new CommandFailedException("No servers found in the database.");

            foreach (var server in servers) {
                var info = await SwatServerInfo.QueryIPAsync(server.IP, server.QueryPort)
                    .ConfigureAwait(false);
                if (info != null)
                    em.AddField(info.HostName, $"IP: {server.IP}:{server.JoinPort}\nPlayers: {Formatter.Bold(info.Players + " / " + info.MaxPlayers)}");
                else
                    em.AddField(server.Name, $"IP: {server.IP}:{server.JoinPort}\nPlayers: Offline");
            }
            await ctx.RespondAsync(embed: em.Build())
                .ConfigureAwait(false);
        }
        #endregion

        #region COMMAND_STARTCHECK
        [Command("startcheck")]
        [Description("Start listening for space on a given server and notifies you when there is space.")]
        [Aliases("checkspace", "spacecheck")]
        [UsageExample("!s4 startcheck 109.70.149.158")]
        [UsageExample("!s4 startcheck 109.70.149.158:10480")]
        [UsageExample("!swat startcheck wm")]
        public async Task StartCheckAsync(CommandContext ctx,
                                         [Description("Registered name or IP.")] string ip,
                                         [Description("Query port")] int queryport = 10481)
        {
            if (string.IsNullOrWhiteSpace(ip))
                throw new InvalidCommandUsageException("Name/IP missing.");

            if (queryport <= 0 || queryport > 65535)
                throw new InvalidCommandUsageException("Port range invalid (must be in range [1-65535])!");

            if (SharedData.UserIDsCheckingForSpace.Contains(ctx.User.Id))
                throw new CommandFailedException("Already checking space for you!");

            if (SharedData.UserIDsCheckingForSpace.Count > 10)
                throw new CommandFailedException("Maximum number of checks reached, please try later!");

            var server = await DatabaseService.GetSwatServerAsync(ip, queryport, name: ip.ToLowerInvariant())
                .ConfigureAwait(false);
            await ReplyWithEmbedAsync(ctx, $"Starting check on {server.IP}:{server.JoinPort}...")
                .ConfigureAwait(false);

            SharedData.UserIDsCheckingForSpace.Add(ctx.User.Id);
            try {
                while (SharedData.UserIDsCheckingForSpace.Contains(ctx.User.Id)) {
                    var info = await SwatServerInfo.QueryIPAsync(server.IP, server.QueryPort)
                        .ConfigureAwait(false);
                    if (info == null) {
                        if (!await AskYesNoQuestionAsync(ctx, "No reply from server. Should I try again?").ConfigureAwait(false)) {
                            await StopCheckAsync(ctx)
                                .ConfigureAwait(false);
                            return;
                        }
                    } else if (info.HasSpace()) {
                        await ReplyWithEmbedAsync(ctx, $"{ctx.User.Mention}, there is space on {info.HostName}!", ":alarm_clock:")
                            .ConfigureAwait(false);
                    }
                    await Task.Delay(TimeSpan.FromSeconds(2))
                        .ConfigureAwait(false);
                }
            } catch (Exception e) {
                await StopCheckAsync(ctx)
                    .ConfigureAwait(false);
                throw e;
            } finally {
                SharedData.UserIDsCheckingForSpace.TryRemove(ctx.User.Id);
            }
        }
        #endregion

        #region COMMAND_STOPCHECK
        [Command("stopcheck")]
        [Description("Stops space checking.")]
        [Aliases("checkstop")]
        [UsageExample("!swat stopcheck")]
        public async Task StopCheckAsync(CommandContext ctx)
        {
            if (!SharedData.UserIDsCheckingForSpace.Contains(ctx.User.Id))
                throw new CommandFailedException("No checks started from you.");
            SharedData.UserIDsCheckingForSpace.TryRemove(ctx.User.Id);
            await ReplyWithEmbedAsync(ctx, "Checking stopped.")
                .ConfigureAwait(false);
        }
        #endregion
    }
}
