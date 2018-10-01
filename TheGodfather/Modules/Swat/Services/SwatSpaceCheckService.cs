#region USING_DIRECTIVES
using DSharpPlus;
using DSharpPlus.Entities;

using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;

using TheGodfather.Common;
using TheGodfather.Common.Collections;
using TheGodfather.Exceptions;
using TheGodfather.Extensions;
using TheGodfather.Modules.Swat.Common;
#endregion

namespace TheGodfather.Modules.Swat.Services
{
    public static class SwatSpaceCheckService
    {
        private static readonly ConcurrentDictionary<SwatServer, ConcurrentHashSet<DiscordChannel>> _listeners = new ConcurrentDictionary<SwatServer, ConcurrentHashSet<DiscordChannel>>(new SwatServerComparer());
        private static readonly object _lock = new object();
        private static readonly AsyncExecutor _async = new AsyncExecutor();
        private static Timer _ticker;


        public static void AddListener(SwatServer server, DiscordChannel channel)
        {
            if (_listeners.Count > 10 || _listeners.Any(kvp => kvp.Value.Count > 10))
                throw new Exception("Maximum amount of simultanous checks reached. Please try again later.");

            lock (_lock) {
                _listeners.AddOrUpdate(
                    server,
                    new ConcurrentHashSet<DiscordChannel>() { channel },
                    (k, v) => {
                        v.Add(channel);
                        return v;
                    }
                );
            }

             Start();
        }

        public static bool IsListening(DiscordChannel channel)
            => _listeners.Any(kvp => kvp.Value.Contains(channel));

        public static void RemoveListener(DiscordChannel channel)
        {
            lock (_lock) {
                foreach ((SwatServer server, ConcurrentHashSet<DiscordChannel> listeners) in _listeners)
                    if (!listeners.TryRemove(channel))
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
                    SwatServerInfo info = _async.Execute(SwatServerInfo.QueryIPAsync(server.Ip, server.QueryPort));
                    if (info is null) {
                        foreach (DiscordChannel channel in _listeners[server]) {
                            try {
                                _async.Execute(channel.InformFailureAsync($"No reply from {server.Ip}:{server.JoinPort}"));
                            } catch {
                                _listeners[server].TryRemove(channel);
                            }
                        }
                    } else if (info.HasSpace) {
                        foreach (DiscordChannel channel in _listeners[server]) {
                            try {
                                _async.Execute(channel.EmbedAsync($"There is space on {Formatter.Bold(info.HostName)}!", StaticDiscordEmoji.AlarmClock, DiscordColor.Black));
                            } catch {
                                _listeners[server].TryRemove(channel);
                            }
                        }
                    }
                }
            }
        }
    }
}
