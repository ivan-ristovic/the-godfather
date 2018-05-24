#region USING_DIRECTIVES
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using TheGodfather.Common.Attributes;

using DSharpPlus;
#endregion

namespace TheGodfather.Common
{
    internal static class AsyncEventListenerHandler
    {
        public static IEnumerable<ListenerMethod> ListenerMethods { get; private set; }

        public static void InstallListeners(DiscordClient client, TheGodfatherShard bot)
        {
            var assembly = Assembly.GetExecutingAssembly();

            ListenerMethods =
                from types in assembly.GetTypes()
                from methods in types.GetMethods()
                let attribute = methods.GetCustomAttribute(typeof(AsyncEventListenerAttribute), true)
                where attribute != null
                select new ListenerMethod { Method = methods, Attribute = attribute as AsyncEventListenerAttribute };

            foreach (var listener in ListenerMethods)
                listener.Attribute.Register(bot, client, listener.Method);
        }
    }


    internal class ListenerMethod
    {
        public MethodInfo Method { get; internal set; }
        public AsyncEventListenerAttribute Attribute { get; internal set; }
    }
}