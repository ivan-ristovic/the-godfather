using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DSharpPlus.Entities;
using Microsoft.Extensions.DependencyInjection;
using TheGodfather.EventListeners.Attributes;
using TheGodfather.Modules.Administration.Extensions;
using TheGodfather.Modules.Administration.Services;
using TheGodfather.Services;

namespace TheGodfather.EventListeners
{
    internal static partial class Listeners
    {
        public static IEnumerable<ListenerMethod> ListenerMethods { get; private set; } = Enumerable.Empty<ListenerMethod>();

        public static void FindAndRegister(TheGodfatherShard shard)
        {
            ListenerMethods =
                from t in Assembly.GetExecutingAssembly().GetTypes()
                from m in t.GetMethods()
                let a = m.GetCustomAttribute(typeof(AsyncEventListenerAttribute), inherit: true)
                where a is { }
                select new ListenerMethod(m, (AsyncEventListenerAttribute)a);

            foreach (ListenerMethod lm in ListenerMethods)
                lm.Attribute.Register(shard, lm.Method);
        }


        private static bool IsLogEnabledForGuild(TheGodfatherShard shard, ulong gid, out LoggingService logService, out LocalizedEmbedBuilder emb)
        {
            logService = shard.Services.GetRequiredService<LoggingService>();
            return logService.IsLogEnabledFor(gid, out emb);
        }

        private static bool IsChannelExempted(TheGodfatherShard shard, DiscordGuild? guild, DiscordChannel channel, out GuildConfigService gcs)
        {
            gcs = shard.Services.GetRequiredService<GuildConfigService>();
            return guild is { } ? gcs.IsChannelExempted(guild.Id, channel.Id, channel.ParentId) : false;
        }
    }


    internal sealed class ListenerMethod
    {
        public MethodInfo Method { get; }
        public AsyncEventListenerAttribute Attribute { get; }

        public ListenerMethod(MethodInfo mi, AsyncEventListenerAttribute attr)
        {
            this.Method = mi;
            this.Attribute = attr;
        }
    }
}