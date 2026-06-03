using System.Net;
using Discord.Commands;

namespace TheGodfather.Common.TypeReaders;

public sealed class IPAddressTypeReader : TypeReader
{
    public override Task<TypeReaderResult> ReadAsync(ICommandContext context, string input, IServiceProvider services)
    {
        return IPAddress.TryParse(input, out IPAddress? address)
            ? Task.FromResult(TypeReaderResult.FromSuccess(address))
            : Task.FromResult(TypeReaderResult.FromError(CommandError.ParseFailed, "Invalid IP address."));
    }
}
