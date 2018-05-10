#region USING_DIRECTIVES
using System;
using System.Threading.Tasks;

using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Converters;
using DSharpPlus.Entities;
#endregion

namespace TheGodfather.Common.Converters
{
    public class CustomUriConverter : IArgumentConverter<Uri>
    {
        public async Task<Optional<Uri>> ConvertAsync(string value, CommandContext ctx)
        {
            await Task.Delay(0);

            if (!Uri.TryCreate(value, UriKind.Absolute, out Uri uri))
                return new Optional<Uri>();
            if (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps)
                return new Optional<Uri>();
            return new Optional<Uri>(uri);
        }
    }
}