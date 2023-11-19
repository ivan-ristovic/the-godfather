using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Microsoft.Extensions.DependencyInjection;
using TheGodfather.Modules.Administration.Extensions;
using TheGodfather.Modules.Administration.Services;

namespace TheGodfather.Modules.Administration;

public sealed partial class ConfigModule
{
    [Group("logging")]
    [Aliases("log", "modlog")]
    public sealed class LoggingModule : TheGodfatherServiceModule<LoggingService>
    {
        #region config logging
        [GroupCommand][Priority(3)]
        public async Task ExecuteGroupAsync(CommandContext ctx,
            [Description(TranslationKey.desc_enable)] bool enable,
            [Description(TranslationKey.desc_log_chn)] DiscordChannel? channel)
        {
            channel ??= ctx.Channel;
            if (channel.Type != ChannelType.Text)
                throw new CommandFailedException(ctx, TranslationKey.cmd_err_chn_type_text);

            await ctx.Services.GetRequiredService<GuildConfigService>().ModifyConfigAsync(ctx.Guild.Id, cfg => {
                cfg.LogChannelIdDb = enable ? (long)channel.Id : 0;
            });

            await ctx.GuildLogAsync(emb => {
                emb.WithLocalizedTitle(TranslationKey.evt_cfg_upd);
                emb.WithColor(this.ModuleColor);
                if (enable)
                    emb.WithLocalizedDescription(TranslationKey.evt_log_on(channel.Mention));
                else
                    emb.WithLocalizedDescription(TranslationKey.evt_log_off);
            });

            if (enable)
                await ctx.InfoAsync(this.ModuleColor, TranslationKey.evt_log_on(channel.Mention));
            else
                await ctx.InfoAsync(this.ModuleColor, TranslationKey.evt_log_off);
        }

        [GroupCommand][Priority(2)]
        public Task ExecuteGroupAsync(CommandContext ctx,
            [Description(TranslationKey.desc_log_chn)] DiscordChannel channel)
            => this.ExecuteGroupAsync(ctx, true, channel);

        [GroupCommand][Priority(1)]
        public Task ExecuteGroupAsync(CommandContext ctx,
            [Description(TranslationKey.desc_enable)] bool enable)
            => this.ExecuteGroupAsync(ctx, enable, ctx.Channel);

        [GroupCommand][Priority(0)]
        public async Task ExecuteGroupAsync(CommandContext ctx)
        {
            IReadOnlyList<ExemptedLoggingEntity> exempts = await this.Service.GetExemptsAsync(ctx.Guild.Id);
            string? exemptString = await exempts.FormatExemptionsAsync(ctx.Client);
            await ctx.WithGuildConfigAsync(gcfg =>
                ctx.RespondWithLocalizedEmbedAsync(emb => {
                    emb.WithLocalizedTitle(TranslationKey.str_logging);
                    if (gcfg.LoggingEnabled) {
                        DiscordChannel? logchn = ctx.Services.GetRequiredService<GuildConfigService>().GetLogChannelForGuild(ctx.Guild);
                        emb.WithLocalizedDescription(TranslationKey.evt_log_on(logchn?.Mention ?? gcfg.LogChannelId.ToString()));
                    } else {
                        emb.WithLocalizedDescription(TranslationKey.evt_log_off);
                    }
                    emb.WithColor(this.ModuleColor);
                    if (exemptString is not null)
                        emb.AddLocalizedField(TranslationKey.str_exempts, exemptString, true);
                })
            );
        }
        #endregion

        #region config antispam exempt
        [Command("exempt")][Priority(2)]
        [Aliases("ex", "exc")]
        public async Task ExemptAsync(CommandContext ctx,
            [Description(TranslationKey.desc_exempt_user)] params DiscordMember[] members)
        {
            if (members is null || !members.Any())
                throw new CommandFailedException(ctx, TranslationKey.cmd_err_exempt);

            await this.Service.ExemptAsync(ctx.Guild.Id, ExemptedEntityType.Member, members.SelectIds());
            await ctx.InfoAsync(this.ModuleColor);
        }

        [Command("exempt")][Priority(1)]
        public async Task ExemptAsync(CommandContext ctx,
            [Description(TranslationKey.desc_exempt_role)] params DiscordRole[] roles)
        {
            if (roles is null || !roles.Any())
                throw new CommandFailedException(ctx, TranslationKey.cmd_err_exempt);

            await this.Service.ExemptAsync(ctx.Guild.Id, ExemptedEntityType.Role, roles.SelectIds());
            await ctx.InfoAsync(this.ModuleColor);
        }

        [Command("exempt")][Priority(0)]
        public async Task ExemptAsync(CommandContext ctx,
            [Description(TranslationKey.desc_exempt_chn)] params DiscordChannel[] channels)
        {
            if (channels is null || !channels.Any())
                throw new CommandFailedException(ctx, TranslationKey.cmd_err_exempt);

            await this.Service.ExemptAsync(ctx.Guild.Id, ExemptedEntityType.Channel, channels.SelectIds());
            await ctx.InfoAsync(this.ModuleColor);
        }
        #endregion

        #region config antispam unexempt
        [Command("unexempt")][Priority(2)]
        [Aliases("unex", "uex")]
        public async Task UnxemptAsync(CommandContext ctx,
            [Description(TranslationKey.desc_unexempt_user)] params DiscordMember[] members)
        {
            if (members is null || !members.Any())
                throw new CommandFailedException(ctx, TranslationKey.cmd_err_exempt);

            await this.Service.UnexemptAsync(ctx.Guild.Id, ExemptedEntityType.Member, members.SelectIds());
            await ctx.InfoAsync(this.ModuleColor);
        }

        [Command("unexempt")][Priority(1)]
        public async Task UnxemptAsync(CommandContext ctx,
            [Description(TranslationKey.desc_unexempt_role)] params DiscordRole[] roles)
        {
            if (roles is null || !roles.Any())
                throw new CommandFailedException(ctx, TranslationKey.cmd_err_exempt);

            await this.Service.UnexemptAsync(ctx.Guild.Id, ExemptedEntityType.Role, roles.SelectIds());
            await ctx.InfoAsync(this.ModuleColor);
        }

        [Command("unexempt")][Priority(0)]
        public async Task UnxemptAsync(CommandContext ctx,
            [Description(TranslationKey.desc_unexempt_chn)] params DiscordChannel[] channels)
        {
            if (channels is null || !channels.Any())
                throw new CommandFailedException(ctx, TranslationKey.cmd_err_exempt);

            await this.Service.UnexemptAsync(ctx.Guild.Id, ExemptedEntityType.Channel, channels.SelectIds());
            await ctx.InfoAsync(this.ModuleColor);
        }
        #endregion
    }
}