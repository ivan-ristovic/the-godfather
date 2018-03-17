#region USING_DIRECTIVES
using System;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;

using TheGodfather.Entities;
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


        protected bool IsValidURL(string url, out Uri uri)
        {
            uri = null;
            if (!Uri.TryCreate(url, UriKind.Absolute, out uri))
                return false;
            if (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps)
                return false;
            return true;
        }

        protected bool IsValidImageURL(string url, out Uri uri)
        {
            if (!IsValidURL(url, out uri))
                return false;

            try {
                if (WebRequest.Create(uri) is HttpWebRequest request) {
                    string contentType = "";
                    if (request.GetResponse() is HttpWebResponse response)
                        contentType = response.ContentType;
                    if (!contentType.StartsWith("image/"))
                        return false;
                } else {
                    return false;
                }
            } catch (Exception e) {
                Logger.LogException(LogLevel.Debug, e);
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
    }
}
