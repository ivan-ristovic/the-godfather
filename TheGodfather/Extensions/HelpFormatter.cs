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
        #region PRIVATE_FIELDS
        private DiscordEmbedBuilder _embed;
        private string _name, _desc;
        private bool _gexec;
        #endregion


        public HelpFormatter(CommandsNextExtension cnext) : base(cnext)
        {
            _embed = new DiscordEmbedBuilder();
            _name = null;
            _desc = null;
            _gexec = false;
        }


        public override CommandHelpMessage Build()
        {
            _embed.WithTitle("Help");
            _embed.WithColor(DiscordColor.SpringGreen);

            var desc = "Listing all commands and groups. Use ``!help <command>`` for detailed information.";
            if (_name != null) {
                var sb = new StringBuilder();
                sb.Append(_name)
                  .Append(": ")
                  .Append(string.IsNullOrWhiteSpace(_desc) ? "No description provided." : _desc);

                if (_gexec)
                    sb.AppendLine().AppendLine().Append("This group can be executed without subcommand.");

                desc = sb.ToString();
            }
            _embed.WithDescription(desc);
            _embed.WithFooter("Detailed info @ https://ivan-ristovic.github.io/the-godfather/");

            return new CommandHelpMessage(embed: _embed);
        }

        public override BaseHelpFormatter WithAliases(IEnumerable<string> aliases)
        {
            if (aliases.Any())
                _embed.AddField("Aliases", string.Join(", ", aliases.Select(a => Formatter.InlineCode(a))));
            return this;
        }

        public override BaseHelpFormatter WithArguments(IEnumerable<CommandArgument> arguments)
        {
            if (arguments.Any()) {
                var sb = new StringBuilder();

                foreach (var arg in arguments) {
                    if (arg.IsOptional)
                        sb.Append("(optional) ");

                    sb.Append($"{Formatter.InlineCode($"[{arg.Type}]")} ");

                    sb.Append(string.IsNullOrWhiteSpace(arg.Description) ? "No description provided." : Formatter.Bold(arg.Description));

                    if (arg.IsOptional)
                        sb.Append(" (def: ").Append(Formatter.InlineCode(arg.DefaultValue != null ? arg.DefaultValue.ToString() : "None")).Append(")");

                    sb.AppendLine();
                }

                _embed.AddField("Arguments", sb.ToString());
            }
            return this;
        }

        public override BaseHelpFormatter WithCommandName(string name)
        {
            _name = name;
            return this;
        }

        public override BaseHelpFormatter WithDescription(string description)
        {
            _desc = description;
            return this;
        }

        public override BaseHelpFormatter WithGroupExecutable()
        {
            _gexec = true;
            return this;
        }

        public override BaseHelpFormatter WithSubcommands(IEnumerable<Command> subcommands)
        {
            if (subcommands.Any())
                _embed.AddField(_name != null ? "Subcommands" : "Commands", string.Join(", ", subcommands.Select(c => Formatter.InlineCode(c.Name))));
            return this;
        }
    }
}
