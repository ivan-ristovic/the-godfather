#region USING_DIRECTIVES
using System.Collections.Generic;
using System.Linq;
using DSharpPlus.CommandsNext;
#endregion

namespace TheGodfather.Extensions
{
    internal static class CNextExtensions
    {
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
