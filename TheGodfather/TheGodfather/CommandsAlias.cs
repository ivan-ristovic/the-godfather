#region USING_DIRECTIVES
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
#endregion

namespace TheGodfatherBot
{
    [Description("Custom response commands.")]
    [RequirePermissions(Permissions.Administrator)]
    public class CommandsAlias
    {
        #region STATIC_FIELDS
        private static Dictionary<string, string> _aliases = new Dictionary<string, string>();
        #endregion

        #region STATIC_FUNCTIONS
        public static void LoadAliases()
        {
            if (File.Exists("aliases.txt")) {
                try {
                    var lines = File.ReadAllLines("aliases.txt");
                    foreach (string line in lines) {
                        if (line.Trim() == "" || line[0] == '#')
                            continue;
                        var values = line.Split('$');
                        _aliases.Add(values[0], values[1]);
                    }
                } catch (Exception) {
                    _aliases.Clear();
                    return;
                }
            }
        }

        public static string FindAlias(string trigger)
        {
            if (_aliases.ContainsKey(trigger))
                return _aliases[trigger];
            else
                return null;
        }
        #endregion

        #region COMMAND_ALIAS
        [Command("alias"), Description("Alias handling, usage: !a <aliasname> or !a add/del/save/clear <aliasname>.")]
        [Aliases("a")]
        public async Task AliasBaseHandle(CommandContext ctx, [RemainingTextAttribute, Description("args")] string cmd)
        {
            try {
                var split = cmd.Split(' ');
                switch (split[0]) {
                    case "add":  await AddAlias(ctx, split[1], split[2]);   break;
                    case "del":  await RemoveAlias(ctx, split[1]);          break;
                    case "save": await SaveAliases(ctx);                    break;
                    case "list": await ListAliases(ctx);                    break;
                    case "clear": await ClearAliases(ctx);                  break;
                    default:
                        if (_aliases != null && _aliases.ContainsKey(split[0]))
                            await ctx.RespondAsync(_aliases[split[0]]);
                        else
                            await ctx.RespondAsync("Unknown alias.");
                        break;
                }
            } catch (Exception) {
                await ctx.RespondAsync("Invalid command.");
            }
        }
        #endregion

        #region HELPER_FUNCTIONS
        private async Task AddAlias(CommandContext ctx, string alias, string response)
        {
            if (_aliases.ContainsKey(alias)) {
                await ctx.RespondAsync("Alias already exists.");
            } else {
                _aliases.Add(alias, response);
                await ctx.RespondAsync("Alias " + alias + " successfully added.");
            }
        }

        private async Task RemoveAlias(CommandContext ctx, string alias)
        {
            _aliases.Remove(alias);
            await ctx.RespondAsync("Alias " + alias + " successfully removed.");
        }

        private async Task SaveAliases(CommandContext ctx)
        {
            try {
                FileStream f = File.Open("aliases.txt", FileMode.Create);
                f.Close();

                List<string> aliaslist = new List<string> {
                "# Alias file",
                "# ",
                "# How to use it:",
                "# Aliases consist of a name and text to be written when alias is triggered.",
                "# Lines in this file contain each one alias in the format: name$reply",
                "# When triggered with '!a name', the bot will reply with 'reply'",
                ""
                };

                foreach (var entry in _aliases)
                    aliaslist.Add(entry.Key + "$" + entry.Value);

                File.WriteAllLines("aliases.txt", aliaslist);
            } catch (Exception) {
                await ctx.RespondAsync("Error while saving aliases.");
                return;
            }

            await ctx.RespondAsync("Aliases successfully saved.");
        }

        private async Task ListAliases(CommandContext ctx)
        {
            var embed = new DiscordEmbed() {
                Title = "Available aliases",
                Color = 0x00FF00    // Green
            };

            foreach (var entry in _aliases) {
                var item = new DiscordEmbedField() {
                    Name = entry.Key,
                    Value = entry.Value,
                    Inline = true
                };
                embed.Fields.Add(item);
            }
            await ctx.RespondAsync("", embed: embed);
        }

        private async Task ClearAliases(CommandContext ctx)
        {
            _aliases.Clear();
            await ctx.RespondAsync("All aliases successfully removed.");
        }
        #endregion
    }
}
