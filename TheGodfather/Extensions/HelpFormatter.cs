#region USING_DIRECTIVES
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Converters;
using DSharpPlus.CommandsNext.Entities;
using DSharpPlus.Entities;
#endregion

namespace TheGodfather.Extensions
{
    public class HelpFormatter : BaseHelpFormatter
    {
        private DiscordEmbedBuilder _emb = new DiscordEmbedBuilder();
        private string _name;
        private string _desc;


        public HelpFormatter(CommandsNextExtension cnext) : base(cnext) { }


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
            _emb.WithFooter("Detailed info @ https://ivan-ristovic.github.io/the-godfather/");

            return new CommandHelpMessage(embed: _emb);
        }

        public override BaseHelpFormatter WithCommand(Command command)
        {
            _name = command.QualifiedName;
            _desc = command.Description;

            if (command.Aliases?.Any() == true)
                _emb.AddField("Aliases", string.Join(", ", command.Aliases.Select(a => Formatter.InlineCode(a))));

            if (command.Overloads?.Any() == true) {
                foreach (var overload in command.Overloads.OrderByDescending(x => x.Priority)) {
                    var sb = new StringBuilder();

                    foreach (var arg in overload.Arguments) {
                        if (arg.IsOptional)
                            sb.Append("(optional) ");

                        sb.Append($"{Formatter.InlineCode($"[{CommandsNext.GetUserFriendlyTypeName(arg.Type)}]")} ");

                        sb.Append(string.IsNullOrWhiteSpace(arg.Description) ? "No description provided." : Formatter.Bold(arg.Description));

                        if (arg.IsOptional)
                            sb.Append(" (def: ").Append(Formatter.InlineCode(arg.DefaultValue != null ? arg.DefaultValue.ToString() : "None")).Append(")");

                        sb.AppendLine();
                    }

                    _emb.AddField($"{(command.Overloads.Count > 1 ? $"Overload #{overload.Priority}" : "Arguments")}" , sb.ToString());
                }
            }

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
