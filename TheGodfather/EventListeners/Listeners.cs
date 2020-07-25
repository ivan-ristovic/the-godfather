using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DSharpPlus.Entities;
using Microsoft.Extensions.DependencyInjection;
using TheGodfather.EventListeners.Attributes;
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


        private static bool IsLogEnabledForGuild(TheGodfatherShard shard, ulong gid, out LoggingService logService, out NewDiscordLogEmbedBuilder emb)
        {
            logService = shard.Services.GetService<LoggingService>() ?? throw new InvalidOperationException("Localization service is null");
            return logService.IsLogEnabledFor(gid, out emb);
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