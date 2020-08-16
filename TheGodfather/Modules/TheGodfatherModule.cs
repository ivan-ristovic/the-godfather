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
        protected static DiscordColor EmbColor { get; private set; }


        // TODO remove dbb once all services are made
        [Obsolete]
        public DbContextBuilder Database { get; } = null!;

        public DiscordColor ModuleColor { get; }


        protected TheGodfatherModule()
        {
            var moduleAttr = Attribute.GetCustomAttribute(this.GetType(), typeof(ModuleAttribute)) as ModuleAttribute;
            this.ModuleColor = moduleAttr?.Module.ToDiscordColor() ?? DiscordColor.Green;
            EmbColor = this.ModuleColor;
        }

        // TODO remove
        [Obsolete]
        protected TheGodfatherModule(DbContextBuilder dbb)
            : this()
        {
            this.Database = dbb;
        }


        // TODO remove
        [Obsolete]
        protected Task InformAsync(CommandContext ctx, string? message = null, string? emoji = null, bool important = true)
            => this.InformAsync(ctx, (emoji is null ? Emojis.CheckMarkSuccess : DiscordEmoji.FromName(ctx.Client, emoji)), message, important);

        // TODO remove
        [Obsolete]
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

        // TODO remove
        [Obsolete]
        protected Task InformFailureAsync(CommandContext ctx, string message)
        {
            return ctx.RespondAsync(embed: new DiscordEmbedBuilder {
                Description = $"{Emojis.X} {ctx.Services.GetService<LocalizationService>().GetString(ctx.Guild?.Id ?? 0, message)}",
                Color = DiscordColor.IndianRed
            });
        }
    }
}