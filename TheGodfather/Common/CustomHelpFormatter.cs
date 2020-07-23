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
using TheGodfather.Extensions;
using TheGodfather.Services;

namespace TheGodfather.Common
{
    // TODO localize and improve

    public sealed class CustomHelpFormatter : BaseHelpFormatter
    {
        private string? name;
        private string? desc;
        private readonly DiscordEmbedBuilder emb;


        public CustomHelpFormatter(CommandContext ctx) 
            : base(ctx)
        {
            this.emb = new DiscordEmbedBuilder();
            this.emb.WithFooter("Detailed documentation @ https://github.com/ivan-ristovic/the-godfather", ctx.Client.CurrentUser.AvatarUrl);
        }


        public override CommandHelpMessage Build()
        {
            this.emb.WithColor(DiscordColor.SpringGreen);

            string desc = $"Listing all commands and groups. Use command {Formatter.InlineCode("help <command>")} for detailed information about the given command.";
            if (!string.IsNullOrWhiteSpace(this.name)) {
                this.emb.WithTitle(this.name);
                desc = string.IsNullOrWhiteSpace(this.desc) ? "No description provided." : this.desc;
            } else {
                this.emb.WithTitle("Help");
            }
            this.emb.WithDescription(desc);

            return new CommandHelpMessage(embed: this.emb);
        }

        public override BaseHelpFormatter WithCommand(Command cmd)
        {
            this.name = cmd is CommandGroup ? $"Group: {cmd.QualifiedName}" : cmd.QualifiedName;
            LocalizationService localization = this.Context.Services.GetService<LocalizationService>();
            try {
                this.desc = localization.GetCommandDescription(this.Context.Guild.Id, cmd.QualifiedName);
            } catch (KeyNotFoundException e) {
                LogExt.Warning(this.Context, e, "Failed to find description for: {Command}", cmd.QualifiedName);
            }

            if (cmd.Aliases?.Any() ?? false)
                this.emb.AddField("Aliases", string.Join(", ", cmd.Aliases.Select(a => Formatter.InlineCode(a))), inline: true);

            this.emb.AddField("Category", ModuleAttribute.AttachedTo(cmd).Module.ToString(), inline: true);

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
                pb.AppendLine(Formatter.Bold("Owner-only."));
            if (checks.Any(chk => chk is RequirePrivilegedUserAttribute))
                pb.AppendLine(Formatter.Bold("Privileged users only."));
            if (perms.Any())
                pb.AppendLine(Formatter.InlineCode(string.Join(", ", perms)));
            if (uperms.Any())
                pb.Append(Formatter.Bold("User permissions: ")).AppendLine(Formatter.InlineCode(string.Join(", ", uperms)));
            if (bperms.Any())
                pb.Append(Formatter.Bold("Bot permissions: ")).AppendLine(Formatter.InlineCode(string.Join(", ", bperms)));

            string pstr = pb.ToString();
            if (!string.IsNullOrWhiteSpace(pstr))
                this.emb.AddField("Required permissions", pstr);

            if (cmd.Overloads?.Any() ?? false) {
                foreach (CommandOverload overload in cmd.Overloads.OrderByDescending(o => o.Priority)) {
                    var ab = new StringBuilder();

                    foreach (CommandArgument arg in overload.Arguments) {
                        if (arg.IsOptional)
                            ab.Append("(optional) ");

                        string typestr = $"[{this.CommandsNext.GetUserFriendlyTypeName(arg.Type)}";
                        if (arg.IsCatchAll)
                            typestr += "...";
                        typestr += "]";

                        ab.Append(Formatter.InlineCode(typestr));
                        ab.Append(" ");

                        ab.Append(string.IsNullOrWhiteSpace(arg.Description) ? "No description provided." : Formatter.Bold(arg.Description));

                        if (arg.IsOptional)
                            ab.Append(" (def: ").Append(Formatter.InlineCode(arg.DefaultValue is null ? "None" : arg.DefaultValue.ToString())).Append(")");

                        ab.AppendLine();
                    }

                    string args = ab.ToString();
                    this.emb.AddField($"{(cmd.Overloads.Count > 1 ? $"Overload #{overload.Priority}" : "Arguments")}", string.IsNullOrWhiteSpace(args) ? "No arguments." : args, inline: true);
                }
            }

            this.emb.AddField("Examples of use", Formatter.BlockCode(string.Join("\n", localization.GetCommandUsageExamples(this.Context.Guild.Id, cmd.QualifiedName))));

            return this;
        }

        public override BaseHelpFormatter WithSubcommands(IEnumerable<Command> subcommands)
        {
            if (subcommands.Any())
                this.emb.AddField(this.name is null ? "Commands" : "Subcommands", string.Join(", ", subcommands.Select(c => Formatter.InlineCode(c.Name))));
            return this;
        }
    }
}
