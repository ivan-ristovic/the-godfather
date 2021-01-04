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
                                               [Description("desc-enable")] bool enable)
            {
                if (enable)
                    await this.Service.EnableAsync(ctx.Guild.Id);
                else
                    await this.Service.DisableAsync(ctx.Guild.Id);

                await ctx.GuildLogAsync(emb => {
                    emb.WithLocalizedTitle("evt-cfg-upd");
                    emb.WithColor(this.ModuleColor);
                    if (enable) {
                        emb.WithLocalizedDescription("evt-bak-enable");
                    } else {
                        emb.WithLocalizedDescription("evt-bak-disable");
                    }
                });

                await ctx.InfoAsync(enable ? "evt-bak-enable" : "evt-bak-disable");
            }

            [GroupCommand, Priority(0)]
            public async Task ExecuteGroupAsync(CommandContext ctx)
            {
                IReadOnlyList<ExemptedBackupEntity> exempts = await this.Service.GetExemptsAsync(ctx.Guild.Id);
                string? exemptString = await exempts.FormatExemptionsAsync(ctx.Client);
                await ctx.WithGuildConfigAsync(gcfg => {
                    return ctx.RespondWithLocalizedEmbedAsync(emb => {
                        emb.WithLocalizedTitle("str-backup");
                        emb.WithLocalizedDescription(gcfg.BackupEnabled ? "str-enabled" : "str-disabled");
                        emb.WithColor(this.ModuleColor);
                        if (exemptString is { })
                            emb.AddLocalizedTitleField("str-exempts", exemptString, inline: true);
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
                if (!await this.Service.WithBackupZipAsync(ctx.Guild.Id, s => ctx.RespondWithFileAsync("backup.zip", s)))
                    throw new CommandFailedException(ctx, "cmd-err-backup");
            }
            #endregion

            #region config backup exempt
            [Command("exempt")]
            [Aliases("ex", "exc")]
            public async Task ExemptAsync(CommandContext ctx,
                                         [Description("desc-exempt-chn")] params DiscordChannel[] channels)
            {
                if (channels is null || !channels.Any())
                    throw new CommandFailedException(ctx, "cmd-err-exempt");

                await this.Service.ExemptAsync(ctx.Guild.Id, this.SelectChildChannelIds());
                await ctx.InfoAsync(this.ModuleColor);
            }
            #endregion

            #region config backup unexempt
            [Command("unexempt")]
            [Aliases("unex", "uex")]
            public async Task UnxemptAsync(CommandContext ctx,
                                          [Description("desc-unexempt-chn")] params DiscordChannel[] channels)
            {
                if (channels is null || !channels.Any())
                    throw new CommandFailedException(ctx, "cmd-err-exempt");

                await this.Service.UnexemptAsync(ctx.Guild.Id, this.SelectChildChannelIds());
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
