#region USING_DIRECTIVES
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Converters;
#endregion

namespace TheGodfather.Extensions
{
    internal static class CNextExtensions
    {
        public static void RegisterConverters(this CommandsNextExtension cnext, Assembly? assembly = null)
        {
            assembly ??= Assembly.GetExecutingAssembly();

            Type iargc = typeof(IArgumentConverter);
            IEnumerable<Type> cs = assembly
                .GetTypes()
                .Where(t => iargc.IsAssignableFrom(t) && !t.IsAbstract)
                ;
            foreach (Type c in cs) {
                object? instance = Activator.CreateInstance(c);
                if (instance is { })
                    cnext.RegisterConverter((dynamic)instance);
            }
        }

        public static IReadOnlyList<Command> GetAllRegisteredCommands(this CommandsNextExtension cnext)
        {
            return cnext.RegisteredCommands
                .SelectMany(cnext.CommandSelector)
                .Distinct()
                .ToList()
                .AsReadOnly();
        }

        public static IEnumerable<Command> CommandSelector(this CommandsNextExtension cnext, KeyValuePair<string, Command> c)
            => cnext.CommandSelector(c.Value);

        public static IEnumerable<Command> CommandSelector(this CommandsNextExtension cnext, Command c)
        {
            Command[] arr = new[] { c };

            if (c is CommandGroup group)
                return arr.Concat(group.Children.SelectMany(cnext.CommandSelector));

            return arr;
        }
    }
}
