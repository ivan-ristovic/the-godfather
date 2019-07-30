#region USING_DIRECTIVES
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;

using Microsoft.Extensions.DependencyInjection;

using System;
using System.Net.Http;
using System.Threading.Tasks;

using TheGodfather.Common;
using TheGodfather.Common.Attributes;
using TheGodfather.Database;
using TheGodfather.Modules.Administration.Services;
using TheGodfather.Services;
#endregion

namespace TheGodfather.Modules
{
    public abstract class TheGodfatherModule : BaseCommandModule
    {
        protected static readonly HttpClient _http;
        private static readonly HttpClientHandler _handler;

        public SharedData Shared { get; }
        public DatabaseContextBuilder Database { get; }
        public DiscordColor ModuleColor { get; }


        static TheGodfatherModule()
        {
            _handler = new HttpClientHandler {
                AllowAutoRedirect = false
            };
            _http = new HttpClient(_handler, true);
        }


        protected TheGodfatherModule(SharedData shared, DatabaseContextBuilder dbb)
        {
            this.Shared = shared;
            this.Database = dbb;
            var moduleAttr = Attribute.GetCustomAttribute(this.GetType(), typeof(ModuleAttribute)) as ModuleAttribute;
            this.ModuleColor = moduleAttr is null ? DiscordColor.Green : moduleAttr.Module.ToDiscordColor();
        }


        protected Task InformAsync(CommandContext ctx, string message = null, string emoji = null, bool important = true)
            => this.InformAsync(ctx, (emoji is null ? StaticDiscordEmoji.CheckMarkSuccess : DiscordEmoji.FromName(ctx.Client, emoji)), message, important);

        protected async Task InformAsync(CommandContext ctx, DiscordEmoji emoji, string message = null, bool important = true)
        {
            if (!important && ctx.Services.GetService<GuildConfigService>().GetCachedConfig(ctx.Guild.Id).ReactionResponse) {
                try {
                    await ctx.Message.CreateReactionAsync(StaticDiscordEmoji.CheckMarkSuccess);
                } catch (NotFoundException) {
                    await this.InformAsync(ctx, "Action completed!");
                }
            } else {
                string response = message is null ? "Done!" : ctx.Services.GetService<LocalizationService>().GetString(ctx.Guild.Id, message);
                await ctx.RespondAsync(embed: new DiscordEmbedBuilder {
                    Description = $"{emoji ?? StaticDiscordEmoji.CheckMarkSuccess} {response}",
                    Color = this.ModuleColor
                });
            }
        }

        protected Task InformFailureAsync(CommandContext ctx, string message)
        {
            return ctx.RespondAsync(embed: new DiscordEmbedBuilder {
                Description = $"{StaticDiscordEmoji.X} {message}",
                Color = DiscordColor.IndianRed
            });
        }

        protected Task LogAsync(CommandContext ctx, DiscordLogEmbedBuilder emb)
        {
            return ctx.Services.GetService<LoggingService>().LogAsync(ctx.Guild, emb
                .WithColor(this.ModuleColor)
            );
        }

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