using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Microsoft.Extensions.DependencyInjection;
using TheGodfather.Database.Models;
using TheGodfather.Exceptions;
using TheGodfather.Extensions;
using TheGodfather.Modules.Administration.Extensions;
using TheGodfather.Modules.Administration.Services;

namespace TheGodfather.Modules.Administration
{
    [Group("logging")]
    [Aliases("log", "modlog")]
    public class LoggingModule : TheGodfatherServiceModule<LoggingService>
    {
        public LoggingModule(LoggingService service)
            : base(service) { }


        #region config logging
        [GroupCommand, Priority(3)]
        public async Task ExecuteGroupAsync(CommandContext ctx,
                                           [Description("desc-enable")] bool enable,
                                           [Description("desc-log-chn")] DiscordChannel? channel)
        {
            channel ??= ctx.Channel;
            if (channel.Type != ChannelType.Text)
                throw new CommandFailedException(ctx, "cmd-err-chn-type-text");

            await ctx.Services.GetRequiredService<GuildConfigService>().ModifyConfigAsync(ctx.Guild.Id, cfg => {
                cfg.LogChannelIdDb = enable ? (long)channel.Id : 0;
            });

            await ctx.GuildLogAsync(emb => {
                emb.WithLocalizedTitle("evt-cfg-upd");
                emb.WithColor(this.ModuleColor);
                if (enable)
                    emb.WithLocalizedDescription("evt-log-on", channel.Mention);
                else
                    emb.WithLocalizedDescription("evt-log-off");
            });

            if (enable)
                await ctx.InfoAsync(this.ModuleColor, "evt-log-on", channel.Mention);
            else
                await ctx.InfoAsync(this.ModuleColor, "evt-log-off");
        }

        [GroupCommand, Priority(2)]
        public Task ExecuteGroupAsync(CommandContext ctx,
                                     [Description("desc-log-chn")] DiscordChannel channel)
            => this.ExecuteGroupAsync(ctx, true, channel);

        [GroupCommand, Priority(1)]
        public Task ExecuteGroupAsync(CommandContext ctx,
                                     [Description("desc-enable")] bool enable)
            => this.ExecuteGroupAsync(ctx, enable, ctx.Channel);

        [GroupCommand, Priority(0)]
        public async Task ExecuteGroupAsync(CommandContext ctx)
        {
            IReadOnlyList<ExemptedLoggingEntity> exempts = await this.Service.GetExemptsAsync(ctx.Guild.Id);
            string? exemptString = await exempts.FormatExemptionsAsync(ctx.Client);
            await ctx.WithGuildConfigAsync(gcfg => 
                ctx.RespondWithLocalizedEmbedAsync(emb => {
                    emb.WithLocalizedTitle("str-logging");
                    if (gcfg.LoggingEnabled) {
                        DiscordChannel? logchn = ctx.Services.GetRequiredService<GuildConfigService>().GetLogChannelForGuild(ctx.Guild);
                        emb.WithLocalizedDescription("evt-log-on", gcfg.LogChannelId);
                    } else {
                        emb.WithLocalizedDescription("evt-log-off");
                    }
                    emb.WithColor(this.ModuleColor);
                    if (exemptString is { })
                        emb.AddLocalizedTitleField("str-exempts", exemptString, inline: true);
                })
            );
        }
        #endregion

        #region config antispam exempt
        [Command("exempt"), Priority(2)]
        [Aliases("ex", "exc")]
        public async Task ExemptAsync(CommandContext ctx,
                                     [Description("desc-exempt-user")] params DiscordMember[] members)
        {
            if (members is null || !members.Any())
                throw new CommandFailedException(ctx, "cmd-err-exempt");

            await this.Service.ExemptAsync(ctx.Guild.Id, ExemptedEntityType.Member, members.SelectIds());
            await ctx.InfoAsync();
        }

        [Command("exempt"), Priority(1)]
        public async Task ExemptAsync(CommandContext ctx,
                                     [Description("desc-exempt-role")] params DiscordRole[] roles)
        {
            if (roles is null || !roles.Any())
                throw new CommandFailedException(ctx, "cmd-err-exempt");

            await this.Service.ExemptAsync(ctx.Guild.Id, ExemptedEntityType.Role, roles.SelectIds());
            await ctx.InfoAsync();
        }

        [Command("exempt"), Priority(0)]
        public async Task ExemptAsync(CommandContext ctx,
                                     [Description("desc-exempt-chn")] params DiscordChannel[] channels)
        {
            if (channels is null || !channels.Any())
                throw new CommandFailedException(ctx, "cmd-err-exempt");

            await this.Service.ExemptAsync(ctx.Guild.Id, ExemptedEntityType.Channel, channels.SelectIds());
            await ctx.InfoAsync();
        }
        #endregion

        #region config antispam unexempt
        [Command("unexempt"), Priority(2)]
        [Aliases("unex", "uex")]
        public async Task UnxemptAsync(CommandContext ctx,
                                      [Description("desc-unexempt-user")] params DiscordMember[] members)
        {
            if (members is null || !members.Any())
                throw new CommandFailedException(ctx, "cmd-err-exempt");

            await this.Service.UnexemptAsync(ctx.Guild.Id, ExemptedEntityType.Member, members.SelectIds());
            await ctx.InfoAsync();
        }

        [Command("unexempt"), Priority(1)]
        public async Task UnxemptAsync(CommandContext ctx,
                                      [Description("desc-unexempt-role")] params DiscordRole[] roles)
        {
            if (roles is null || !roles.Any())
                throw new CommandFailedException(ctx, "cmd-err-exempt");

            await this.Service.UnexemptAsync(ctx.Guild.Id, ExemptedEntityType.Role, roles.SelectIds());
            await ctx.InfoAsync();
        }

        [Command("unexempt"), Priority(0)]
        public async Task UnxemptAsync(CommandContext ctx,
                                      [Description("desc-unexempt-chn")] params DiscordChannel[] channels)
        {
            if (channels is null || !channels.Any())
                throw new CommandFailedException(ctx, "cmd-err-exempt");

            await this.Service.UnexemptAsync(ctx.Guild.Id, ExemptedEntityType.Channel, channels.SelectIds());
            await ctx.InfoAsync();
        }
        #endregion
    }
}
