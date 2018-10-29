#region USING_DIRECTIVES
using DSharpPlus;
using DSharpPlus.Entities;

using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;

using TheGodfather.Common;
using TheGodfather.Common.Collections;
using TheGodfather.Database.Entities;
using TheGodfather.Exceptions;
using TheGodfather.Extensions;
using TheGodfather.Modules.Swat.Common;
#endregion

namespace TheGodfather.Modules.Swat.Services
{
    public static class SwatSpaceCheckService
    {
        private static readonly ConcurrentDictionary<DatabaseSwatServer, ConcurrentHashSet<DiscordChannel>> _listeners;
        private static readonly object _lock;
        private static readonly AsyncExecutor _async;
        private static Timer _ticker;

        static SwatSpaceCheckService()
        {
            _listeners = new ConcurrentDictionary<DatabaseSwatServer, ConcurrentHashSet<DiscordChannel>>(new DatabaseSwatServerComparer());
            _lock = new object();
            _async = new AsyncExecutor();
        }


        public static void AddListener(DatabaseSwatServer server, DiscordChannel channel)
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
                foreach ((DatabaseSwatServer server, ConcurrentHashSet<DiscordChannel> listeners) in _listeners)
                    if (!listeners.TryRemove(channel))
                        throw new ConcurrentOperationException("Failed to unregister space check task! Please try again.");

                var toRemove = _listeners
                    .Where(kvp => kvp.Value.IsEmpty)
                    .Select(kvp => kvp.Key)
                    .ToList();
                foreach (DatabaseSwatServer server in toRemove)
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
                foreach (DatabaseSwatServer server in _listeners.Keys) {
                    SwatServerInfo info = _async.Execute(SwatServerInfo.QueryIPAsync(server.IP, server.QueryPort));
                    if (info is null) {
                        foreach (DiscordChannel channel in _listeners[server]) {
                            try {
                                _async.Execute(channel.InformFailureAsync($"No reply from {server.IP}:{server.JoinPort}"));
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
