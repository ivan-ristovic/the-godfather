using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using Microsoft.EntityFrameworkCore;
using Serilog;
using TheGodfather.Common.Collections;
using TheGodfather.Database;
using TheGodfather.Database.Entities;
using TheGodfather.Exceptions;
using TheGodfather.Modules.Reactions.Common;
using TheGodfather.Services;

namespace TheGodfather.Modules.Reactions.Services
{
    public sealed class ReactionsService : ITheGodfatherService
    {
        public bool IsDisabled => false;

        private readonly DatabaseContextBuilder dbb;
        private ConcurrentDictionary<ulong, ConcurrentHashSet<EmojiReaction>> ereactions;
        private ConcurrentDictionary<ulong, ConcurrentHashSet<TextReaction>> treactions;


        public ReactionsService(DatabaseContextBuilder dbb, bool loadData = true)
        {
            this.dbb = dbb;
            this.ereactions = new ConcurrentDictionary<ulong, ConcurrentHashSet<EmojiReaction>>();
            this.treactions = new ConcurrentDictionary<ulong, ConcurrentHashSet<TextReaction>>();
            if (loadData)
                this.LoadData();
        }


        public void LoadData()
        {
            Log.Debug("Loading text and emoji reactions");
            try {
                using (DatabaseContext db = this.dbb.CreateContext()) {
                    var x = db.BlockedChannels.ToList();
                    this.treactions = new ConcurrentDictionary<ulong, ConcurrentHashSet<TextReaction>>(
                        db.TextReactions
                            .Include(t => t.DbTriggers)
                            .AsEnumerable()
                            .GroupBy(tr => tr.GuildId)
                            .ToDictionary(g => g.Key, g => new ConcurrentHashSet<TextReaction>(g.Select(tr => new TextReaction(tr.Id, tr.Triggers, tr.Response, true))))
                    );
                    this.ereactions = new ConcurrentDictionary<ulong, ConcurrentHashSet<EmojiReaction>>(
                        db.EmojiReactions
                            .Include(t => t.DbTriggers)
                            .AsEnumerable()
                            .GroupBy(er => er.GuildId)
                            .ToDictionary(g => g.Key, g => new ConcurrentHashSet<EmojiReaction>(g.Select(er => new EmojiReaction(er.Id, er.Triggers, er.Reaction, true))))
                    );
                }
            } catch (Exception e) {
                Log.Error(e, "Loading reactions failed");
            }
        }


        #region EmojiReactions
        public IReadOnlyCollection<EmojiReaction> GetGuildEmojiReactions(ulong gid)
        {
            if (this.ereactions.TryGetValue(gid, out ConcurrentHashSet<EmojiReaction> ers))
                return ers.ToList();
            else
                return Array.Empty<EmojiReaction>();
        }

        public IReadOnlyCollection<EmojiReaction> FindMatchingEmojiReactions(ulong gid, string trigger)
        {
            return this.GetGuildEmojiReactions(gid)
                .Where(er => er.IsMatch(trigger))
                .ToList();
        }

        public async Task<int> AddEmojiReactionAsync(ulong gid, DiscordEmoji emoji, IEnumerable<string> triggers, bool regex)
        {
            ConcurrentHashSet<EmojiReaction> ers = this.ereactions.GetOrAdd(gid, new ConcurrentHashSet<EmojiReaction>());

            int added = 0;
            using (DatabaseContext db = this.dbb.CreateContext()) {
                await db.Database.BeginTransactionAsync();
                try {
                    DatabaseEmojiReaction dber = db.EmojiReactions.FirstOrDefault(er => er.GuildId == gid && er.Reaction == emoji.GetDiscordName());
                    if (dber is null) {
                        dber = new DatabaseEmojiReaction {
                            GuildId = gid,
                            Reaction = emoji.GetDiscordName()
                        };
                        db.EmojiReactions.Add(dber);
                        await db.SaveChangesAsync();
                    }

                    foreach (string trigger in triggers.Select(t => t.ToLowerInvariant())) {
                        string ename = emoji.GetDiscordName();
                        if (ers.Where(er => er.ContainsTriggerPattern(trigger)).Any(er => er.Response == ename))
                            continue;

                        EmojiReaction reaction = ers.FirstOrDefault(tr => tr.Response == ename);
                        if (reaction is null) {
                            if (!ers.Add(new EmojiReaction(dber.Id, trigger, ename, isRegex: regex))) {
                                db.Database.RollbackTransaction();
                                return 0;
                            }
                        } else {
                            if (!reaction.AddTrigger(trigger, isRegex: regex)) {
                                db.Database.RollbackTransaction();
                                return 0;
                            }
                        }

                        dber.DbTriggers.Add(new DatabaseEmojiReactionTrigger { ReactionId = dber.Id, Trigger = regex ? trigger : Regex.Escape(trigger) });
                        added++;
                    }
                } catch {
                    db.Database.RollbackTransaction();
                    return 0;
                }
                db.Database.CommitTransaction();
                await db.SaveChangesAsync();
            }

            return added;
        }

        public async Task<int> RemoveEmojiReactionsAsync(ulong gid, DiscordEmoji emoji)
        {
            if (!this.ereactions.TryGetValue(gid, out ConcurrentHashSet<EmojiReaction> ers) || !ers.Any())
                return 0;

            string ename = emoji.GetDiscordName();
            int removed = ers.RemoveWhere(er => er.Response == ename);

            using (DatabaseContext db = this.dbb.CreateContext()) {
                db.EmojiReactions.RemoveRange(db.EmojiReactions.Where(er => er.GuildId == gid && er.Reaction == ename));
                await db.SaveChangesAsync();
            }

            return removed;
        }

        public async Task<int> RemoveEmojiReactionsAsync(ulong gid, IEnumerable<string> toRemove)
        {
            if (!toRemove.Any())
                return 0;

            if (!this.ereactions.TryGetValue(gid, out ConcurrentHashSet<EmojiReaction> ers) || !ers.Any())
                return 0;

            int removed = ers.RemoveWhere(er => toRemove.Contains(er.Response));

            using (DatabaseContext db = this.dbb.CreateContext()) {
                db.EmojiReactions.RemoveRange(db.EmojiReactions.Where(er => er.GuildId == gid && toRemove.Contains(er.Reaction)));
                await db.SaveChangesAsync();
            }

            return removed;
        }

        public async Task<int> RemoveEmojiReactionsAsync(ulong gid)
        {
            int removed = 0;
            if (this.ereactions.ContainsKey(gid)) {
                if (this.ereactions.TryRemove(gid, out ConcurrentHashSet<EmojiReaction> ers))
                    removed = ers.Count;
                else
                    throw new ConcurrentOperationException("Failed to remove emoji reaction collection!");
            }

            using (DatabaseContext db = this.dbb.CreateContext()) {
                db.EmojiReactions.RemoveRange(db.EmojiReactions.Where(er => er.GuildId == gid));
                await db.SaveChangesAsync();
            }

            return removed;
        }

        public async Task<int> RemoveEmojiReactionsAsync(ulong gid, IEnumerable<int> ids)
        {
            if (!ids.Any())
                return 0;

            if (!this.ereactions.TryGetValue(gid, out ConcurrentHashSet<EmojiReaction> ers) || !ers.Any())
                return 0;

            int removed = ers.RemoveWhere(er => ids.Contains(er.Id));

            var eb = new StringBuilder();
            using (DatabaseContext db = this.dbb.CreateContext()) {
                foreach (int id in ids) {
                    if (!ers.Any(er => er.Id == id)) {
                        eb.AppendLine($"Note: Reaction with ID {id} does not exist in this guild.");
                        continue;
                    } else {
                        db.EmojiReactions.Remove(new DatabaseEmojiReaction { Id = id, GuildId = gid });
                    }
                }
                await db.SaveChangesAsync();
            }

            return removed;
        }

        public async Task<int> RemoveEmojiReactionTriggersAsync(ulong gid, IEnumerable<EmojiReaction> reactions, IEnumerable<string> triggers)
        {
            if (!reactions.Any() || !triggers.Any())
                return 0;

            if (!this.ereactions.TryGetValue(gid, out ConcurrentHashSet<EmojiReaction> ers))
                return 0;

            int removed = 0;
            foreach (string trigger in triggers) {
                foreach (EmojiReaction er in reactions) {
                    er.RemoveTrigger(trigger);
                    if (er.RegexCount == 0)
                        removed += ers.TryRemove(er) ? 1 : 0;
                }
            }

            using (DatabaseContext db = this.dbb.CreateContext()) {
                var toUpdate = db.EmojiReactions
                   .Include(er => er.DbTriggers)
                   .Where(er => er.GuildId == gid && reactions.Any(r => r.Id == er.Id))
                   .ToList();
                foreach (DatabaseEmojiReaction er in toUpdate) {
                    foreach (string trigger in triggers)
                        er.DbTriggers.Remove(er.DbTriggers.FirstOrDefault(r => er.Id == r.ReactionId && r.Trigger == trigger));
                    if (er.DbTriggers.Any())
                        db.EmojiReactions.Update(er);
                    else
                        db.EmojiReactions.Remove(er);
                    await db.SaveChangesAsync();
                }
            }

            return removed;
        }
        #endregion

        #region TextReactions
        public IReadOnlyCollection<TextReaction> GetGuildTextReactions(ulong gid)
        {
            if (this.treactions.TryGetValue(gid, out ConcurrentHashSet<TextReaction> trs))
                return trs.ToList();
            else
                return Array.Empty<TextReaction>();
        }

        public TextReaction FindMatchingTextReaction(ulong gid, string trigger)
        {
            return this.GetGuildTextReactions(gid)
                .FirstOrDefault(tr => tr.IsMatch(trigger));
        }

        public bool GuildHasTextReaction(ulong gid, string trigger)
            => this.GetGuildTextReactions(gid).Any(tr => tr.ContainsTriggerPattern(trigger));

        public async Task<bool> AddTextReactionAsync(ulong gid, string trigger, string response, bool regex)
        {
            ConcurrentHashSet<TextReaction> trs = this.treactions.GetOrAdd(gid, new ConcurrentHashSet<TextReaction>());

            if (trs.Any(tr => tr.ContainsTriggerPattern(trigger)))
                return false;

            bool success = true;
            using (DatabaseContext db = this.dbb.CreateContext()) {
                await db.Database.BeginTransactionAsync();
                try {
                    DatabaseTextReaction dbtr = db.TextReactions.FirstOrDefault(tr => tr.GuildId == gid && tr.Response == response);
                    if (dbtr is null) {
                        dbtr = new DatabaseTextReaction {
                            GuildId = gid,
                            Response = response,
                        };
                        db.TextReactions.Add(dbtr);
                        await db.SaveChangesAsync();
                    }

                    dbtr.DbTriggers.Add(new DatabaseTextReactionTrigger { ReactionId = dbtr.Id, Trigger = regex ? trigger : Regex.Escape(trigger) });

                    await db.SaveChangesAsync();

                    TextReaction reaction = trs.FirstOrDefault(tr => tr.Response == response);
                    success = reaction is null ? trs.Add(new TextReaction(dbtr.Id, trigger, response, regex)) : reaction.AddTrigger(trigger, regex);
                } finally {
                    if (success) {
                        db.Database.CommitTransaction();
                        await db.SaveChangesAsync();
                    } else {
                        db.Database.RollbackTransaction();
                    }
                }
            }

            return success;
        }

        public async Task<int> RemoveTextReactionsAsync(ulong gid)
        {
            int removed = 0;
            if (this.treactions.ContainsKey(gid)) {
                if (this.treactions.TryRemove(gid, out ConcurrentHashSet<TextReaction> trs))
                    removed = trs.Count;
                else
                    throw new ConcurrentOperationException("Failed to remove emoji reaction collection!");
            }

            using (DatabaseContext db = this.dbb.CreateContext()) {
                db.TextReactions.RemoveRange(db.TextReactions.Where(tr => tr.GuildId == gid));
                await db.SaveChangesAsync();
            }

            return removed;
        }

        public async Task<int> RemoveTextReactionsAsync(ulong gid, IEnumerable<int> ids)
        {
            if (!ids.Any())
                return 0;

            if (!this.treactions.TryGetValue(gid, out ConcurrentHashSet<TextReaction> trs) || !trs.Any())
                return 0;

            int removed = trs.RemoveWhere(tr => ids.Contains(tr.Id));

            using (DatabaseContext db = this.dbb.CreateContext()) {
                db.TextReactions.RemoveRange(db.TextReactions.Where(tr => tr.GuildId == gid && ids.Contains(tr.Id)));
                await db.SaveChangesAsync();
            }

            return removed;
        }

        public async Task<int> RemoveTextReactionTriggersAsync(ulong gid, IEnumerable<TextReaction> reactions, IEnumerable<string> triggers)
        {
            if (!reactions.Any() || !triggers.Any())
                return 0;

            if (!this.treactions.TryGetValue(gid, out ConcurrentHashSet<TextReaction> trs))
                return 0;

            int removed = 0;
            foreach (string trigger in triggers) {
                foreach (TextReaction tr in reactions) {
                    tr.RemoveTrigger(trigger);
                    if (tr.RegexCount == 0)
                        removed += trs.TryRemove(tr) ? 1 : 0;
                }
            }

            using (DatabaseContext db = this.dbb.CreateContext()) {
                var toUpdate = db.TextReactions
                    .Include(tr => tr.DbTriggers)
                    .Where(tr => tr.GuildId == gid && reactions.Any(r => r.Id == tr.Id))
                    .ToList();
                foreach (DatabaseTextReaction tr in toUpdate) {
                    foreach (string trigger in triggers)
                        tr.DbTriggers.Remove(tr.DbTriggers.FirstOrDefault(r => tr.Id == r.ReactionId && r.Trigger == trigger));
                    if (tr.DbTriggers.Any())
                        db.TextReactions.Update(tr);
                    else
                        db.TextReactions.Remove(tr);
                    await db.SaveChangesAsync();
                }
            }

            return removed;
        }
        #endregion
    }
}
