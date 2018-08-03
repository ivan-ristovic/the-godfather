#region USING_DIRECTIVES
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using TheGodfather.Extensions;
using TheGodfather.Services.Database;
#endregion

namespace TheGodfather.Modules
{
    public abstract class TheGodfatherModule : BaseCommandModule
    {
        private static readonly HttpClientHandler _handler = new HttpClientHandler { AllowAutoRedirect = false };
        protected static readonly HttpClient _http = new HttpClient(_handler, true);

        protected SharedData Shared { get; }
        protected DBService Database { get; }
        protected DiscordColor ModuleColor { get; set; }


        protected TheGodfatherModule(SharedData shared = null, DBService db = null)
        {
            this.Shared = shared;
            this.Database = db;
            this.ModuleColor = DiscordColor.Green;
        }


        protected Task InformAsync(CommandContext ctx, string message = null, string icon_emoji = null, bool important = false)
            => ctx.EmbedAsync(this.Shared, message, important, icon_emoji, this.ModuleColor);

        protected Task InformAsync(CommandContext ctx, DiscordEmoji emoji, string message = null, bool important = false)
            => ctx.EmbedAsync(this.Shared, emoji, message, important, this.ModuleColor);

        protected async Task<bool> IsValidImageUriAsync(Uri uri)
        {
            try {
                HttpResponseMessage response = await _http.GetAsync(uri).ConfigureAwait(false);
                if (response.Content.Headers.ContentType.MediaType.StartsWith("image/"))
                    return true;
            } catch {

            }

            return false;
        }
    }
}
