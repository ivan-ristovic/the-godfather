using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using Microsoft.Extensions.DependencyInjection;
using TheGodfather.Attributes;
using TheGodfather.Database.Models;
using TheGodfather.Exceptions;
using TheGodfather.Extensions;
using TheGodfather.Modules.Administration.Extensions;
using TheGodfather.Modules.Administration.Services;
using TheGodfather.Modules.Reactions.Services;

namespace TheGodfather.Modules.Reactions
{
    [Group("textreaction"), Module(ModuleType.Reactions), NotBlocked]
    [Aliases("treact", "tr", "txtr", "textreactions")]
    [RequireGuild, RequireUserPermissions(Permissions.ManageGuild)]
    [Cooldown(3, 5, CooldownBucketType.Guild)]
    public sealed class TextReactionsModule : TheGodfatherServiceModule<ReactionsService>
    {
        #region textreactions
        [GroupCommand, Priority(1)]
        public Task ExecuteGroupAsync(CommandContext ctx)
            => this.ListAsync(ctx);

        [GroupCommand, Priority(0)]
        public Task ExecuteGroupAsync(CommandContext ctx,
                                     [Description("desc-trigger")] string trigger,
                                     [RemainingText, Description("desc-response")] string response)
            => this.AddAsync(ctx, trigger, response);
        #endregion

        #region textreactions add
        [Command("add")]
        [Aliases("register", "reg", "new", "a", "+", "+=", "<<", "<", "<-", "<=")]
        public Task AddAsync(CommandContext ctx,
                            [Description("desc-trigger")] string trigger,
                            [RemainingText, Description("desc-response")] string response)
            => this.AddTextReactionAsync(ctx, trigger, response, false);
        #endregion

        #region textreactions addregex
        [Command("addregex")]
        [Aliases("registerregex", "regex", "newregex", "ar", "+r", "+=r", "<<r", "<r", "<-r", "<=r", "+regex", "+regexp", "+rgx")]
        public Task AddRegexAsync(CommandContext ctx,
                                 [Description("desc-trigger")] string trigger,
                                 [RemainingText, Description("desc-response")] string response)
            => this.AddTextReactionAsync(ctx, trigger, response, true);
        #endregion

        #region textreactions delete
        [Command("delete"), Priority(1)]
        [Aliases("unregister", "remove", "rm", "del", "d", "-", "-=", ">", ">>", "->", "=>")]
        public async Task DeleteAsync(CommandContext ctx,
                                     [Description("desc-r-del-ids")] params int[] ids)
        {
            if (ids is null || !ids.Any())
                throw new InvalidCommandUsageException(ctx, "cmd-err-ids-none");

            if (!this.Service.GetGuildTextReactions(ctx.Guild.Id).Any())
                throw new CommandFailedException(ctx, "cmd-err-tr-none");

            int removed = await this.Service.RemoveTextReactionsAsync(ctx.Guild.Id, ids);

            await ctx.ImpInfoAsync(this.ModuleColor, "fmt-tr-del", removed);

            if (removed > 0) {
                await ctx.GuildLogAsync(emb => {
                    emb.WithLocalizedTitle("evt-tr-del");
                    emb.WithColor(this.ModuleColor);
                    emb.AddLocalizedTitleField("str-ids", ids.JoinWith(", "), inline: true);
                    emb.AddLocalizedTitleField("str-count", removed, inline: true);
                });
            }
        }

        [Command("delete"), Priority(0)]
        public async Task DeleteAsync(CommandContext ctx,
                                     [RemainingText, Description("desc-triggers")] params string[] triggers)
        {
            if (triggers is null || !triggers.Any())
                throw new InvalidCommandUsageException(ctx, "cmd-err-trig-none");

            IReadOnlyCollection<TextReaction> ers = this.Service.GetGuildTextReactions(ctx.Guild.Id);
            if (!ers.Any())
                throw new CommandFailedException(ctx, "cmd-err-tr-none");

            var eb = new StringBuilder();
            var validTriggers = new HashSet<string>();
            var foundReactions = new HashSet<TextReaction>();
            foreach (string trigger in triggers.Select(t => t.ToLowerInvariant()).Distinct()) {
                if (!trigger.TryParseRegex(out _)) {
                    eb.AppendLine(this.Localization.GetString(ctx.Guild.Id, "cmd-err-trig-regex", trigger));
                    continue;
                }

                IEnumerable<TextReaction> found = ers.Where(er => er.ContainsTriggerPattern(trigger));
                if (!found.Any()) {
                    eb.AppendLine(this.Localization.GetString(ctx.Guild.Id, "cmd-err-trig-404", trigger));
                    continue;
                }

                validTriggers.Add(trigger);
                foreach (TextReaction er in found)
                    foundReactions.Add(er);
            }

            int removed = await this.Service.RemoveTextReactionTriggersAsync(ctx.Guild.Id, foundReactions, validTriggers);

            if (eb.Length > 0)
                await ctx.ImpInfoAsync(this.ModuleColor, "fmt-action-err", eb);
            else
                await ctx.ImpInfoAsync(this.ModuleColor, "fmt-tr-del", removed);

            await ctx.GuildLogAsync(emb => {
                emb.WithLocalizedTitle("evt-tr-del");
                emb.WithColor(this.ModuleColor);
                emb.AddLocalizedTitleField("str-triggers", validTriggers.JoinWith(), inline: true);
                emb.AddLocalizedTitleField("str-count", removed, inline: true);
            });
        }
        #endregion

        #region textreactions deleteall
        [Command("deleteall"), UsesInteractivity]
        [Aliases("removeall", "rmrf", "rma", "clearall", "clear", "delall", "da", "cl", "-a", "--", ">>>")]
        [RequireUserPermissions(Permissions.Administrator)]
        public async Task DeleteAllAsync(CommandContext ctx)
        {
            if (!this.Service.GetGuildTextReactions(ctx.Guild.Id).Any())
                throw new CommandFailedException(ctx, "cmd-err-tr-none");

            if (!await ctx.WaitForBoolReplyAsync("q-tr-rem-all"))
                return;

            int removed = await this.Service.RemoveTextReactionsAsync(ctx.Guild.Id);

            await ctx.ImpInfoAsync(this.ModuleColor, "fmt-tr-del", removed);

            if (removed > 0) {
                await ctx.GuildLogAsync(emb => {
                    emb.WithLocalizedTitle("evt-tr-del");
                    emb.WithColor(this.ModuleColor);
                    emb.AddLocalizedTitleField("str-count", removed, inline: true);
                });
            }
        }
        #endregion

        #region textreactions find
        [Command("find")]
        [Aliases("f", "test")]
        public Task FindAsync(CommandContext ctx,
                             [RemainingText, Description("desc-trigger")] string trigger)
        {
            TextReaction? tr = this.Service.FindMatchingTextReaction(ctx.Guild.Id, trigger);
            if (tr is null)
                throw new CommandFailedException(ctx, "cmd-err-tr-404");

            return ctx.RespondWithLocalizedEmbedAsync(emb => {
                emb.WithTitle("str-tr-matching");
                emb.WithDescription(Formatter.InlineCode(tr.Triggers.JoinWith(" | ")));
                emb.WithColor(this.ModuleColor);
                emb.AddLocalizedTitleField("str-id", tr.Id, inline: true);
                emb.AddLocalizedTitleField("str-response", tr.Response, inline: true);
            });
        }
        #endregion

        #region textreactions list
        [Command("list")]
        [Aliases("print", "show", "view", "ls", "l", "p")]
        public Task ListAsync(CommandContext ctx)
        {
            IReadOnlyCollection<TextReaction> trs = this.Service.GetGuildTextReactions(ctx.Guild.Id);
            if (!trs.Any())
                throw new CommandFailedException(ctx, "cmd-err-tr-none");

            return ctx.PaginateAsync(
                "str-tr",
                trs.OrderBy(er => er.Id),
                tr => $"{Formatter.InlineCode($"{tr.Id:D4}")} : {tr.Response} | Triggers: {Formatter.InlineCode(string.Join(" | ", tr.Triggers))}",
                this.ModuleColor
            );
        }
        #endregion


        #region internals
        private async Task AddTextReactionAsync(CommandContext ctx, string trigger, string response, bool regex)
        {
            if (string.IsNullOrWhiteSpace(response))
                throw new InvalidCommandUsageException(ctx, "cmd-err-tr-resp");

            if (trigger.Length < 2 || response.Length < 2)
                throw new CommandFailedException(ctx, "cmd-err-trig-len-min", 2);

            if (trigger.Length > ReactionTrigger.TriggerLimit)
                throw new CommandFailedException(ctx, "cmd-err-trig-len", ReactionTrigger.TriggerLimit);

            if (response.Length > Reaction.ResponseLimit)
                throw new CommandFailedException(ctx, "cmd-err-resp-len", ReactionTrigger.TriggerLimit);

            if (regex && !trigger.TryParseRegex(out _))
                throw new CommandFailedException(ctx, "cmd-err-trig-regex", trigger);

            if (this.Service.GuildHasTextReaction(ctx.Guild.Id, trigger))
                throw new CommandFailedException(ctx, "cmd-err-trig-exists", trigger);

            if (ctx.Services.GetRequiredService<FilteringService>().TextContainsFilter(ctx.Guild.Id, trigger, out _))
                throw new CommandFailedException(ctx, "cmd-err-trig-coll", trigger);

            if (!await this.Service.AddTextReactionAsync(ctx.Guild.Id, trigger, response, regex))
                throw new CommandFailedException(ctx, "cmd-err-trig-fail", trigger);

            await ctx.ImpInfoAsync(this.ModuleColor, "fmt-tr-add", 1);

            await ctx.GuildLogAsync(emb => {
                emb.WithLocalizedTitle("evt-tr-add");
                emb.WithColor(this.ModuleColor);
                emb.AddLocalizedTitleField("str-response", response, inline: true);
                emb.AddLocalizedTitleField("str-count", 1, inline: true);
                emb.AddLocalizedTitleField("str-trigger", trigger);
            });
        }
        #endregion
    }
}
