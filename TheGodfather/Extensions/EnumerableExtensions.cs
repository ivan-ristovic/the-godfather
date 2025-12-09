namespace TheGodfather.Extensions;

internal static class EnumerableExtensions
{
    public static string JoinWith<T>(this IEnumerable<T> source, string separator = "\n")
        => string.Join(separator, source.Select(e => e?.ToString() ?? ""));
}