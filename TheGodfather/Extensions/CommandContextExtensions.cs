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
using TheGodfather.Translations;

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
                reason = ls.GetString(ctx.Guild?.Id, TranslationKey.rsn_none);
            return ls.GetString(ctx.Guild?.Id,  TranslationKey.fmt_invocation_details(ctx.User, reason, ctx.Channel));
        }

        public static Task InfoAsync(this CommandContext ctx, DiscordColor color)
            => InternalInformAsync(ctx, null, null, false, color);

        public static Task InfoAsync(this CommandContext ctx, TranslationKey? key = null)
            => InternalInformAsync(ctx, null, key, false, null);

        public static Task InfoAsync(this CommandContext ctx, DiscordColor color, TranslationKey key)
            => InternalInformAsync(ctx, null, key, false, color);

        public static Task InfoAsync(this CommandContext ctx, DiscordEmoji emoji, TranslationKey key)
            => InternalInformAsync(ctx, emoji, key, false, null);

        public static Task InfoAsync(this CommandContext ctx, DiscordColor color, DiscordEmoji emoji, TranslationKey key)
            => InternalInformAsync(ctx, emoji, key, false, color);

        public static Task ImpInfoAsync(this CommandContext ctx, TranslationKey? key = null)
            => InternalInformAsync(ctx, null, key, true, null);

        public static Task ImpInfoAsync(this CommandContext ctx, DiscordColor color, TranslationKey key)
            => InternalInformAsync(ctx, null, key, true, color);

        public static Task ImpInfoAsync(this CommandContext ctx, DiscordEmoji emoji, TranslationKey key)
            => InternalInformAsync(ctx, emoji, key, true, null);

        public static Task ImpInfoAsync(this CommandContext ctx, DiscordColor color, DiscordEmoji emoji, TranslationKey key)
            => InternalInformAsync(ctx, emoji, key, true, color);

        public static Task FailAsync(this CommandContext ctx, TranslationKey key)
        {
            return ctx.RespondAsync(embed: new DiscordEmbedBuilder {
                Description = $"{Emojis.X} {ctx.Services.GetRequiredService<LocalizationService>().GetString(ctx.Guild?.Id ?? 0, key)}",
                Color = DiscordColor.IndianRed
            });
        }

        public static Task PaginateAsync<T>(this CommandContext ctx, TranslationKey key, IEnumerable<T> collection,
                                            Func<T, string> selector, DiscordColor? color = null, int pageSize = 10)
        {
            T[] arr = collection.ToArray();
            LocalizationService ls = ctx.Services.GetRequiredService<LocalizationService>();

            var pages = new List<Page>();
            int pageCount = (arr.Length - 1) / pageSize + 1;
            int from = 0;
            string title = ls.GetString(ctx.Guild?.Id, key);
            for (int i = 1; i <= pageCount; i++) {
                int to = from + pageSize > arr.Length ? arr.Length : from + pageSize;
                pages.Add(new Page(embed: new DiscordEmbedBuilder {
                    Title = title,
                    Description = arr[from..to].Select(selector).JoinWith(),
                    Color = color ?? DiscordColor.Black,
                    Footer = new DiscordEmbedBuilder.EmbedFooter {
                        Text = ls.GetString(ctx.Guild?.Id, TranslationKey.fmt_page_footer(from + 1, to, arr.Length, i, pageCount)),
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
                    emb.WithLocalizedFooter(TranslationKey.fmt_page_footer_single(i + 1, count), null);
                    emb.WithColor(color ?? DiscordColor.Black);
                    emb = formatter(emb, e);
                    return new Page { Embed = emb.Build() };
                });

            return count > 1
                ? ctx.Client.GetInteractivity().SendPaginatedMessageAsync(ctx.Channel, ctx.User, pages)
                : ctx.Channel.SendMessageAsync(content: pages.Single().Content, embed: pages.Single().Embed);
        }

        public static async Task<bool> WaitForBoolReplyAsync(this CommandContext ctx, TranslationKey key, DiscordChannel? channel = null, bool reply = true)
        {
            channel ??= ctx.Channel;
            LocalizationService ls = ctx.Services.GetRequiredService<LocalizationService>();

            await ctx.RespondAsync(embed: new DiscordEmbedBuilder {
                Description = $"{Emojis.Question} {ls.GetString(ctx.Guild?.Id, key)} (y/n)",
                Color = DiscordColor.Yellow
            });

            if (await ctx.Client.GetInteractivity().WaitForBoolReplyAsync(ctx))
                return true;

            if (reply)
                await channel.InformFailureAsync(ls.GetString(ctx.Guild?.Id, TranslationKey.str_aborting));

            return false;
        }

        public static async Task<DiscordMessage?> WaitForDmReplyAsync(this CommandContext ctx, DiscordDmChannel dm, DiscordUser user, TimeSpan? waitInterval = null)
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


        private static async Task InternalInformAsync(this CommandContext ctx, DiscordEmoji? emoji = null, TranslationKey? key = null,
                                                      bool important = true, DiscordColor? color = null)
        {
            emoji ??= Emojis.CheckMarkSuccess;
            ulong gid = ctx.Guild?.Id ?? 0;
            if (!important && (ctx.Services.GetRequiredService<GuildConfigService>().GetCachedConfig(gid)?.ReactionResponse ?? false)) {
                try {
                    await ctx.Message.CreateReactionAsync(Emojis.CheckMarkSuccess);
                } catch (NotFoundException) {
                    await ImpInfoAsync(ctx, emoji, TranslationKey.str_done);
                }
            } else {
                LocalizationService ls = ctx.Services.GetRequiredService<LocalizationService>();
                string response = ls.GetString(gid, key ?? TranslationKey.str_done);
                await ctx.RespondAsync(embed: new DiscordEmbedBuilder {
                    Description = $"{emoji} {response}",
                    Color = color ?? DiscordColor.Green,
                });
            }
        }
    }
}
