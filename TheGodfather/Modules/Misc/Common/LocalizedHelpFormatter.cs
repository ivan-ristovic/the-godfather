using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.CommandsNext.Converters;
using DSharpPlus.CommandsNext.Entities;
using DSharpPlus.Entities;
using Microsoft.Extensions.DependencyInjection;
using TheGodfather.Attributes;
using TheGodfather.Exceptions;
using TheGodfather.Extensions;
using TheGodfather.Services;
using TheGodfather.Services.Common;

namespace TheGodfather.Modules.Misc.Common
{
    public class LocalizedHelpFormatter : BaseHelpFormatter
    {
        private ulong GuildId => this.Context.Guild?.Id ?? 0;

        private string? name;
        private string? desc;
        private readonly LocalizedEmbedBuilder emb;
        private readonly LocalizationService lcs;


        public LocalizedHelpFormatter(CommandContext ctx)
            : base(ctx)
        {
            this.lcs = ctx.Services.GetRequiredService<LocalizationService>();
            this.emb = new LocalizedEmbedBuilder(this.lcs, this.GuildId);
            this.emb.WithLocalizedFooter("h-footer", ctx.Client.CurrentUser.AvatarUrl);
        }


        public override CommandHelpMessage Build()
        {
            this.emb.WithColor(DiscordColor.SpringGreen);

            string desc = this.GetS("h-desc-def");
            if (!string.IsNullOrWhiteSpace(this.name)) {
                this.emb.WithTitle(this.name);
                desc = string.IsNullOrWhiteSpace(this.desc) ? this.GetS("h-desc-none") : this.desc;
            } else {
                this.emb.WithLocalizedTitle("h-title");
            }
            this.emb.WithDescription(desc);

            return new CommandHelpMessage(embed: this.emb.Build());
        }

        public override BaseHelpFormatter WithCommand(Command cmd)
        {
            this.name = cmd is CommandGroup ? this.GetS("fmt-group", cmd.QualifiedName) : cmd.QualifiedName;
            CommandService cs = this.Context.Services.GetRequiredService<CommandService>();
            try {
                this.desc = cs.GetCommandDescription(this.GuildId, cmd.QualifiedName);
            } catch (Exception e) when (e is LocalizationException or KeyNotFoundException) {
                LogExt.Warning(this.Context, e, "Failed to find description for: {Command}", cmd.QualifiedName);
                this.desc = this.GetS("h-desc-none");
            }

            if (cmd.Aliases?.Any() ?? false)
                this.emb.AddLocalizedTitleField("str-aliases", cmd.Aliases.Select(a => Formatter.InlineCode(a)).JoinWith(", "), inline: true);

            this.emb.AddLocalizedTitleField("str-category", ModuleAttribute.AttachedTo(cmd).Module.ToString(), inline: true);

            IEnumerable<CheckBaseAttribute> checks = cmd.ExecutionChecks
                .Union(cmd.Parent?.ExecutionChecks ?? Enumerable.Empty<CheckBaseAttribute>());
            IEnumerable<string> perms = checks
                .OfType<RequirePermissionsAttribute>()
                .Select(chk => chk.Permissions.ToPermissionString())
                .Union(checks
                    .OfType<RequirePermissionsAttribute>()
                    .Select(chk => chk.Permissions.ToPermissionString())
                );
            IEnumerable<string> uperms = checks
                .OfType<RequireUserPermissionsAttribute>()
                .Select(chk => chk.Permissions.ToPermissionString());
            IEnumerable<string> bperms = checks
                .OfType<RequireBotPermissionsAttribute>()
                .Select(chk => chk.Permissions.ToPermissionString());

            var pb = new StringBuilder();
            if (checks.Any(chk => chk is RequireOwnerAttribute))
                pb.AppendLine(Formatter.Bold(this.GetS("str-owner-only")));
            if (checks.Any(chk => chk is RequirePrivilegedUserAttribute))
                pb.AppendLine(Formatter.Bold(this.GetS("str-priv-only")));
            if (perms.Any())
                pb.AppendLine(Formatter.InlineCode(perms.JoinWith(", ")));
            if (uperms.Any())
                pb.Append(this.GetS("str-perms-user")).Append(' ').AppendLine(Formatter.InlineCode(uperms.JoinWith(", ")));
            if (bperms.Any())
                pb.Append(this.GetS("str-perms-bot")).Append(' ').AppendLine(Formatter.InlineCode(bperms.JoinWith(", ")));

            string pstr = pb.ToString();
            if (!string.IsNullOrWhiteSpace(pstr))
                this.emb.AddLocalizedTitleField("str-perms-req", pstr);

            if (cmd.Overloads?.Any() ?? false) {
                foreach (CommandOverload overload in cmd.Overloads.OrderByDescending(o => o.Priority)) {
                    var ab = new StringBuilder();

                    foreach (CommandArgument arg in overload.Arguments) {
                        if (arg.IsOptional)
                            ab.Append(this.GetS("str-optional")).Append(' ');

                        ab.Append($"[`{this.CommandsNext.GetUserFriendlyTypeName(arg.Type)}"));
                        if (arg.IsCatchAll)
                            ab.Append("...");
                        ab.Append("`] ");

                        ab.Append(Formatter.Bold(this.GetS(string.IsNullOrWhiteSpace(arg.Description) ? "h-desc-none" : arg.Description)));

                        if (arg.IsOptional) {
                            ab.Append(" (")
                              .Append(this.GetS("str-def"))
                              .Append(' ')
                              .Append(Formatter.InlineCode(arg.DefaultValue is null ? this.GetS("str-none") : arg.DefaultValue.ToString()))
                              .Append(')');
                        }

                        ab.AppendLine();
                    }

                    string args = ab.Length > 0 ? ab.ToString() : this.GetS("str-args-none");
                    if (cmd.Overloads.Count > 1)
                        this.emb.AddLocalizedTitleField("str-overload", args, inline: true, titleArgs: overload.Priority);
                    else
                        this.emb.AddLocalizedTitleField("str-args", args, inline: true, titleArgs: overload.Priority);
                }
            }

            if (cmd is CommandGroup { IsExecutableWithoutSubcommands: true })
                this.emb.AddLocalizedTitleField("str-usage-examples", Formatter.BlockCode(cs.GetCommandUsageExamples(this.GuildId, cmd.QualifiedName).JoinWith()));

            return this;
        }

        public override BaseHelpFormatter WithSubcommands(IEnumerable<Command> subcommands)
        {
            if (subcommands.Any()) {
                string title = string.IsNullOrWhiteSpace(this.name) ? "str-cmds" : "str-subcmds";
                this.emb.AddLocalizedTitleField(title, subcommands.Select(c => Formatter.InlineCode(c.Name)).JoinWith(", "));
            }
            return this;
        }


        private string GetS(string key, params object?[]? args)
            => this.lcs.GetString(this.GuildId, key, args);
    }
}
