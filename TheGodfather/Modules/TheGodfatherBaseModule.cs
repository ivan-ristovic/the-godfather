#region USING_DIRECTIVES
using System;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using TheGodfather.Common;
using TheGodfather.Services;

using DSharpPlus;
using DSharpPlus.CommandsNext;
#endregion

namespace TheGodfather.Modules
{
    public abstract class TheGodfatherBaseModule : BaseCommandModule
    {
        private static HttpClientHandler _handler = new HttpClientHandler { AllowAutoRedirect = false };
        protected static HttpClient HTTPClient { get; } = new HttpClient(_handler, true);

        protected SharedData Shared { get; }
        protected DBService Database { get; }


        protected TheGodfatherBaseModule(SharedData shared = null, DBService db = null)
        {
            Shared = shared;
            Database = db;
        }
        

        protected async Task<bool> IsValidImageUriAsync(Uri uri)
        {
            try {
                var response = await HTTPClient.GetAsync(uri)
                    .ConfigureAwait(false);
                if (!response.Content.Headers.ContentType.MediaType.StartsWith("image/"))
                    return false;
            } catch (Exception e) {
                TheGodfather.LogHandle.LogException(LogLevel.Debug, e);
                return false;
            }

            return true;
        }

        protected bool IsValidRegex(string pattern)
        {
            if (string.IsNullOrEmpty(pattern))
                return false;

            try {
                Regex.Match("", pattern);
            } catch (ArgumentException) {
                return false;
            }

            return true;
        }

        protected bool TryParseRegex(string pattern, out Regex result)
        {
            result = null;
            if (!IsValidRegex(pattern))
                return false;

            result = new Regex($@"\b{pattern}\b", RegexOptions.IgnoreCase);
            return true;
        }
    }
}
