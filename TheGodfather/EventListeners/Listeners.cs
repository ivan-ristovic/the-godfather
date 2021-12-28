using System.Reflection;
using TheGodfather.EventListeners.Attributes;

namespace TheGodfather.EventListeners;

internal static partial class Listeners
{
    public static IEnumerable<ListenerMethod> ListenerMethods { get; private set; } = Enumerable.Empty<ListenerMethod>();

    public static void FindAndRegister(TheGodfatherBot shard)
    {
        ListenerMethods =
            from t in Assembly.GetExecutingAssembly().GetTypes()
            from m in t.GetMethods()
            let a = m.GetCustomAttribute(typeof(AsyncEventListenerAttribute), true)
            where a is { }
            select new ListenerMethod(m, (AsyncEventListenerAttribute)a);

        foreach (ListenerMethod lm in ListenerMethods)
            lm.Attribute.Register(shard, lm.Method);
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