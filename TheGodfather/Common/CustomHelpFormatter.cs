#region USING_DIRECTIVES
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.CommandsNext.Converters;
using DSharpPlus.CommandsNext.Entities;
using DSharpPlus.Entities;

using System.Collections.Generic;
using System.Linq;
using System.Text;

using TheGodfather.Common.Attributes;
#endregion

namespace TheGodfather.Common
{
    public sealed class CustomHelpFormatter : BaseHelpFormatter
    {
        private string name;
        private string description;
        private readonly DiscordEmbedBuilder emb;


        public CustomHelpFormatter(CommandContext ctx) : base(ctx)
        {
            this.emb = new DiscordEmbedBuilder();
            this.emb.WithFooter("Detailed documentation @ https://github.com/ivan-ristovic/the-godfather", ctx.Client.CurrentUser.AvatarUrl);
        }


        public override CommandHelpMessage Build()
        {
            this.emb.WithColor(DiscordColor.SpringGreen);

            string desc = $"Listing all commands and groups. Use {Formatter.InlineCode("!help <command>")} for detailed information.";
            if (!string.IsNullOrWhiteSpace(this.name)) {
                this.emb.WithTitle(this.name);
                desc = string.IsNullOrWhiteSpace(this.description) ? "No description provided." : this.description;
            } else {
                this.emb.WithTitle("Help");
            }
            this.emb.WithDescription(desc);

            return new CommandHelpMessage(embed: this.emb);
        }

        public override BaseHelpFormatter WithCommand(Command cmd)
        {
            this.name = cmd is CommandGroup ? $"Group: {cmd.QualifiedName}" : cmd.QualifiedName;
            this.description = cmd.Description;

            if (cmd.Aliases?.Any() ?? false)
                this.emb.AddField("Aliases", string.Join(", ", cmd.Aliases.Select(a => Formatter.InlineCode(a))), inline: true);

            this.emb.AddField("Category", ModuleAttribute.ForCommand(cmd).Module.ToString(), inline: true);

            var checks = cmd.ExecutionChecks.Union(cmd.Parent?.ExecutionChecks ?? Enumerable.Empty<CheckBaseAttribute>());
            var perms = checks
                .Where(chk => chk is RequirePermissionsAttribute)
                .Select(chk => chk as RequirePermissionsAttribute)
                .Select(chk => chk.Permissions.ToPermissionString())
                .Union(checks
                    .Where(chk => chk is RequireOwnerOrPermissionsAttribute)
                    .Select(chk => chk as RequireOwnerOrPermissionsAttribute)
                    .Select(chk => chk.Permissions.ToPermissionString())
                );
            var uperms = checks.Where(chk => chk is RequireUserPermissionsAttribute)
                                  .Select(chk => chk as RequireUserPermissionsAttribute)
                                  .Select(chk => chk.Permissions.ToPermissionString());
            var bperms = checks.Where(chk => chk is RequireBotPermissionsAttribute)
                                  .Select(chk => chk as RequireBotPermissionsAttribute)
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
                foreach (var overload in cmd.Overloads.OrderByDescending(o => o.Priority)) {
                    var ab = new StringBuilder();

                    foreach (var arg in overload.Arguments) {
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
                    this.emb.AddField($"{(cmd.Overloads.Count > 1 ? $"Overload #{overload.Priority}" : "Arguments")}" , string.IsNullOrWhiteSpace(args) ? "No arguments.": args, inline: true);
                }
            }

            if (cmd.CustomAttributes.FirstOrDefault(chk => chk is UsageExamplesAttribute) is UsageExamplesAttribute example)
                this.emb.AddField("Examples of use", Formatter.BlockCode(example.JoinExamples()));                

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
