#region USING_DIRECTIVES
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using DSharpPlus;
using DSharpPlus.Entities;
using TheGodfather.Common;
using TheGodfather.Common.Collections;
using TheGodfather.Database.Models;
using TheGodfather.Exceptions;
using TheGodfather.Extensions;
using TheGodfather.Modules.Swat.Common;
using TheGodfather.Services;
#endregion

namespace TheGodfather.Modules.Swat.Services
{
    public static class SwatSpaceCheckService
    {
        private sealed class DiscordChannelInfo
        {
            public DiscordChannel Channel { get; }
            public ulong LastMessageId { get; set; }
            public bool Success { get; set; }


            public DiscordChannelInfo(DiscordChannel channel, ulong? mid = null)
            {
                this.Channel = channel;
                this.LastMessageId = mid ?? channel.LastMessageId;
            }
        }


        private static readonly ConcurrentDictionary<SwatServer, ConcurrentHashSet<DiscordChannelInfo>> _listeners;
        private static readonly object _lock;
        private static readonly AsyncExecutionService _async;   // FIXME
        private static Timer _ticker;

        static SwatSpaceCheckService()
        {
            _listeners = new ConcurrentDictionary<SwatServer, ConcurrentHashSet<DiscordChannelInfo>>(new SwatServerComparer());
            _lock = new object();
            _async = new AsyncExecutionService();
        }


        public static void AddListener(SwatServer server, DiscordChannel channel)
        {
            if (_listeners.Count > 10 || _listeners.Any(kvp => kvp.Value.Count > 10))
                throw new Exception("Maximum amount of simultanous checks reached. Please try again later.");

            lock (_lock) {
                _listeners.AddOrUpdate(
                    server,
                    new ConcurrentHashSet<DiscordChannelInfo> { new DiscordChannelInfo(channel) },
                    (k, v) => {
                        v.Add(new DiscordChannelInfo(channel));
                        return v;
                    }
                );
            }

            Start();
        }

        public static bool IsListening(DiscordChannel channel)
            => _listeners.Any(kvp => kvp.Value.Any(c => c.Channel == channel));

        public static void RemoveListener(DiscordChannel channel)
        {
            lock (_lock) {
                foreach ((SwatServer server, ConcurrentHashSet<DiscordChannelInfo> listeners) in _listeners)
                    if (listeners.RemoveWhere(c => c.Channel == channel) <= 0)
                        throw new ConcurrentOperationException("Failed to unregister space check task! Please try again.");

                var toRemove = _listeners
                    .Where(kvp => kvp.Value.IsEmpty)
                    .Select(kvp => kvp.Key)
                    .ToList();
                foreach (SwatServer server in toRemove)
                    _listeners.TryRemove(server, out _);
            }
        }


        private static void Start()
        {
            if (_ticker is null)
                _ticker = new Timer(CheckCallback, null, TimeSpan.Zero, TimeSpan.FromSeconds(5));
            else
                _ticker.Change(TimeSpan.Zero, TimeSpan.FromSeconds(5));
        }

        private static void CheckCallback(object _)
        {
            if (_listeners.IsEmpty) {
                _ticker.Change(Timeout.Infinite, Timeout.Infinite);
                return;
            }

            lock (_lock) {
                foreach (SwatServer server in _listeners.Keys) {
                    SwatServerInfo info = _async.Execute(SwatServerInfo.QueryIPAsync(server.IP, server.QueryPort));
                    if (info is null) {
                        foreach (DiscordChannelInfo ci in _listeners[server]) {
                            try {
                                if (ci.Success == true || ci.LastMessageId != ci.LastMessageId)
                                    ci.LastMessageId = _async.Execute(ci.Channel.InformFailureAsync($"No reply from {server.IP}:{server.JoinPort}")).Id;
                                ci.Success = false;
                            } catch {
                                _listeners[server].TryRemove(ci);
                            }
                        }
                    } else if (info.HasSpace) {
                        foreach (DiscordChannelInfo ci in _listeners[server]) {
                            try {
                                if (ci.Success == false || ci.LastMessageId != ci.Channel.LastMessageId)
                                    ci.LastMessageId = _async.Execute(ci.Channel.EmbedAsync($"There is space on {Formatter.Bold(info.HostName)}!", Emojis.AlarmClock, DiscordColor.Black)).Id;
                                ci.Success = true;
                            } catch {
                                _listeners[server].TryRemove(ci);
                            }
                        }
                    }
                }
            }
        }
    }
}
