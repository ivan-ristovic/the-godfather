#region USING_DIRECTIVES
using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

using TheGodfather.Common.Attributes;
using TheGodfather.Exceptions;
using TheGodfather.Extensions;
using TheGodfather.Modules.SWAT.Common;
using TheGodfather.Services;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
#endregion

namespace TheGodfather.Modules.SWAT
{
    [Group("swat"), Module(ModuleType.SWAT)]
    [Description("SWAT4 related commands.")]
    [Aliases("s4", "swat4")]
    [Cooldown(3, 5, CooldownBucketType.Channel)]
    [NotBlocked]
    public partial class SwatModule : TheGodfatherBaseModule
    {

        public SwatModule(SharedData shared, DBService db) : base(shared, db) { }


        #region COMMAND_IP
        [Command("ip"), Module(ModuleType.SWAT)]
        [Description("Return IP of the registered server by name.")]
        [Aliases("getip")]
        [UsageExample("!s4 ip wm")]
        public async Task QueryAsync(CommandContext ctx,
                                    [Description("Registered name.")] string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new InvalidCommandUsageException("Name missing.");

            var server = await Database.GetSwatServerFromDatabaseAsync(name.ToLowerInvariant())
                .ConfigureAwait(false);
            if (server == null)
                throw new CommandFailedException("Server with such name isn't found in the database.");

            await ctx.RespondWithIconEmbedAsync($"IP: {Formatter.Bold($"{server.IP}:{server.JoinPort}")}")
                .ConfigureAwait(false);
        }
        #endregion

        #region COMMAND_QUERY
        [Command("query"), Module(ModuleType.SWAT)]
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

            var server = await Database.GetSwatServerAsync(ip, queryport, name: ip.ToLowerInvariant())
                .ConfigureAwait(false);

            var info = await SwatServerInfo.QueryIPAsync(server.IP, server.QueryPort)
                .ConfigureAwait(false);

            if (info != null)
                await ctx.RespondAsync(embed: info.EmbedData()).ConfigureAwait(false);
            else
                await ctx.RespondWithFailedEmbedAsync("No reply from server.").ConfigureAwait(false);
        }
        #endregion

        #region COMMAND_SETTIMEOUT
        [Command("settimeout"), Module(ModuleType.SWAT)]
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
        [Command("serverlist"), Module(ModuleType.SWAT)]
        [Description("Print the serverlist with current player numbers.")]
        [UsageExample("!swat serverlist")]
        public async Task ServerlistAsync(CommandContext ctx)
        {
            var em = new DiscordEmbedBuilder() {
                Title = "Servers",
                Color = DiscordColor.Black
            };

            var servers = await Database.GetAllSwatServersAsync()
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
        [Command("startcheck"), Module(ModuleType.SWAT)]
        [Description("Start listening for space on a given server and notifies you when there is space.")]
        [Aliases("checkspace", "spacecheck")]
        [UsageExample("!s4 startcheck 109.70.149.158")]
        [UsageExample("!s4 startcheck 109.70.149.158:10480")]
        [UsageExample("!swat startcheck wm")]
        [InteractivitySensitive]
        public async Task StartCheckAsync(CommandContext ctx,
                                         [Description("Registered name or IP.")] string ip,
                                         [Description("Query port")] int queryport = 10481)
        {
            if (string.IsNullOrWhiteSpace(ip))
                throw new InvalidCommandUsageException("Name/IP missing.");

            if (queryport <= 0 || queryport > 65535)
                throw new InvalidCommandUsageException("Port range invalid (must be in range [1-65535])!");

            if (Shared.SpaceCheckingCTS.ContainsKey(ctx.User.Id))
                throw new CommandFailedException("Already checking space for you!");

            if (Shared.SpaceCheckingCTS.Count > 10)
                throw new CommandFailedException("Maximum number of simultanous checks reached (10), please try later!");

            var server = await Database.GetSwatServerAsync(ip, queryport, name: ip.ToLowerInvariant())
                .ConfigureAwait(false);
            await ctx.RespondWithIconEmbedAsync($"Starting space listening on {server.IP}:{server.JoinPort}...")
                .ConfigureAwait(false);

            if (!Shared.SpaceCheckingCTS.TryAdd(ctx.User.Id, new CancellationTokenSource()))
                throw new CommandFailedException("Failed to register space check task! Please try again.");

            try {
                var t = Task.Run(async () => {
                    while (Shared.SpaceCheckingCTS.ContainsKey(ctx.User.Id) && !Shared.SpaceCheckingCTS[ctx.User.Id].IsCancellationRequested) {
                        var info = await SwatServerInfo.QueryIPAsync(server.IP, server.QueryPort)
                            .ConfigureAwait(false);
                        if (info == null) {
                            if (!await ctx.AskYesNoQuestionAsync("No reply from server. Should I try again?").ConfigureAwait(false)) {
                                await StopCheckAsync(ctx)
                                    .ConfigureAwait(false);
                                throw new OperationCanceledException();
                            }
                        } else if (info.HasSpace()) {
                            await ctx.RespondWithIconEmbedAsync($"{ctx.User.Mention}, there is space on {Formatter.Bold(info.HostName)}!", ":alarm_clock:")
                                .ConfigureAwait(false);
                        }
                        await Task.Delay(TimeSpan.FromSeconds(2))
                            .ConfigureAwait(false);
                    }
                }, Shared.SpaceCheckingCTS[ctx.User.Id].Token);
            } catch {
                Shared.SpaceCheckingCTS.TryRemove(ctx.User.Id, out _);
            }
        }
        #endregion

        #region COMMAND_STOPCHECK
        [Command("stopcheck"), Module(ModuleType.SWAT)]
        [Description("Stops space checking.")]
        [Aliases("checkstop")]
        [UsageExample("!swat stopcheck")]
        public async Task StopCheckAsync(CommandContext ctx)
        {
            if (!Shared.SpaceCheckingCTS.ContainsKey(ctx.User.Id))
                throw new CommandFailedException("You haven't started any space listeners.");
            Shared.SpaceCheckingCTS[ctx.User.Id].Cancel();
            Shared.SpaceCheckingCTS[ctx.User.Id].Dispose();
            Shared.SpaceCheckingCTS.TryRemove(ctx.User.Id, out _);
            await ctx.RespondWithIconEmbedAsync("Checking stopped.")
                .ConfigureAwait(false);
        }
        #endregion
    }
}
