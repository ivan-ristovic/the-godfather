using System.Text;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.CommandsNext.Converters;
using DSharpPlus.CommandsNext.Entities;
using DSharpPlus.Entities;
using Microsoft.Extensions.DependencyInjection;
using TheGodfather.Services.Common;

namespace TheGodfather.Modules.Misc.Common;

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
        this.emb.WithLocalizedFooter(TranslationKey.h_footer, ctx.Client.CurrentUser.AvatarUrl);
    }


    public override CommandHelpMessage Build()
    {
        this.emb.WithColor(DiscordColor.SpringGreen);

        string desc = this.GetS(TranslationKey.h_desc_def);
        if (!string.IsNullOrWhiteSpace(this.name)) {
            this.emb.WithTitle(this.name);
            desc = string.IsNullOrWhiteSpace(this.desc) ? this.GetS(TranslationKey.h_desc_none) : this.desc;
        } else {
            this.emb.WithLocalizedTitle(TranslationKey.h_title);
        }
        this.emb.WithDescription(desc);

        return new CommandHelpMessage(embed: this.emb.Build());
    }

    public override BaseHelpFormatter WithCommand(Command cmd)
    {
        this.name = cmd is CommandGroup ? this.GetS(TranslationKey.fmt_group(cmd.QualifiedName)) : cmd.QualifiedName;
        CommandService cs = this.Context.Services.GetRequiredService<CommandService>();
        try {
            this.desc = cs.GetCommandDescription(this.GuildId, cmd.QualifiedName);
        } catch (Exception e) when (e is LocalizationException or KeyNotFoundException) {
            LogExt.Warning(this.Context, e, "Failed to find description for: {Command}", cmd.QualifiedName);
            this.desc = this.GetS(TranslationKey.h_desc_none);
        }

        if (cmd.Aliases?.Any() ?? false)
            this.emb.AddLocalizedField(TranslationKey.str_aliases, cmd.Aliases.Select(a => Formatter.InlineCode(a)).JoinWith(", "), true);

        this.emb.AddLocalizedField(TranslationKey.str_category, ModuleAttribute.AttachedTo(cmd).Module.ToString(), true);

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
            pb.AppendLine(Formatter.Bold(this.GetS(TranslationKey.str_owner_only)));
        if (checks.Any(chk => chk is RequirePrivilegedUserAttribute))
            pb.AppendLine(Formatter.Bold(this.GetS(TranslationKey.str_priv_only)));
        if (perms.Any())
            pb.AppendLine(Formatter.InlineCode(perms.JoinWith(", ")));
        if (uperms.Any())
            pb.Append(this.GetS(TranslationKey.str_perms_user)).Append(' ').AppendLine(Formatter.InlineCode(uperms.JoinWith(", ")));
        if (bperms.Any())
            pb.Append(this.GetS(TranslationKey.str_perms_bot)).Append(' ').AppendLine(Formatter.InlineCode(bperms.JoinWith(", ")));

        string pstr = pb.ToString();
        if (!string.IsNullOrWhiteSpace(pstr))
            this.emb.AddLocalizedField(TranslationKey.str_perms_req, pstr);

        if (cmd.Overloads?.Any() ?? false)
            foreach (CommandOverload overload in cmd.Overloads.OrderByDescending(o => o.Priority)) {
                var ab = new StringBuilder();

                foreach (CommandArgument arg in overload.Arguments) {
                    if (arg.IsOptional)
                        ab.Append(this.GetS(TranslationKey.str_optional)).Append(' ');

                    ab.Append("[`").Append(this.CommandsNext.GetUserFriendlyTypeName(arg.Type));
                    if (arg.IsCatchAll)
                        ab.Append("...");
                    ab.Append("`] ");

                    string desc = string.IsNullOrWhiteSpace(arg.Description) 
                        ? this.GetS(TranslationKey.h_desc_none) 
                        : this.lcs.GetStringUnsafe(this.GuildId, arg.Description);
                    ab.Append(Formatter.Bold(desc));

                    if (arg.IsOptional)
                        ab.Append(" (")
                            .Append(this.GetS(TranslationKey.str_def))
                            .Append(' ')
                            .Append(Formatter.InlineCode(arg.DefaultValue is null ? this.GetS(TranslationKey.str_none) : arg.DefaultValue.ToString()))
                            .Append(')');

                    ab.AppendLine();
                }

                string args = ab.Length > 0 ? ab.ToString() : this.GetS(TranslationKey.str_args_none);
                if (cmd.Overloads.Count > 1)
                    this.emb.AddLocalizedField(TranslationKey.fmt_overload(overload.Priority), args, true);
                else
                    this.emb.AddLocalizedField(TranslationKey.str_args, args, true);
            }

        if (cmd is CommandGroup { IsExecutableWithoutSubcommands: false })
            return this;

        this.emb.AddLocalizedField(TranslationKey.str_usage_examples, Formatter.BlockCode(cs.GetCommandUsageExamples(this.GuildId, cmd.QualifiedName).JoinWith()));
        return this;
    }

    public override BaseHelpFormatter WithSubcommands(IEnumerable<Command> subcommands)
    {
        if (subcommands.Any()) {
            TranslationKey title = string.IsNullOrWhiteSpace(this.name) ? TranslationKey.str_cmds : TranslationKey.str_subcmds;
            this.emb.AddLocalizedField(title, subcommands.Select(c => Formatter.InlineCode(c.Name)).JoinWith(", "));
        }
        return this;
    }


    private string GetS(TranslationKey key)
        => this.lcs.GetString(this.GuildId, key);
}