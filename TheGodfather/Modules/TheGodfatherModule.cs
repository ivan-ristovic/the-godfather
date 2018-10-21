#region USING_DIRECTIVES
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;
using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

using TheGodfather.Common;
using TheGodfather.Database;
using TheGodfather.Database.Entities;
using TheGodfather.Services;
#endregion

namespace TheGodfather.Modules
{
    public abstract class TheGodfatherModule : BaseCommandModule
    {
        protected static readonly HttpClient _http;
        private static readonly HttpClientHandler _handler;

        protected SharedData Shared { get; }
        protected DBService Database { get; }
        protected DatabaseContextBuilder DatabaseBuilder { get; }
        protected DiscordColor ModuleColor {
            get { return this.moduleColor ?? DiscordColor.Green; }
            set { this.moduleColor = value; }
        }

        private DiscordColor? moduleColor;


        static TheGodfatherModule()
        {
            _handler = new HttpClientHandler {
                AllowAutoRedirect = false
            };
            _http = new HttpClient(_handler, true);
        }

        protected TheGodfatherModule(SharedData shared, DBService db = null)
        {
            this.Shared = shared;
            this.Database = db;
            this.ModuleColor = DiscordColor.Green;
            this.DatabaseBuilder = new DatabaseContextBuilder(shared.BotConfiguration.DatabaseConfig);
        }


        protected Task InformAsync(CommandContext ctx, string message = null, string emoji = null, bool important = true)
            => this.InformAsync(ctx, (emoji is null ? StaticDiscordEmoji.CheckMarkSuccess : DiscordEmoji.FromName(ctx.Client, emoji)), message, important);

        protected async Task InformAsync(CommandContext ctx, DiscordEmoji emoji, string message = null, bool important = true)
        {
            if (!important && this.Shared.GetGuildConfig(ctx.Guild.Id).ReactionResponse) {
                try {
                    await ctx.Message.CreateReactionAsync(StaticDiscordEmoji.CheckMarkSuccess);
                } catch (NotFoundException) {
                    await this.InformAsync(ctx, "Action completed!");
                }
            } else {
                await ctx.RespondAsync(embed: new DiscordEmbedBuilder {
                    Description = $"{(emoji ?? StaticDiscordEmoji.CheckMarkSuccess)} {message ?? "Done!"}",
                    Color = this.ModuleColor
                });
            }
        }

        protected Task InformFailureAsync(CommandContext ctx, string message)
        {
            return ctx.RespondAsync(embed: new DiscordEmbedBuilder {
                Description = $"{StaticDiscordEmoji.BoardPieceX} {message}",
                Color = DiscordColor.IndianRed
            });
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

        public async Task<DatabaseGuildConfig> GetGuildConfig(ulong gid)
        {
            DatabaseGuildConfig gcfg = null;
            using (DatabaseContext db = this.DatabaseBuilder.CreateContext())
                gcfg = await db.GuildConfig.FindAsync(gid) ?? new DatabaseGuildConfig();
            return gcfg;
        }

        public async Task<DatabaseGuildConfig> ModifyGuildConfigAsync(ulong gid, Action<DatabaseGuildConfig> action)
        {
            DatabaseGuildConfig gcfg = null;
            using (DatabaseContext db = this.DatabaseBuilder.CreateContext()) {
                gcfg = await db.GuildConfig.FindAsync(gid) ?? new DatabaseGuildConfig();
                action(gcfg);
                await db.SaveChangesAsync();
            }

            CachedGuildConfig cgcfg = this.Shared.GetGuildConfig(gid);
            cgcfg = gcfg.CachedConfig;

            return gcfg;
        }
    }
}
