using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;
using Microsoft.Extensions.DependencyInjection;
using TheGodfather.Attributes;
using TheGodfather.Database.Models;
using TheGodfather.Exceptions;
using TheGodfather.Extensions;
using TheGodfather.Modules.Misc.Services;
using TheGodfather.Services;

namespace TheGodfather.Modules.Misc
{
    [Group("birthday"), Module(ModuleType.Miscellaneous), NotBlocked]
    [Aliases("birthdays", "bday", "bd", "bdays")]
    [RequireGuild, RequireUserPermissions(Permissions.ManageGuild)]
    [Cooldown(3, 5, CooldownBucketType.Guild)]
    public class BirthdayModule : TheGodfatherServiceModule<BirthdayService>
    {
        public BirthdayModule(BirthdayService service)
            : base(service) { }


        #region birthday
        [GroupCommand, Priority(3)]
        public Task ExecuteGroupAsync(CommandContext ctx,
                                     [Description("desc-bd-user")] DiscordUser user)
            => this.ListAsync(ctx, user);

        [GroupCommand, Priority(2)]
        public Task ExecuteGroupAsync(CommandContext ctx,
                                     [Description("desc-bd-channel")] DiscordChannel? channel = null)
            => this.ListAsync(ctx, channel);

        [GroupCommand, Priority(1)]
        public Task ExecuteGroupAsync(CommandContext ctx,
                                     [Description("desc-bd-user")] DiscordUser user,
                                     [Description("desc-bd-channel")] DiscordChannel channel,
                                     [Description("desc-bd-date")] string? date = null)
            => this.AddAsync(ctx, user, date, channel);

        [GroupCommand, Priority(0)]
        public Task ExecuteGroupAsync(CommandContext ctx,
                                     [Description("desc-bd-user")] DiscordUser user,
                                     [Description("desc-bd-date")] string? date,
                                     [Description("desc-bd-channel")] DiscordChannel? channel = null)
            => this.AddAsync(ctx, user, date, channel);
        #endregion

        #region birthday add
        [Command("add"), Priority(0)]
        [Aliases("register", "reg", "a", "+", "+=", "<<", "<", "<-", "<=")]
        public async Task AddAsync(CommandContext ctx,
                                  [Description("desc-bd-user")] DiscordUser user,
                                  [Description("desc-bd-date")] string? date,
                                  [Description("desc-bd-chn")] DiscordChannel? channel = null)
        {
            channel ??= ctx.Channel;
            if (channel.Type != ChannelType.Text)
                throw new CommandFailedException(ctx, "cmd-err-chn-type-text");

            LocalizationService lcs = ctx.Services.GetRequiredService<LocalizationService>();
            DateTimeStyles styles = DateTimeStyles.NoCurrentDateDefault | DateTimeStyles.AssumeLocal | DateTimeStyles.AllowInnerWhite;

            DateTime dt = DateTime.Now;
            if (date is { } && !DateTime.TryParse(date, lcs.GetGuildCulture(ctx.Guild.Id).DateTimeFormat, styles, out dt))
                throw new InvalidCommandUsageException(ctx, "cmd-err-date-format");

            if (channel.Type != ChannelType.Text)
                throw new CommandFailedException(ctx, "cmd-err-chn-type-text");

            await this.Service.AddAsync(new Birthday {
                ChannelId = channel.Id,
                Date = dt,
                GuildId = ctx.Guild.Id,
                UserId = user.Id
            });

            await ctx.InfoAsync(this.ModuleColor, "fmt-bd-add", channel.Mention, user.Mention, lcs.GetLocalizedTime(ctx.Guild.Id, dt));
        }

        [Command("add"), Priority(1)]
        public Task AddAsync(CommandContext ctx,
                            [Description("desc-bd-user")] DiscordUser user,
                            [Description("desc-bd-chn")] DiscordChannel? channel = null,
                            [Description("desc-bd-date")] string? date = null)
            => this.AddAsync(ctx, user, date, channel);
        #endregion

        #region birthday delete
        [Command("delete"), Priority(1), UsesInteractivity]
        [Aliases("unregister", "remove", "rm", "del", "d", "-", "-=", ">", ">>", "->", "=>")]
        public async Task DeleteAsync(CommandContext ctx,
                                     [Description("desc-bd-user")] DiscordUser user)
        {
            IReadOnlyList<Birthday> bds = await this.Service.GetUserBirthdaysAsync(ctx.Guild.Id, user.Id);
            await this.Service.RemoveAsync(bds);
            await ctx.InfoAsync(this.ModuleColor, "fmt-bd-rem-user", bds.Count, user.Mention);
        }

        [Command("delete"), Priority(0)]
        public async Task DeleteAsync(CommandContext ctx,
                                     [Description("desc-bd-chn")] DiscordChannel channel)
        {
            if (channel.Type != ChannelType.Text)
                throw new CommandFailedException(ctx, "cmd-err-chn-type-text");

            if (!await ctx.WaitForBoolReplyAsync("q-bd-rem-all", args: channel.Mention))
                return;

            await this.Service.ClearAsync((ctx.Guild.Id, ctx.Channel.Id));
            await ctx.InfoAsync(this.ModuleColor);
        }
        #endregion

        #region birthday deleteall
        [Command("deleteall"), UsesInteractivity]
        [Aliases("removeall", "rmrf", "rma", "clearall", "clear", "delall", "da", "cl", "-a", "--", ">>>")]
        public async Task RemoveAllAsync(CommandContext ctx)
        {
            if (!await ctx.WaitForBoolReplyAsync("q-bd-clear"))
                return;

            IReadOnlyList<Birthday> bds = await this.Service.GetAllBirthdaysAsync(ctx.Guild.Id);
            await this.Service.RemoveAsync(bds);
            await ctx.InfoAsync(this.ModuleColor);
        }
        #endregion

        #region birthday list
        [Command("list"), Priority(1)]
        [Aliases("print", "show", "ls", "l", "p")]
        public async Task ListAsync(CommandContext ctx,
                                   [Description("desc-bd-user")] DiscordUser user)
        {
            IReadOnlyList<Birthday> bds = await this.Service.GetUserBirthdaysAsync(ctx.Guild.Id, user.Id);
            await this.InternalListAsync(ctx, bds);
        }

        [Command("list"), Priority(0)]
        public async Task ListAsync(CommandContext ctx,
                                   [Description("desc-bd-chn")] DiscordChannel? channel = null)
        {
            channel ??= ctx.Channel;
            if (channel.Type != ChannelType.Text)
                throw new CommandFailedException(ctx, "cmd-err-chn-type-text");

            IReadOnlyList<Birthday> bds = await this.Service.GetAsync((ctx.Guild.Id, channel.Id));
            await this.InternalListAsync(ctx, bds);
        }
        #endregion

        #region birthday listall
        [Command("listall")]
        [Aliases("printall", "showall", "lsa", "la", "pa")]
        public async Task ListAllAsync(CommandContext ctx)
        {
            IReadOnlyList<Birthday> bds = await this.Service.GetAllBirthdaysAsync(ctx.Guild.Id);
            await this.InternalListAsync(ctx, bds);
        }
        #endregion


        #region Helpers
        public async Task InternalListAsync(CommandContext ctx, IReadOnlyList<Birthday> bds)
        {
            if (!bds.Any())
                throw new CommandFailedException(ctx, "cmd-err-bd-none");

            var lcs = ctx.Services.GetRequiredService<LocalizationService>();
            var bdaysToRemove = new List<Birthday>();
            var lines = new List<string>();
            foreach (IGrouping<ulong, Birthday> g in bds.GroupBy(bd => bd.ChannelId)) {
                try {
                    DiscordChannel channel = await ctx.Client.GetChannelAsync(g.Key);
                    foreach (Birthday bd in g) {
                        DiscordUser? user = await ctx.Client.GetUserAsync(bd.UserId);
                        if (user is { })
                            lines.Add($"{Formatter.InlineCode(lcs.GetLocalizedTime(ctx.Guild.Id, bd.Date, "d"))} | {user.Mention} | {channel.Mention}");
                        else
                            bdaysToRemove.Add(bd);
                    }
                } catch (NotFoundException) {
                    bdaysToRemove.AddRange(g);
                }
            }

            if (bdaysToRemove.Any()) {
                LogExt.Information(ctx, "Cleaning {Count} invalid birthdays...", bdaysToRemove.Count);
                await this.Service.RemoveAsync(bdaysToRemove);
            }

            await ctx.PaginateAsync("str-bdays", lines, line => line, this.ModuleColor, 10);
        }
        #endregion
    }
}
