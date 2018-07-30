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
    internal sealed class CustomHelpFormatter : BaseHelpFormatter
    {
        #region PROPERTIES
        private DiscordEmbedBuilder EmbedBuilder { get; }
        private string Name { get; set; }
        private string Description { get; set; }
        #endregion


        public CustomHelpFormatter(CommandContext ctx) : base(ctx)
        {
            this.EmbedBuilder = new DiscordEmbedBuilder();
        }


        public override CommandHelpMessage Build()
        {
            this.EmbedBuilder.WithColor(DiscordColor.SpringGreen);

            string desc = $"Listing all commands and groups. Use {Formatter.InlineCode("!help <command>")} for detailed information.";
            if (!string.IsNullOrWhiteSpace(this.Name)) {
                this.EmbedBuilder.WithTitle(this.Name);
                desc = string.IsNullOrWhiteSpace(this.Description) ? "No description provided." : this.Description;
            } else {
                this.EmbedBuilder.WithTitle("Help");
            }
            this.EmbedBuilder.WithDescription(desc);
            this.EmbedBuilder.WithFooter("Detailed documentation @ https://github.com/ivan-ristovic/the-godfather");

            return new CommandHelpMessage(embed: this.EmbedBuilder);
        }

        public override BaseHelpFormatter WithCommand(Command cmd)
        {
            this.Name = cmd is CommandGroup ? $"Group: {cmd.QualifiedName}" : cmd.QualifiedName;
            this.Description = cmd.Description;

            this.EmbedBuilder.AddField("Module", ModuleAttribute.ForCommand(cmd).Module.ToString());

            if (cmd.Aliases?.Any() ?? false)
                this.EmbedBuilder.AddField("Aliases", string.Join(", ", cmd.Aliases.Select(a => Formatter.InlineCode(a))));

            var allchecks = cmd.ExecutionChecks.Union(cmd.Parent?.ExecutionChecks ?? Enumerable.Empty<CheckBaseAttribute>());
            var perms = allchecks.Where(chk => chk is RequirePermissionsAttribute)
                                 .Select(chk => chk as RequirePermissionsAttribute)
                                 .Select(chk => chk.Permissions.ToPermissionString());
            var uperms = allchecks.Where(chk => chk is RequireUserPermissionsAttribute)
                                  .Select(chk => chk as RequireUserPermissionsAttribute)
                                  .Select(chk => chk.Permissions.ToPermissionString());
            var bperms = allchecks.Where(chk => chk is RequireBotPermissionsAttribute)
                                  .Select(chk => chk as RequireBotPermissionsAttribute)
                                  .Select(chk => chk.Permissions.ToPermissionString());

            var pb = new StringBuilder();
            if (allchecks.Any(chk => chk is RequireOwnerAttribute))
                pb.AppendLine(Formatter.Bold("Owner-only."));
            if (perms.Any()) 
                pb.AppendLine(Formatter.InlineCode(string.Join(", ", perms)));
            if (uperms.Any()) 
                pb.Append(Formatter.Bold("User permissions: ")).AppendLine(Formatter.InlineCode(string.Join(", ", uperms)));
            if (bperms.Any()) 
                pb.Append(Formatter.Bold("Bot permissions: ")).AppendLine(Formatter.InlineCode(string.Join(", ", bperms)));

            string pstr = pb.ToString();
            if (!string.IsNullOrWhiteSpace(pstr))
                this.EmbedBuilder.AddField("Required permissions", pstr);

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
                            ab.Append(" (def: ").Append(Formatter.InlineCode(arg.DefaultValue != null ? arg.DefaultValue.ToString() : "None")).Append(")");

                        ab.AppendLine();
                    }

                    string args = ab.ToString();
                    this.EmbedBuilder.AddField($"{(cmd.Overloads.Count > 1 ? $"Overload #{overload.Priority}" : "Arguments")}" , string.IsNullOrWhiteSpace(args) ? "None": args, inline: true);
                }
            }

            var eb = new StringBuilder();
            if (cmd.CustomAttributes.FirstOrDefault(chk => chk is UsageExamplesAttribute) is UsageExamplesAttribute example) {
                eb.AppendLine("```");
                eb.AppendLine(example.JoinExamples());
                eb.AppendLine("```");
            }
            string estr = eb.ToString();
            if (!string.IsNullOrWhiteSpace(estr))
                this.EmbedBuilder.AddField("Examples of use", estr);

            return this;
        }

        public override BaseHelpFormatter WithSubcommands(IEnumerable<Command> subcommands)
        {
            if (subcommands.Any())
                this.EmbedBuilder.AddField(this.Name != null ? "Subcommands" : "Commands", string.Join(", ", subcommands.Select(c => Formatter.InlineCode(c.Name))));
            return this;
        }
    }
}
