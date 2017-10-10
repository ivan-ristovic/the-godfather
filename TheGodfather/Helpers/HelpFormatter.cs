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

namespace TheGodfather.Helpers
{
    public class HelpFormatter : IHelpFormatter
    {
        #region PRIVATE_FIELDS
        private DiscordEmbedBuilder _embed;
        private string _name, _desc;
        private bool _gexec;
        #endregion


        public HelpFormatter()
        {
            _embed = new DiscordEmbedBuilder();
            _name = null;
            _desc = null;
            _gexec = false;
        }


        public CommandHelpMessage Build()
        {
            _embed.Title = "Help";
            _embed.Color = DiscordColor.Azure;

            var desc = "Listing all top-level commands and groups. Use ``!help <command>`` to see more information.";
            if (_name != null) {
                var sb = new StringBuilder();
                sb.Append(_name)
                  .Append(": ")
                  .Append(string.IsNullOrWhiteSpace(_desc) ? "No description provided." : _desc);

                if (_gexec)
                    sb.AppendLine().AppendLine().Append("This group can be executed without subcommand.");

                desc = sb.ToString();
            }
            _embed.Description = desc;

            return new CommandHelpMessage(embed: _embed);
        }

        public IHelpFormatter WithAliases(IEnumerable<string> aliases)
        {
            if (aliases.Any())
                _embed.AddField("Aliases", string.Join("\n", aliases), false);
            return this;
        }

        public IHelpFormatter WithArguments(IEnumerable<CommandArgument> arguments)
        {
            if (arguments.Any()) {
                var sb = new StringBuilder();

                foreach (var arg in arguments) {

                    sb.Append($"{Formatter.Bold(arg.Name)}");

                    if (arg.IsOptional && arg.DefaultValue != null)
                        sb.Append(" (optional) ");

                    sb.Append($" [type: {arg.Type.ToUserFriendlyName()}] Description: ");

                    sb.Append(string.IsNullOrWhiteSpace(arg.Description) ? "No description provided." : Formatter.Bold(arg.Description));

                    if (arg.IsOptional && arg.DefaultValue != null)
                        sb.Append(" Default value: ").Append(arg.DefaultValue);

                    sb.AppendLine();
                }
                _embed.AddField("Arguments", sb.ToString(), false);
            }
            return this;
        }

        public IHelpFormatter WithCommandName(string name)
        {
            _name = name;
            return this;
        }

        public IHelpFormatter WithDescription(string description)
        {
            _desc = description;
            return this;
        }

        public IHelpFormatter WithGroupExecutable()
        {
            _gexec = true;
            return this;
        }

        public IHelpFormatter WithSubcommands(IEnumerable<Command> subcommands)
        {
            if (subcommands.Any())
                _embed.AddField(_name != null ? "Subcommands" : "Commands", string.Join(", ", subcommands.Select(c => c.Name)), false);
            return this;
        }
    }
}
