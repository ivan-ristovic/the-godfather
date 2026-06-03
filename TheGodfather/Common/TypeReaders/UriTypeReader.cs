using Discord.Commands;

namespace TheGodfather.Common.TypeReaders;

public sealed class UriTypeReader : TypeReader
{
    public override Task<TypeReaderResult> ReadAsync(ICommandContext context, string input, IServiceProvider services)
    {
        return Uri.TryCreate(input, UriKind.Absolute, out Uri? uri)
            ? Task.FromResult(TypeReaderResult.FromSuccess(uri))
            : Task.FromResult(TypeReaderResult.FromError(CommandError.ParseFailed, "Invalid URI."));
    }
}
