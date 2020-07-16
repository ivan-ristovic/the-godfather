using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using TheGodfather.Attributes;
using TheGodfather.EventListeners.Common;
using TheGodfather.Extensions;
using TheGodfather.Modules.Administration.Extensions;
using TheGodfather.Services;

namespace TheGodfather.Modules.Administration.Services
{
    public sealed class DiscordLogEmbedBuilder
    {
        public DiscordEmbedBuilder Builder { get; private set; }


        public DiscordLogEmbedBuilder(string title, string desc = null, DiscordEventType? eventType = null)
        {
            this.Builder = new DiscordEmbedBuilder()
                .WithTitle(title);
            if (!string.IsNullOrWhiteSpace(desc))
                this.Builder = this.Builder.WithDescription(desc);
            if (eventType.HasValue)
                this.WithColor(eventType.Value.ToDiscordColor());
        }

        public DiscordLogEmbedBuilder(CommandContext ctx, string title, string desc = null, DiscordEventType? eventType = null)
            : this(title, desc, eventType)
        {
            this.AddInvocationFields(ctx);
        }


        public DiscordLogEmbedBuilder AddField(string title, string text, string altText = "Unknown", bool inline = false)
        {
            if (string.IsNullOrWhiteSpace(text)) {
                if (!string.IsNullOrWhiteSpace(altText))
                    this.Builder = this.Builder.AddField(title, altText, inline);
            } else {
                this.Builder = this.Builder.AddField(title, text, inline);
            }
            return this;
        }

        public DiscordLogEmbedBuilder AddField(string title, IEnumerable<object> collection, bool inline = false, string sep = "\n")
        {
            if (collection.Any())
                this.Builder = this.Builder.AddField(title, string.Join(sep, collection.Select(r => r.ToString())), inline);
            return this;
        }

        public DiscordLogEmbedBuilder AddInvocationFields(CommandContext ctx)
            => this.AddInvocationFields(ctx.User, ctx.Channel);

        public DiscordLogEmbedBuilder AddInvocationFields(DiscordUser user, DiscordChannel channel = null)
        {
            this.Builder = this.Builder.AddField("User responsible", user?.Mention ?? "Unknown", inline: true);

            if (!(channel is null))
                this.Builder = this.Builder.AddField("Invoked in", channel.Mention, inline: true);

            return this;
        }

        public DiscordLogEmbedBuilder AddPropertyChangeField<T>(string title, PropertyChange<T> propertyChange, bool inline = true)
        {
            if (!(propertyChange is null)) {
                string before = Formatter.InlineCode(propertyChange.Before?.ToString() ?? "Unknown");
                string after = Formatter.InlineCode(propertyChange.After?.ToString() ?? "Unknown");
                this.Builder = this.Builder.AddField(title, $"{before} => {after}", inline);
            }

            return this;
        }

        public DiscordLogEmbedBuilder AddPropertyChangeField<T>(string title, T before, T after, bool inline = true)
        {
            if (!before.Equals(after)) {
                string beforeStr = Formatter.InlineCode(before?.ToString() ?? "Unknown");
                string afterStr = Formatter.InlineCode(after?.ToString() ?? "Unknown");
                this.Builder = this.Builder.AddField(title, $"{beforeStr} => {afterStr}", inline);
            }

            return this;
        }

        public DiscordLogEmbedBuilder WithColor(DiscordColor color)
        {
            this.Builder = this.Builder.WithColor(color);
            return this;
        }

        public DiscordLogEmbedBuilder WithDescription(string desc, string alt = "Unknown")
        {
            if (string.IsNullOrWhiteSpace(desc)) {
                if (!string.IsNullOrWhiteSpace(alt))
                    this.Builder = this.Builder.WithDescription(alt);
            } else {
                this.Builder = this.Builder.WithDescription(desc);
            }
            return this;
        }

        public DiscordLogEmbedBuilder WithTimestampFooter(DateTimeOffset timestamp, string iconUrl = null)
        {
            this.Builder = this.Builder.WithFooter(timestamp.ToUtcTimestamp(), iconUrl);
            return this;
        }

        public DiscordLogEmbedBuilder WithThumbnailUrl(string url)
        {
            this.Builder = this.Builder.WithThumbnailUrl(url);
            return this;
        }

        public DiscordLogEmbedBuilder WithTitle(string title, string alt = null)
        {
            if (string.IsNullOrWhiteSpace(title)) {
                if (!string.IsNullOrWhiteSpace(alt))
                    this.Builder = this.Builder.WithTitle(alt);
            } else {
                this.Builder = this.Builder.WithTitle(title);
            }
            return this;
        }

        public DiscordEmbed Build()
            => this.Builder.Build();
    }


    public sealed class LoggingService : ITheGodfatherService
    {
        public bool IsDisabled => false;

        private readonly GuildConfigService gcs;


        public LoggingService(GuildConfigService gcs)
        {
            this.gcs = gcs;
        }


        public Task LogAsync(DiscordGuild guild, DiscordLogEmbedBuilder embed)
        {
            DiscordChannel logchn = this.gcs.GetLogChannelForGuild(guild);
            return logchn is null ? Task.CompletedTask : logchn.SendMessageAsync(embed: embed.Build());
        }
    }
}
