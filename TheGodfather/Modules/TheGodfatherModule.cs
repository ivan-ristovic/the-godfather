using System;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;
using Microsoft.Extensions.DependencyInjection;
using TheGodfather.Attributes;
using TheGodfather.Common;
using TheGodfather.Database;
using TheGodfather.Modules.Administration.Services;
using TheGodfather.Services;

namespace TheGodfather.Modules
{
    public abstract class TheGodfatherModule : BaseCommandModule
    {
        // TODO remove dbb once all services are made
        public DbContextBuilder Database { get; }

        public DiscordColor ModuleColor { get; }


        // TODO remove dbb once all services are made
        protected TheGodfatherModule(DbContextBuilder dbb)
        {
            this.Database = dbb;
            var moduleAttr = Attribute.GetCustomAttribute(this.GetType(), typeof(ModuleAttribute)) as ModuleAttribute;
            this.ModuleColor = moduleAttr?.Module.ToDiscordColor() ?? DiscordColor.Green;
        }


        protected Task InformAsync(CommandContext ctx, string? message = null, string? emoji = null, bool important = true)
            => this.InformAsync(ctx, (emoji is null ? Emojis.CheckMarkSuccess : DiscordEmoji.FromName(ctx.Client, emoji)), message, important);

        protected async Task InformAsync(CommandContext ctx, DiscordEmoji emoji, string? message = null, bool important = true)
        {
            ulong gid = ctx.Guild?.Id ?? 0;
            if (!important && (ctx.Services.GetService<GuildConfigService>().GetCachedConfig(gid)?.ReactionResponse ?? false)) {
                try {
                    await ctx.Message.CreateReactionAsync(Emojis.CheckMarkSuccess);
                } catch (NotFoundException) {
                    await this.InformAsync(ctx, ctx.Services.GetService<LocalizationService>().GetString(gid, "Action completed"));
                }
            } else {
                string response = message is null ? "Done!" : ctx.Services.GetService<LocalizationService>().GetString(gid, message);
                await ctx.RespondAsync(embed: new DiscordEmbedBuilder {
                    Description = $"{emoji ?? Emojis.CheckMarkSuccess} {response}",
                    Color = this.ModuleColor
                });
            }
        }

        protected Task InformFailureAsync(CommandContext ctx, string message)
        {
            return ctx.RespondAsync(embed: new DiscordEmbedBuilder {
                Description = $"{Emojis.X} {ctx.Services.GetService<LocalizationService>().GetString(ctx.Guild?.Id ?? 0, message)}",
                Color = DiscordColor.IndianRed
            });
        }

        protected Task LogAsync(CommandContext ctx, DiscordLogEmbedBuilder emb) 
            => ctx.Services.GetService<LoggingService>().LogAsync(ctx.Guild, emb.WithColor(this.ModuleColor));
    }
}