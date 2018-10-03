#region USING_DIRECTIVES
using DSharpPlus;

using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using TheGodfather.Common.Attributes;
#endregion

namespace TheGodfather.Common
{
    internal static class AsyncExecutionManager
    {
        public static IEnumerable<ListenerMethod> ListenerMethods { get; private set; }

        public static void RegisterEventListeners(DiscordClient client, TheGodfatherShard shard)
        {
            ListenerMethods =
                from types in Assembly.GetExecutingAssembly().GetTypes()
                from methods in types.GetMethods()
                let attribute = methods.GetCustomAttribute(typeof(AsyncEventListenerAttribute), inherit: true)
                where !(attribute is null)
                select new ListenerMethod {
                    Method = methods,
                    Attribute = attribute as AsyncEventListenerAttribute
                };

            foreach (ListenerMethod lm in ListenerMethods)
                lm.Attribute.Register(shard, client, lm.Method);
        }
    }


    internal class ListenerMethod
    {
        public MethodInfo Method { get; internal set; }
        public AsyncEventListenerAttribute Attribute { get; internal set; }
    }
}