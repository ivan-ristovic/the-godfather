using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using TheGodfather.Attributes;
using TheGodfather.Exceptions;
using TheGodfather.Extensions;
using TheGodfather.Modules.Administration.Services;
using TimeZoneConverter;

namespace TheGodfather.Modules.Administration
{
    public sealed partial class ConfigModule
    {
        [Group("timezone")]
        [Aliases("tz")]
        public sealed class TimezoneModule : TheGodfatherServiceModule<GuildConfigService>
        {
            #region config timezone
            [GroupCommand, Priority(1)]
            public Task ExecuteGroupAsync(CommandContext ctx)
                => this.CurrentAsync(ctx);

            [GroupCommand, Priority(0)]
            public Task ExecuteGroupAsync(CommandContext ctx,
                                         [RemainingText, Description("desc-tz")] string tzid)
                => this.SetAsync(ctx, tzid);
            #endregion

            #region config timezone current
            [Command("current")]
            [Aliases("curr", "active")]
            public Task CurrentAsync(CommandContext ctx)
            {
                TimeZoneInfo tzInfo = this.Localization.GetGuildTimeZone(ctx.Guild.Id);
                return ctx.InfoAsync(this.ModuleColor, "fmt-tz-curr", tzInfo.DisplayName, tzInfo.StandardName);
            }
            #endregion

            #region config timezone info
            [Command("info")]
            [Aliases("i", "information")]
            public Task InfoAsync(CommandContext ctx,
                                 [RemainingText, Description("desc-tz")] string? tzid = null)
            {
                TimeZoneInfo tzInfo = this.GetTimeZoneInfo(ctx, tzid);
                DateTimeOffset time = TimeZoneInfo.ConvertTime(DateTimeOffset.Now, tzInfo);
                return ctx.InfoAsync(this.ModuleColor, "fmt-tz", tzInfo.DisplayName, tzInfo.StandardName, time);
            }
            #endregion

            #region config timezone list
            [Command("list")]
            [Aliases("print", "show", "view", "ls", "l", "p")]
            public Task ListAsync(CommandContext ctx)
            {
                IEnumerable<(string Group, string Name)> timezones = TZConvert.KnownWindowsTimeZoneIds.Select(tz => ("Win", tz))
                    .Concat(TZConvert.KnownIanaTimeZoneNames.Select(tz => ("IANA", tz)))
                    .Concat(TZConvert.KnownRailsTimeZoneNames.Select(tz => ("Rails", tz)))
                    ;
                return ctx.PaginateAsync(
                    "str-tz-list",
                    timezones,
                    tup => $"({Formatter.InlineCode(tup.Group)}) {Formatter.Bold(tup.Name)}",
                    this.ModuleColor,
                    5
                );
            }
            #endregion

            #region config timezone set
            [Command("set")]
            [Aliases("s")]
            public async Task SetAsync(CommandContext ctx,
                                      [RemainingText, Description("desc-tz")] string tzid)
            {
                TimeZoneInfo tzInfo = this.GetTimeZoneInfo(ctx, tzid);
                await this.Service.ModifyConfigAsync(ctx.Guild.Id, gcfg => gcfg.TimezoneId = tzid);
                await ctx.InfoAsync(this.ModuleColor, "fmt-tz-set", tzInfo.DisplayName, tzInfo.StandardName);
            }
            #endregion

            #region config timezone reset
            [Command("reset"), UsesInteractivity]
            [Aliases("default", "def", "rr")]
            public async Task ResetAsync(CommandContext ctx)
            {
                if (!await ctx.WaitForBoolReplyAsync("q-setup-reset"))
                    return;
                await this.Service.ModifyConfigAsync(ctx.Guild.Id, gcfg => gcfg.TimezoneId = null);
                await ctx.InfoAsync(this.ModuleColor, "str-cfg-tz-reset");
            }
            #endregion


            #region internals
            private TimeZoneInfo GetTimeZoneInfo(CommandContext ctx, string? tzid)
            {
                tzid ??= this.Localization.GetGuildTimeZone(ctx.Guild.Id).Id;
                try {
                    return TZConvert.GetTimeZoneInfo(tzid);
                } catch (TimeZoneNotFoundException) {
                    throw new CommandFailedException(ctx, "cmd-err-tz");
                }
            }
            #endregion
        }
    }
}
