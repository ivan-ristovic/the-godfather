#region USING_DIRECTIVES
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Converters;
using DSharpPlus.Entities;
using System;
using System.Threading.Tasks;
#endregion

namespace TheGodfather.Common.Converters
{
    public class CustomUriConverter : IArgumentConverter<Uri>
    {
        public Task<Optional<Uri>> ConvertAsync(string value, CommandContext ctx)
        {
            if (!Uri.TryCreate(value, UriKind.Absolute, out Uri uri))
                return Task.FromResult(new Optional<Uri>());
            if (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps)
                return Task.FromResult(new Optional<Uri>());
            return Task.FromResult(new Optional<Uri>(uri));
        }
    }
}