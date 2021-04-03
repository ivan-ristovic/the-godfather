using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using Microsoft.Extensions.DependencyInjection;
using TheGodfather.Common;
using TheGodfather.Exceptions;
using TheGodfather.Modules.Administration.Services;
using TheGodfather.Services;
using TheGodfather.Services.Common;

namespace TheGodfather.Extensions
{
    internal static class CommandContextExtensions
    {
        public static Task<DiscordMessage> RespondWithLocalizedEmbedAsync(this CommandContext ctx, Action<LocalizedEmbedBuilder> action,
                                                                          DiscordChannel? channel = null)
        {
            channel ??= ctx.Channel;
            LocalizationService lcs = ctx.Services.GetRequiredService<LocalizationService>();
            var emb = new LocalizedEmbedBuilder(lcs, ctx.Guild?.Id);
            action(emb);
            return channel.SendMessageAsync(embed: emb.Build());
        }

        public static async Task<DiscordMessage> RespondWithLocalizedEmbedAsync(this CommandContext ctx, Func<LocalizedEmbedBuilder, Task> asyncAction,
                                                                                DiscordChannel? channel = null)
        {
            channel ??= ctx.Channel;
            LocalizationService lcs = ctx.Services.GetRequiredService<LocalizationService>();
            var emb = new LocalizedEmbedBuilder(lcs, ctx.Guild?.Id);
            await asyncAction(emb);
            return await channel.SendMessageAsync(embed: emb.Build());
        }

        public static string BuildInvocationDetailsString(this CommandContext ctx, string? reason = null)
        {
            LocalizationService ls = ctx.Services.GetRequiredService<LocalizationService>();
            if (string.IsNullOrWhiteSpace(reason))
                reason = ls.GetString(ctx.Guild?.Id, "rsn-none");
            return ls.GetString(ctx.Guild?.Id, "fmt-invocation-details", ctx.User, reason, ctx.Channel);
        }

        public static Task InfoAsync(this CommandContext ctx, DiscordColor color)
            => InternalInformAsync(ctx, null, null, false, color, null);

        public static Task InfoAsync(this CommandContext ctx, string? key = null, params object?[]? args)
            => InternalInformAsync(ctx, null, key, false, null, args);

        public static Task InfoAsync(this CommandContext ctx, DiscordColor color, string key, params object?[]? args)
            => InternalInformAsync(ctx, null, key, false, color, args);

        public static Task InfoAsync(this CommandContext ctx, DiscordEmoji emoji, string key, params object?[]? args)
            => InternalInformAsync(ctx, emoji, key, false, null, args);

        public static Task InfoAsync(this CommandContext ctx, DiscordColor color, DiscordEmoji emoji, string key, params object?[]? args)
            => InternalInformAsync(ctx, emoji, key, false, color, args);

        public static Task ImpInfoAsync(this CommandContext ctx, string? key = null, params object?[]? args)
            => InternalInformAsync(ctx, null, key, true, null, args);

        public static Task ImpInfoAsync(this CommandContext ctx, DiscordColor color, string key, params object?[]? args)
            => InternalInformAsync(ctx, null, key, true, color, args);

        public static Task ImpInfoAsync(this CommandContext ctx, DiscordEmoji emoji, string key, params object?[]? args)
            => InternalInformAsync(ctx, emoji, key, true, null, args);

        public static Task ImpInfoAsync(this CommandContext ctx, DiscordColor color, DiscordEmoji emoji, string key, params object?[]? args)
            => InternalInformAsync(ctx, emoji, key, true, color, args);

        public static Task FailAsync(this CommandContext ctx, string key, params object[]? args)
        {
            return ctx.RespondAsync(embed: new DiscordEmbedBuilder {
                Description = $"{Emojis.X} {ctx.Services.GetRequiredService<LocalizationService>().GetString(ctx.Guild?.Id ?? 0, key, args)}",
                Color = DiscordColor.IndianRed
            });
        }

        public static async Task<List<string>?> WaitAndParsePollOptionsAsync(this CommandContext ctx, string separator = ";")
        {
            InteractivityService interactivity = ctx.Services.GetRequiredService<InteractivityService>();
            interactivity.AddPendingResponse(ctx.Channel.Id, ctx.User.Id);

            InteractivityResult<DiscordMessage> mctx = await ctx.Client.GetInteractivity().WaitForMessageAsync(
                xm => xm.Author == ctx.User && xm.Channel == ctx.Channel
            );

            if (!interactivity.RemovePendingResponse(ctx.Channel.Id, ctx.User.Id))
                throw new ConcurrentOperationException("Failed to remove user from pending list");

            return mctx.TimedOut
                ? null
                : mctx.Result.Content.Split(separator, StringSplitOptions.RemoveEmptyEntries)
                .Distinct()
                .ToList();
        }

        public static Task PaginateAsync<T>(this CommandContext ctx, string key, IEnumerable<T> collection,
                                            Func<T, string> selector, DiscordColor? color = null, int pageSize = 10,
                                            params object?[]? args)
        {
            T[] arr = collection.ToArray();
            LocalizationService ls = ctx.Services.GetRequiredService<LocalizationService>();

            var pages = new List<Page>();
            int pageCount = (arr.Length - 1) / pageSize + 1;
            int from = 0;
            string title = ls.GetString(ctx.Guild?.Id, key, args);
            for (int i = 1; i <= pageCount; i++) {
                int to = from + pageSize > arr.Length ? arr.Length : from + pageSize;
                pages.Add(new Page(embed: new DiscordEmbedBuilder {
                    Title = title,
                    Description = arr[from..to].Select(selector).JoinWith(),
                    Color = color ?? DiscordColor.Black,
                    Footer = new DiscordEmbedBuilder.EmbedFooter {
                        Text = ls.GetString(ctx.Guild?.Id, "fmt-page-footer", from + 1, to, arr.Length, i, pageCount),
                    }
                }));
                from += pageSize;
            }

            return pages.Count > 1
                ? ctx.Client.GetInteractivity().SendPaginatedMessageAsync(ctx.Channel, ctx.User, pages)
                : ctx.Channel.SendMessageAsync(content: pages.First().Content, embed: pages.First().Embed);
        }

        public static Task PaginateAsync<T>(this CommandContext ctx, IEnumerable<T> collection,
                                            Func<LocalizedEmbedBuilder, T, LocalizedEmbedBuilder> formatter, DiscordColor? color = null)
        {
            int count = collection.Count();
            LocalizationService ls = ctx.Services.GetRequiredService<LocalizationService>();

            IEnumerable<Page> pages = collection
                .Select((e, i) => {
                    var emb = new LocalizedEmbedBuilder(ls, ctx.Guild?.Id);
                    emb.WithLocalizedFooter("fmt-page-footer-single", null, i + 1, count);
                    emb.WithColor(color ?? DiscordColor.Black);
                    emb = formatter(emb, e);
                    return new Page { Embed = emb.Build() };
                });

            return count > 1
                ? ctx.Client.GetInteractivity().SendPaginatedMessageAsync(ctx.Channel, ctx.User, pages)
                : ctx.Channel.SendMessageAsync(content: pages.Single().Content, embed: pages.Single().Embed);
        }

        public static async Task<bool> WaitForBoolReplyAsync(this CommandContext ctx, string key, DiscordChannel? channel = null,
                                                             bool reply = true, params object[]? args)
        {
            channel ??= ctx.Channel;
            LocalizationService ls = ctx.Services.GetRequiredService<LocalizationService>();

            await ctx.RespondAsync(embed: new DiscordEmbedBuilder {
                Description = $"{Emojis.Question} {ls.GetString(ctx.Guild?.Id, key, args)} (y/n)",
                Color = DiscordColor.Yellow
            });

            if (await ctx.Client.GetInteractivity().WaitForBoolReplyAsync(ctx))
                return true;

            if (reply)
                await channel.InformFailureAsync(ls.GetString(ctx.Guild?.Id, "str-aborting"));

            return false;
        }

        public static async Task<DiscordMessage?> WaitForDmReplyAsync(this CommandContext ctx,
                                                                      DiscordDmChannel dm,
                                                                      DiscordUser user,
                                                                      TimeSpan? waitInterval = null)
        {
            InteractivityExtension interactivity = ctx.Client.GetInteractivity();
            InteractivityService interactivityService = ctx.Services.GetRequiredService<InteractivityService>();

            interactivityService.AddPendingResponse(ctx.Channel.Id, user.Id);
            InteractivityResult<DiscordMessage> mctx = await interactivity.WaitForMessageAsync(m => m.Channel == dm && m.Author == user, waitInterval);
            if (interactivityService is { } && !interactivityService.RemovePendingResponse(ctx.Channel.Id, user.Id))
                throw new ConcurrentOperationException("Failed to remove user from pending list");

            return mctx.TimedOut ? null : mctx.Result;
        }

        public static Task ExecuteOtherCommandAsync(this CommandContext ctx, string command, params string?[] args)
        {
            string callStr = $"{command} {args.JoinWith(" ")}";
            Command cmd = ctx.CommandsNext.FindCommand(callStr, out string actualArgs);
            CommandContext fctx = ctx.CommandsNext.CreateFakeContext(ctx.User, ctx.Channel, callStr, ctx.Prefix, cmd, actualArgs);
            return ctx.CommandsNext.ExecuteCommandAsync(fctx);
        }


        private static async Task InternalInformAsync(this CommandContext ctx, DiscordEmoji? emoji = null, string? key = null,
                                                      bool important = true, DiscordColor? color = null, params object?[]? args)
        {
            emoji ??= Emojis.CheckMarkSuccess;
            ulong gid = ctx.Guild?.Id ?? 0;
            if (!important && (ctx.Services.GetRequiredService<GuildConfigService>().GetCachedConfig(gid)?.ReactionResponse ?? false)) {
                try {
                    await ctx.Message.CreateReactionAsync(Emojis.CheckMarkSuccess);
                } catch (NotFoundException) {
                    await InfoAsync(ctx, emoji, "str-done");
                }
            } else {
                LocalizationService ls = ctx.Services.GetRequiredService<LocalizationService>();
                string response = ls.GetString(gid, string.IsNullOrWhiteSpace(key) ? "str-done" : key, args);
                await ctx.RespondAsync(embed: new DiscordEmbedBuilder {
                    Description = $"{emoji} {response}",
                    Color = color ?? DiscordColor.Green,
                });
            }
        }

    }
}
