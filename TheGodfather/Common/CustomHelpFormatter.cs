#region USING_DIRECTIVES
using System.Collections.Generic;
using System.Linq;
using System.Text;

using TheGodfather.Common.Attributes;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.CommandsNext.Converters;
using DSharpPlus.CommandsNext.Entities;
using DSharpPlus.Entities;
#endregion

namespace TheGodfather.Common
{
    public class CustomHelpFormatter : BaseHelpFormatter
    {
        private DiscordEmbedBuilder _emb = new DiscordEmbedBuilder();
        private string _name;
        private string _desc;


        public CustomHelpFormatter(CommandContext ctx) : base(ctx) { }


        public override CommandHelpMessage Build()
        {
            _emb.WithColor(DiscordColor.SpringGreen);

            string desc = $"Listing all commands and groups. Use {Formatter.InlineCode("!help <command>")} for detailed information.";
            if (_name != null) {
                _emb.WithTitle(_name);
                desc = string.IsNullOrWhiteSpace(_desc) ? "No description provided." : _desc;
            } else {
                _emb.WithTitle("Help");
            }
            _emb.WithDescription(desc);
            _emb.WithFooter("Detailed documentation @ https://github.com/ivan-ristovic/the-godfather");

            return new CommandHelpMessage(embed: _emb);
        }

        public override BaseHelpFormatter WithCommand(Command command)
        {
            _name = command is CommandGroup ? $"Group: {command.QualifiedName}" : command.QualifiedName;
            _desc = command.Description;

            if (command.CustomAttributes.FirstOrDefault(a => a is ModuleAttribute) is ModuleAttribute modattr)
                _emb.AddField("Module", modattr.Module.ToString());

            if (command.Aliases?.Any() == true)
                _emb.AddField("Aliases", string.Join(", ", command.Aliases.Select(a => Formatter.InlineCode(a))));

            var allchecks = command.ExecutionChecks.Union(command.Parent?.ExecutionChecks ?? Enumerable.Empty<CheckBaseAttribute>());
            var permissions = allchecks.Where(chk => chk is RequirePermissionsAttribute)
                                       .Select(chk => chk as RequirePermissionsAttribute)
                                       .Select(chk => chk.Permissions.ToPermissionString());
            var userpermissions = allchecks.Where(chk => chk is RequireUserPermissionsAttribute)
                                           .Select(chk => chk as RequireUserPermissionsAttribute)
                                           .Select(chk => chk.Permissions.ToPermissionString());
            var botpermissions = allchecks.Where(chk => chk is RequireBotPermissionsAttribute)
                                          .Select(chk => chk as RequireBotPermissionsAttribute)
                                          .Select(chk => chk.Permissions.ToPermissionString());

            var pb = new StringBuilder();
            if (allchecks.Any(chk => chk is RequireOwnerAttribute))
                pb.AppendLine(Formatter.Bold("Owner-only."));
            if (permissions.Any()) 
                pb.AppendLine(Formatter.InlineCode(string.Join(", ", permissions)));
            if (userpermissions.Any()) 
                pb.Append(Formatter.Bold("User permissions: ")).AppendLine(Formatter.InlineCode(string.Join(", ", userpermissions)));
            if (botpermissions.Any()) 
                pb.Append(Formatter.Bold("Bot permissions: ")).AppendLine(Formatter.InlineCode(string.Join(", ", botpermissions)));

            string perms = pb.ToString();
            if (!string.IsNullOrWhiteSpace(perms))
                _emb.AddField("Required permissions",  perms);

            if (command.Overloads?.Any() == true) {
                foreach (var overload in command.Overloads.OrderByDescending(o => o.Priority)) {
                    var ab = new StringBuilder();

                    foreach (var arg in overload.Arguments) {
                        if (arg.IsOptional)
                            ab.Append("(optional) ");

                        string typestr = $"[{CommandsNext.GetUserFriendlyTypeName(arg.Type)}";
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
                    _emb.AddField($"{(command.Overloads.Count > 1 ? $"Overload #{overload.Priority}" : "Arguments")}" , string.IsNullOrWhiteSpace(args) ? "None": args, inline: true);
                }
            }

            var examples = command.CustomAttributes.Where(chk => chk is UsageExamplesAttribute)
                                                   .Select(chk => chk as UsageExamplesAttribute);
            var eb = new StringBuilder();
            if (examples.Any()) {
                eb.AppendLine("```");
                foreach (var example in examples)
                    eb.AppendLine(example.Example);
                eb.AppendLine("```");
            }
            string examplestr = eb.ToString();
            if (!string.IsNullOrWhiteSpace(examplestr))
                _emb.AddField("Examples of use", examplestr);

            return this;
        }

        public override BaseHelpFormatter WithSubcommands(IEnumerable<Command> subcommands)
        {
            if (subcommands.Any())
                _emb.AddField(_name != null ? "Subcommands" : "Commands", string.Join(", ", subcommands.Select(c => Formatter.InlineCode(c.Name))));
            return this;
        }
    }
}
