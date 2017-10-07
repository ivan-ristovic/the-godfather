#region USING_DIRECTIVES
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;

using TheGodfatherBot.Exceptions;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
#endregion

namespace TheGodfatherBot.Commands.Messages
{
    [Group("reactions", CanInvokeWithoutSubcommand = false /*true*/)]
    [Description("Reaction handling commands.")]
    [Aliases("react", "reaction")]
    [Cooldown(2, 3, CooldownBucketType.User), Cooldown(5, 3, CooldownBucketType.Channel)]
    public class CommandsReaction
    {
        #region STATIC_FIELDS
        private static SortedDictionary<ulong, SortedDictionary<string, string>> _reactions = new SortedDictionary<ulong, SortedDictionary<string, string>>();
        private static bool _error = false;
        #endregion

        #region STATIC_FUNCTIONS
        public static void LoadReactions(DebugLogger log)
        {
            if (File.Exists("Resources/reactions.json")) {
                try {
                    _reactions = JsonConvert.DeserializeObject<SortedDictionary<ulong, SortedDictionary<string, string>>>(File.ReadAllText("Resources/reactions.json"));
                } catch (Exception e) {
                    log.LogMessage(LogLevel.Error, "TheGodfather", "Reaction loading error, check file formatting. Details:\n" + e.ToString(), DateTime.Now);
                    _error = true;
                }
            } else {
                log.LogMessage(LogLevel.Warning, "TheGodfather", "reactions.json is missing.", DateTime.Now);
            }
        }

        public static void SaveReactions(DebugLogger log)
        {
            if (_error) {
                log.LogMessage(LogLevel.Warning, "TheGodfather", "Reactions saving skipped until file conflicts are resolved!", DateTime.Now);
                return;
            }

            try {
                File.WriteAllText("Resources/reactions.json", JsonConvert.SerializeObject(_reactions));
            } catch (Exception e) {
                log.LogMessage(LogLevel.Error, "TheGodfather", "Reactions save error. Details:\n" + e.ToString(), DateTime.Now);
                throw new IOException("IO error while saving reactions.");
            }
        }

        public static List<DiscordEmoji> GetReactionEmojis(DiscordClient cl, ulong gid, string message)
        {
            var emojis = new List<DiscordEmoji>();
            
            if (_reactions.ContainsKey(gid)) {
                foreach (var word in message.ToLower().Split(' ')) {
                    if (_reactions[gid].ContainsKey(word)) {
                        try {
                            emojis.Add(DiscordEmoji.FromName(cl, _reactions[gid][word]));
                        } catch (ArgumentException) {
                            cl.DebugLogger.LogMessage(LogLevel.Error, "TheGodfather", "Emoji name is not valid!", DateTime.Now);
                        }
                    }
                }
            }
            
            return emojis;
        }
        #endregion

        /*
        public async Task ExecuteGroupAsync(CommandContext ctx,
                                           [Description("Emoji to send.")] DiscordEmoji emoji = null,
                                           [RemainingText, Description("Trigger word list.")] params string[] triggers)
        {
            await AddReaction(ctx, emoji, triggers);
        }
        */

        /*
        #region COMMAND_REACTIONS_ADD
        [Command("add")]
        [Description("Add reactions to list.")]
        [Aliases("+", "new")]
        [RequireUserPermissions(Permissions.ManageMessages)]
        public async Task AddReaction(CommandContext ctx, 
                                     [Description("Emoji to send.")] DiscordEmoji emoji = null,
                                     [RemainingText, Description("Trigger word list.")] params string[] triggers)
        {
            if (!_reactions.ContainsKey(ctx.Guild.Id))
                _reactions.Add(ctx.Guild.Id, new SortedDictionary<string, string>());

            bool conflict_exists = false;
            foreach (var word in triggers) {
                if (_reactions[ctx.Guild.Id].ContainsKey(word))
                    conflict_exists = true;
                else
                    _reactions[ctx.Guild.Id].Add(word, $":{emoji.Name}:");
            }

            if (conflict_exists)
                await ctx.RespondAsync("Done. Some triggers were already present in list so I skipped those.");
            else
                await ctx.RespondAsync("Done."); 
        }
        #endregion
        */
        #region COMMAND_REACTIONS_DELETE
        [Command("delete")]
        [Description("Remove trigger word (can be more than one) from list.")]
        [Aliases("-", "remove", "del")]
        [RequireUserPermissions(Permissions.ManageMessages)]
        public async Task DeleteReaction(CommandContext ctx,
                                        [RemainingText, Description("Trigger word list.")] params string[] triggers)
        {
            if (!_reactions.ContainsKey(ctx.Guild.Id))
                throw new CommandFailedException("No reactions recorded in this guild.", new KeyNotFoundException());

            bool not_found = false;
            foreach (var trigger in triggers) {
                if (!_reactions[ctx.Guild.Id].ContainsKey(trigger))
                    not_found = true;
                else
                    _reactions[ctx.Guild.Id].Remove(trigger);
            }

            if (not_found)
                await ctx.RespondAsync("Done. Some triggers were not in list anyway though.");
            else
                await ctx.RespondAsync("Done.");
        }
        #endregion

        #region COMMAND_REACTONS_SAVE
        [Command("save")]
        [Description("Save reactions to file.")]
        [RequireOwner]
        public async Task SaveReactions(CommandContext ctx)
        {
            SaveReactions(ctx.Client.DebugLogger);
            await ctx.RespondAsync("Aliases successfully saved.");
        }
        #endregion

        #region COMMAND_REACTIONS_LIST
        [Command("list")]
        [Description("Show all reactions.")]
        public async Task ListReactions(CommandContext ctx,
                                       [Description("Page.")] int page = 1)
        {
            if (!_reactions.ContainsKey(ctx.Guild.Id)) {
                await ctx.RespondAsync("No reactions registered.");
                return;
            }

            if (page < 1 || page > _reactions[ctx.Guild.Id].Count / 10 + 1)
                throw new CommandFailedException("No reactions on that page.");

            string s = "";
            int starti = (page - 1) * 10;
            int endi = starti + 10 < _reactions[ctx.Guild.Id].Count ? starti + 10 : _reactions[ctx.Guild.Id].Count;
            var keys = _reactions[ctx.Guild.Id].Keys.Take(page * 10).ToArray();
            for (var i = starti; i < endi; i++)
                s += $"{Formatter.Bold(keys[i])} : {_reactions[ctx.Guild.Id][keys[i]]}\n";

            await ctx.RespondAsync("", embed: new DiscordEmbedBuilder() {
                Title = $"Available reactions (page {page}/{_reactions[ctx.Guild.Id].Count / 10 + 1}) :",
                Description = s,
                Color = DiscordColor.Yellow
            });
        }
        #endregion

        #region COMMAND_REACTIONS_CLEAR
        [Command("clear")]
        [Description("Delete all reactions for the current guild.")]
        [RequireUserPermissions(Permissions.Administrator)]
        public async Task ClearReactions(CommandContext ctx)
        {
            if (_reactions.ContainsKey(ctx.Guild.Id))
                _reactions.Remove(ctx.Guild.Id);
            await ctx.RespondAsync("All reactions successfully removed.");
        }
        #endregion

        #region COMMAND_REACTIONS_CLEARALL
        [Command("clearall")]
        [Description("Delete all reactions stored for ALL guilds.")]
        [RequireOwner]
        public async Task ClearAllReactions(CommandContext ctx)
        {
            _reactions.Clear();
            await ctx.RespondAsync("All reactions successfully removed.");
        }
        #endregion
    }
}
