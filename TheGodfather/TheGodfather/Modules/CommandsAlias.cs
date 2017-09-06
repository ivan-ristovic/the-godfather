#region USING_DIRECTIVES
using System;
using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
#endregion

namespace TheGodfatherBot
{
    [Group("alias", CanInvokeWithoutSubcommand = true)]
    [Description("Alias handling commands.")]
    [Aliases("a")]
    [RequirePermissions(Permissions.ManageMessages)]
    public class CommandsAlias
    {
        #region STATIC_FIELDS
        private static Dictionary<ulong, Dictionary<string, string>> _aliases = new Dictionary<ulong, Dictionary<string, string>>();
        #endregion

        #region STATIC_FUNCTIONS
        public static void LoadAliases(DebugLogger log)
        {
            if (File.Exists("aliases.txt")) {
                try {
                    var lines = File.ReadAllLines("aliases.txt");
                    foreach (string line in lines) {
                        if (line.Trim() == "" || line[0] == '#')
                            continue;
                        var values = line.Split('$');
                        ulong gid = ulong.Parse(values[0]);
                        if (!_aliases.ContainsKey(gid))
                            _aliases.Add(gid, new Dictionary<string, string>());
                        _aliases[gid].Add(values[1], values[2]);
                    }
                } catch (Exception e) {
                    log.LogMessage(LogLevel.Warning, "TheGodfather", "Exception occured, clearing aliases. Details : " + e.ToString(), DateTime.Now);
                    _aliases.Clear();
                }
            } else {
                log.LogMessage(LogLevel.Warning, "TheGodfather", "aliases.txt is missing.", DateTime.Now);
            }
        }

        public static string FindAlias(ulong gid, string trigger)
        {
            trigger = trigger.ToLower();
            if (_aliases.ContainsKey(gid) && _aliases[gid].ContainsKey(trigger))
                return _aliases[gid][trigger];
            else
                return null;
        }
        #endregion

        
        public async Task ExecuteGroup(CommandContext ctx, [RemainingText, Description("Alias name.")] string name = null)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new Exception("Alias name is missing.");
            
            if (_aliases != null && _aliases.ContainsKey(ctx.Guild.Id) && _aliases[ctx.Guild.Id].ContainsKey(name))
                await ctx.RespondAsync(_aliases[ctx.Guild.Id][name]);
            else
                await ctx.RespondAsync("Unknown alias.");
        }
        

        #region COMMAND_ALIAS_ADD
        [Command("add")]
        [Description("Add alias to list.")]
        public async Task AddAlias(CommandContext ctx,
                                 [Description("Alias name (case sensitive).")] string alias = null,
                                 [Description("Response")] string response = null)
        {
            if (string.IsNullOrWhiteSpace(alias) || string.IsNullOrWhiteSpace(response))
                throw new Exception("Alias name or response missing or invalid.");

            if (!_aliases.ContainsKey(ctx.Guild.Id))
                _aliases.Add(ctx.Guild.Id, new Dictionary<string, string>());

            alias = alias.ToLower();
            if (_aliases[ctx.Guild.Id].ContainsKey(alias)) {
                await ctx.RespondAsync("Alias already exists.");
            } else {
                _aliases[ctx.Guild.Id].Add(alias, response);
                await ctx.RespondAsync("Alias " + alias + " successfully added.");
            }
        }
        #endregion

        #region COMMAND_ALIAS_DELETE
        [Command("delete")]
        [Description("Remove alias from list.")]
        [Aliases("remove", "del")]
        public async Task DeleteAlias(CommandContext ctx, [Description("Alias to remove.")] string alias = null)
        {
            if (string.IsNullOrWhiteSpace(alias))
                throw new Exception("Alias name missing.");

            if (!_aliases.ContainsKey(ctx.Guild.Id))
                throw new Exception("No aliases recorded in this guild.");

            _aliases[ctx.Guild.Id].Remove(alias);
            await ctx.RespondAsync($"Alias '{alias}' successfully removed.");
        }
        #endregion

        #region COMMAND_ALIAS_SAVE
        [Command("save")]
        [Description("Save aliases to file.")]
        [RequireOwner]
        public async Task SaveAliases(CommandContext ctx)
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

                foreach (var guild_aliases in _aliases)
                    foreach (var alias in guild_aliases.Value)
                        aliaslist.Add(guild_aliases.Key + "$" + alias.Key + "$" + alias.Value);

                File.WriteAllLines("aliases.txt", aliaslist);
            } catch (Exception e) {
                ctx.Client.DebugLogger.LogMessage(LogLevel.Error, "TheGodfather", "IO Alias save error:" + e.ToString(), DateTime.Now);
                throw new InvalidDataException("IO error while saving aliases.");
            }

            await ctx.RespondAsync("Aliases successfully saved.");
        }
        #endregion

        #region COMMAND_ALIAS_LIST
        [Command("list")]
        [Description("Show all aliases.")]
        public async Task ListAliases(CommandContext ctx)
        {
            if (!_aliases.ContainsKey(ctx.Guild.Id)) {
                await ctx.RespondAsync("No aliases registered.");
                return;
            }

            var embed = new DiscordEmbed() {
                Title = "Available aliases",
                Color = 0x00FF00    // Green
            };

            foreach (var alias in _aliases[ctx.Guild.Id]) {
                var item = new DiscordEmbedField() {
                    Name = alias.Key,
                    Value = alias.Value,
                    Inline = true
                };
                embed.Fields.Add(item);
            }
            await ctx.RespondAsync("", embed: embed);
        }
        #endregion

        #region COMMAND_ALIAS_CLEAR
        [Command("clear")]
        [Description("Delete all aliases.")]
        [RequireOwner]
        public async Task ClearAliases(CommandContext ctx)
        {
            if (_aliases.ContainsKey(ctx.Guild.Id))
                _aliases[ctx.Guild.Id].Clear();
            await ctx.RespondAsync("All aliases successfully removed.");
        }
        #endregion
    }
}
