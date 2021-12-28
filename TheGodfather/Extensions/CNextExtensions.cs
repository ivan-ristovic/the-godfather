using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Converters;

namespace TheGodfather.Extensions;

internal static class CNextExtensions
{
    public static void RegisterConverters(this CommandsNextExtension cnext, Assembly? assembly = null)
    {
        assembly ??= Assembly.GetExecutingAssembly();

        Type argConvType = typeof(IArgumentConverter);
        IEnumerable<Type> converterTypes = assembly
                .GetTypes()
                .Where(t => argConvType.IsAssignableFrom(t) && !t.IsAbstract && !t.IsInterface)
            ;

        foreach (Type converterType in converterTypes)
            try {
                object? converterInstance = Activator.CreateInstance(converterType);
                if (converterInstance is { }) {
                    cnext.RegisterConverter((dynamic)converterInstance);
                    Log.Debug("Registered converter: {Converter}", converterType.Name);
                }
            } catch {
                Log.Error("Failed to register converter: {Converter}", converterType.Name);
            }
    }

    public static IReadOnlyList<Command> GetRegisteredCommands(this CommandsNextExtension cnext)
    {
        return cnext.RegisteredCommands
            .SelectMany(cnext.CommandSelector)
            .Distinct()
            .ToList()
            .AsReadOnly();
    }


    private static IEnumerable<Command> CommandSelector(this CommandsNextExtension cnext, KeyValuePair<string, Command> c)
        => cnext.CommandSelector(c.Value);

    private static IEnumerable<Command> CommandSelector(this CommandsNextExtension cnext, Command c)
    {
        Command[] arr = { c };
        return c is CommandGroup group
            ? arr.Concat(group.Children.SelectMany(cnext.CommandSelector))
            : arr;
    }
}

public sealed class CommandKeyValuePairComparer : IEqualityComparer<KeyValuePair<string, Command>>
{
    public bool Equals([AllowNull] KeyValuePair<string, Command> x, [AllowNull] KeyValuePair<string, Command> y)
        => Equals(x.Value?.QualifiedName, y.Value?.QualifiedName);

    public int GetHashCode([DisallowNull] KeyValuePair<string, Command> obj)
        => obj.Value.QualifiedName.GetHashCode();
}