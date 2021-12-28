using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using TheGodfather.Database.Models;
using TheGodfather.Exceptions;
using TheGodfather.Extensions;
using TheGodfather.Modules.Administration.Extensions;
using TheGodfather.Modules.Administration.Services;

namespace TheGodfather.Modules.Administration
{
    public sealed partial class ConfigModule
    {
        [Group("backup")]
        [Aliases("bk", "bak")]
        public sealed class BackupModule : TheGodfatherServiceModule<BackupService>
        {
            #region config backup
            [GroupCommand, Priority(1)]
            public async Task ExecuteGroupAsync(CommandContext ctx,
                                               [Description(TranslationKey.desc_enable)] bool enable)
            {
                if (enable)
                    await this.Service.EnableAsync(ctx.Guild.Id);
                else
                    await this.Service.DisableAsync(ctx.Guild.Id);

                await ctx.GuildLogAsync(emb => {
                    emb.WithLocalizedTitle(TranslationKey.evt_cfg_upd);
                    emb.WithColor(this.ModuleColor);
                    if (enable) {
                        emb.WithLocalizedDescription(TranslationKey.evt_bak_enable);
                    } else {
                        emb.WithLocalizedDescription(TranslationKey.evt_bak_disable);
                    }
                });

                await ctx.InfoAsync(enable ? TranslationKey.evt_bak_enable : TranslationKey.evt_bak_disable);
            }

            [GroupCommand, Priority(0)]
            public async Task ExecuteGroupAsync(CommandContext ctx)
            {
                IReadOnlyList<ExemptedBackupEntity> exempts = await this.Service.GetExemptsAsync(ctx.Guild.Id);
                string? exemptString = await exempts.FormatExemptionsAsync(ctx.Client);
                await ctx.WithGuildConfigAsync(gcfg => {
                    return ctx.RespondWithLocalizedEmbedAsync(emb => {
                        emb.WithLocalizedTitle(TranslationKey.str_backup);
                        emb.WithLocalizedDescription(gcfg.BackupEnabled ? TranslationKey.str_enabled : TranslationKey.str_disabled);
                        emb.WithColor(this.ModuleColor);
                        emb.AddLocalizedField(TranslationKey.str_exempts, exemptString, inline: true, unknown: false);
                    });
                });
            }
            #endregion

            #region config backup download
            [Command("download")]
            [Aliases("dl", "get", "zip")]
            [RequireUserPermissions(Permissions.Administrator)]
            public async Task DownloadAsync(CommandContext ctx)
            {
                if (!await this.Service.WithBackupZipAsync(ctx.Guild.Id, s => ctx.RespondAsync(new DiscordMessageBuilder().WithFile("backup.zip", s))))
                    throw new CommandFailedException(ctx, TranslationKey.cmd_err_backup);
            }
            #endregion

            #region config backup exempt
            [Command("exempt")]
            [Aliases("ex", "exc")]
            public async Task ExemptAsync(CommandContext ctx,
                                         [Description(TranslationKey.desc_exempt_chn)] params DiscordChannel[] channels)
            {
                if (channels is null || !channels.Any())
                    throw new CommandFailedException(ctx, TranslationKey.cmd_err_exempt);

                await this.Service.ExemptAsync(ctx.Guild.Id, this.SelectChildChannelIds(channels));
                await ctx.InfoAsync(this.ModuleColor);
            }
            #endregion

            #region config backup unexempt
            [Command("unexempt")]
            [Aliases("unex", "uex")]
            public async Task UnxemptAsync(CommandContext ctx,
                                          [Description(TranslationKey.desc_unexempt_chn)] params DiscordChannel[] channels)
            {
                if (channels is null || !channels.Any())
                    throw new CommandFailedException(ctx, TranslationKey.cmd_err_exempt);

                await this.Service.UnexemptAsync(ctx.Guild.Id, this.SelectChildChannelIds(channels));
                await ctx.InfoAsync(this.ModuleColor);
            }
            #endregion


            #region internals
            private IEnumerable<ulong> SelectChildChannelIds(params DiscordChannel[] channels)
            {
                return channels
                    .Where(c => c.Type is ChannelType.Text or ChannelType.Category)
                    .SelectMany(c => c.Type == ChannelType.Category ? c.Children.Select(c => c.Id) : new[] { c.Id })
                    ;
            }
            #endregion
        }
    }
}
