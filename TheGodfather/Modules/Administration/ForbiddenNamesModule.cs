using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;
using TheGodfather.Attributes;
using TheGodfather.Database.Models;
using TheGodfather.EventListeners.Common;
using TheGodfather.Exceptions;
using TheGodfather.Extensions;
using TheGodfather.Modules.Administration.Extensions;
using TheGodfather.Modules.Administration.Services;

namespace TheGodfather.Modules.Administration
{
    [Group("forbiddennames"), Module(ModuleType.Administration), NotBlocked]
    [Aliases("forbiddenname", "forbiddennicknames", "disallowednames", "fnames", "fname", "fn")]
    [RequireGuild, RequireUserPermissions(Permissions.ManageGuild), RequirePermissions(Permissions.ManageNicknames)]
    [Cooldown(3, 5, CooldownBucketType.Guild)]
    public sealed class ForbiddenNamesModule : TheGodfatherServiceModule<ForbiddenNamesService>
    {
        #region forbiddennames
        [GroupCommand, Priority(1)]
        public Task ExecuteGroupAsync(CommandContext ctx)
            => this.ListAsync(ctx);

        [GroupCommand, Priority(0)]
        public Task ExecuteGroupAsync(CommandContext ctx,
                                     [RemainingText, Description(TranslationKey.desc_fnames)] params string[] names)
            => this.AddAsync(ctx, names);
        #endregion

        #region forbiddennames add
        [Command("add"), Priority(2)]
        [Aliases("register", "reg", "a", "+", "+=", "<<", "<", "<-", "<=")]
        public Task AddAsync(CommandContext ctx,
                            [Description(TranslationKey.desc_punish_action)] Punishment.Action action,
                            [RemainingText, Description(TranslationKey.desc_fnames)] params string[] names)
            => this.InternalAddAsync(ctx, action, names);

        [Command("add"), Priority(1)]
        public Task AddAsync(CommandContext ctx,
                            [Description(TranslationKey.desc_fname)] string name,
                            [Description(TranslationKey.desc_punish_action)] Punishment.Action? action = null)
            => this.InternalAddAsync(ctx, action, new[] { name });

        [Command("add"), Priority(0)]
        public Task AddAsync(CommandContext ctx,
                            [RemainingText, Description(TranslationKey.desc_fnames)] params string[] names)
            => this.InternalAddAsync(ctx, null, names);
        #endregion

        #region forbiddennames delete
        [Group("delete")]
        [Aliases("remove", "rm", "del", "d", "-", "-=", ">", ">>")]
        public class ForbiddenNamesDeleteModule : TheGodfatherServiceModule<ForbiddenNamesService>
        {
            #region forbiddennames delete
            [GroupCommand, Priority(1)]
            public Task DeleteAsync(CommandContext ctx,
                                   [RemainingText, Description(TranslationKey.desc_fnames_del_ids)] params int[] ids)
                => this.DeleteIdAsync(ctx, ids);

            [GroupCommand, Priority(0)]
            public Task DeleteAsync(CommandContext ctx,
                                   [RemainingText, Description(TranslationKey.desc_fnames_del)] params string[] regexStrings)
                => this.DeletePatternAsync(ctx, regexStrings);
            #endregion

            #region forbiddennames delete id
            public async Task DeleteIdAsync(CommandContext ctx,
                                           [RemainingText, Description(TranslationKey.desc_fnames_del_ids)] params int[] ids)
            {
                if (ids is null || !ids.Any())
                    throw new CommandFailedException(ctx, TranslationKey.cmd_err_f_ids_none);

                IReadOnlyCollection<ForbiddenName> fns = this.Service.GetGuildForbiddenNames(ctx.Guild.Id);
                if (!fns.Any())
                    throw new InvalidCommandUsageException(ctx, TranslationKey.cmd_err_fn_none);

                int removed = await this.Service.RemoveForbiddenNamesAsync(ctx.Guild.Id, ids);

                await ctx.GuildLogAsync(emb => {
                    emb.WithLocalizedTitle(DiscordEventType.GuildUpdated, TranslationKey.evt_fn_del);
                    emb.WithDescription(ids.JoinWith());
                });

                await ctx.InfoAsync(this.ModuleColor, TranslationKey.str_fn_del(removed));
            }
            #endregion

            #region forbiddennames delete matching
            public async Task DeleteMatchingAsync(CommandContext ctx,
                                                 [Description(TranslationKey.desc_fnames_del)] string match)
            {
                if (string.IsNullOrWhiteSpace(match))
                    throw new CommandFailedException(ctx, TranslationKey.cmd_err_fn_pat_none);

                IReadOnlyCollection<ForbiddenName> fns = this.Service.GetGuildForbiddenNames(ctx.Guild.Id);
                if (!fns.Any())
                    throw new InvalidCommandUsageException(ctx, TranslationKey.cmd_err_fn_none);

                int removed = await this.Service.RemoveForbiddenNamesMatchingAsync(ctx.Guild.Id, match);

                await ctx.GuildLogAsync(emb => {
                    emb.WithLocalizedTitle(DiscordEventType.GuildUpdated, TranslationKey.evt_fn_del);
                    emb.WithDescription(match);
                });

                await ctx.InfoAsync(this.ModuleColor, TranslationKey.str_fn_del(removed));
            }
            #endregion

            #region forbiddennames delete pattern
            public async Task DeletePatternAsync(CommandContext ctx,
                                                [RemainingText, Description(TranslationKey.desc_fnames_del)] params string[] regexStrings)
            {
                if (regexStrings is null || !regexStrings.Any())
                    throw new CommandFailedException(ctx, TranslationKey.cmd_err_fn_pat_none);

                IReadOnlyCollection<ForbiddenName> fs = this.Service.GetGuildForbiddenNames(ctx.Guild.Id);
                if (!fs.Any())
                    throw new InvalidCommandUsageException(ctx, TranslationKey.cmd_err_fn_none);

                int removed = await this.Service.RemoveForbiddenNamesAsync(ctx.Guild.Id, regexStrings);

                await ctx.GuildLogAsync(emb => {
                    emb.WithLocalizedTitle(DiscordEventType.GuildUpdated, TranslationKey.evt_fn_del);
                    emb.WithDescription(regexStrings.JoinWith());
                });

                await ctx.InfoAsync(this.ModuleColor, TranslationKey.str_fn_del(removed));
            }
            #endregion
        }
        #endregion

        #region forbiddennames deleteall
        [Command("deleteall"), UsesInteractivity]
        [Aliases("removeall", "rmrf", "rma", "clearall", "clear", "delall", "da", "cl", "-a", "--", ">>>")]
        public async Task DeleteAllAsync(CommandContext ctx)
        {
            if (!await ctx.WaitForBoolReplyAsync(TranslationKey.q_fn_rem_all))
                return;

            int removed = await this.Service.RemoveForbiddenNamesAsync(ctx.Guild.Id);

            await ctx.GuildLogAsync(emb => {
                emb.WithLocalizedTitle(DiscordEventType.GuildUpdated, TranslationKey.evt_fn_del_all);
                emb.AddLocalizedField(TranslationKey.str_count, removed, inline: true);
            });

            await ctx.InfoAsync(this.ModuleColor, TranslationKey.str_fn_del_all(removed));
        }
        #endregion

        #region forbiddennames list
        [Command("list")]
        [Aliases("print", "show", "view", "ls", "l", "p")]
        public Task ListAsync(CommandContext ctx)
        {
            IReadOnlyCollection<ForbiddenName> fs = this.Service.GetGuildForbiddenNames(ctx.Guild.Id);
            return fs.Any()
                ? ctx.PaginateAsync(
                    TranslationKey.str_fn,
                    fs.OrderBy(f => f.Id),
                    f => $"{Formatter.InlineCode($"{f.Id:D3}")} | {Formatter.InlineCode(f.RegexString)}",
                    this.ModuleColor
                )
                : throw new CommandFailedException(ctx, TranslationKey.cmd_err_fn_none);
        }
        #endregion


        #region internals
        public async Task InternalAddAsync(CommandContext ctx, Punishment.Action? action, params string[] names)
        {
            if (names is null || !names.Any())
                throw new InvalidCommandUsageException(ctx, TranslationKey.cmd_err_fn_pat_none);

            var eb = new StringBuilder();
            var addedPatterns = new List<Regex>();
            foreach (string regexString in names) {
                if (regexString.Length is < 3 or > ForbiddenName.NameLimit) {
                    eb.AppendLine(this.Localization.GetString(ctx.Guild.Id, TranslationKey.cmd_err_fn_size(Formatter.InlineCode(regexString), ForbiddenName.NameLimit)));
                    continue;
                }

                if (!regexString.TryParseRegex(out Regex? regex) || regex is null) {
                    eb.AppendLine(this.Localization.GetString(ctx.Guild.Id, TranslationKey.cmd_err_fn_invalid(Formatter.InlineCode(regexString))));
                    continue;
                }

                if (!this.Service.IsSafePattern(regex)) {
                    eb.AppendLine(this.Localization.GetString(ctx.Guild.Id, TranslationKey.cmd_err_fn_unsafe(Formatter.InlineCode(regexString))));
                    continue;
                }

                if (!await this.Service.AddForbiddenNameAsync(ctx.Guild.Id, regex, action)) {
                    eb.AppendLine(this.Localization.GetString(ctx.Guild.Id, TranslationKey.cmd_err_fn_dup(Formatter.InlineCode(regexString))));
                    continue;
                }

                addedPatterns.Add(regex);
            }

            DiscordMember bot = ctx.Guild.CurrentMember;
            bool failed = false;
            IReadOnlyCollection<DiscordMember> members = await ctx.Guild.GetAllMembersAsync();
            foreach (DiscordMember member in members.Where(m => !m.IsBot && m.Hierarchy < bot.Hierarchy)) {
                Regex? match = addedPatterns.FirstOrDefault(r => r.IsMatch(member.DisplayName));
                if (match is { }) {
                    try {
                        if (action is { }) {
                            await this.Service.PunishMemberAsync(ctx.Guild, member, action.Value);
                        } else {
                            await member.ModifyAsync(m => {
                                m.Nickname = member.Id.ToString();
                                m.AuditLogReason = this.Localization.GetString(ctx.Guild.Id, TranslationKey.rsn_fname_match(match));
                            });
                        }
                        if (!member.IsBot)
                            await member.SendMessageAsync(this.Localization.GetString(null, TranslationKey.dm_fname_match(Formatter.Italic(ctx.Guild.Name))));
                    } catch (UnauthorizedException) {
                        if (!failed) {
                            failed = true;
                            eb.Append(this.Localization.GetString(ctx.Guild.Id, TranslationKey.err_fname_match));
                        }
                    }
                }
            }

            await ctx.GuildLogAsync(emb => {
                emb.WithLocalizedTitle(DiscordEventType.GuildUpdated, TranslationKey.evt_fn_add);
                emb.AddLocalizedField(TranslationKey.str_fn_add, Formatter.BlockCode(addedPatterns.Select(p => p.ToString()).JoinWith()));
                if (eb.Length > 0)
                    emb.AddLocalizedField(TranslationKey.str_err, eb);
            });

            if (eb.Length > 0)
                await ctx.FailAsync(TranslationKey.fmt_err(eb.ToString()));
            else
                await ctx.InfoAsync(this.ModuleColor, TranslationKey.str_fn_add);
        }
        #endregion
    }
}
