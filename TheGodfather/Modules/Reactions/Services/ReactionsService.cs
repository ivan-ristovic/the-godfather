using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Serilog;
using TheGodfather.Common.Collections;
using TheGodfather.Database;
using TheGodfather.Database.Models;
using TheGodfather.Exceptions;
using TheGodfather.Services;

namespace TheGodfather.Modules.Reactions.Services
{
    public sealed class ReactionsService : ITheGodfatherService
    {
        public bool IsDisabled => false;

        private readonly DbContextBuilder dbb;
        private ConcurrentDictionary<ulong, ConcurrentHashSet<EmojiReaction>> ereactions;
        private ConcurrentDictionary<ulong, ConcurrentHashSet<TextReaction>> treactions;


        public ReactionsService(DbContextBuilder dbb, bool loadData = true)
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
                using (TheGodfatherDbContext db = this.dbb.CreateContext()) {
                    this.treactions = new ConcurrentDictionary<ulong, ConcurrentHashSet<TextReaction>>(
                        db.TextReactions
                          .Include(tr => tr.DbTriggers)
                          .AsEnumerable()
                          .GroupBy(tr => tr.GuildId)
                          .ToDictionary(g => g.Key, g => new ConcurrentHashSet<TextReaction>(g))
                    );
                    this.ereactions = new ConcurrentDictionary<ulong, ConcurrentHashSet<EmojiReaction>>(
                        db.EmojiReactions
                          .Include(er => er.DbTriggers)
                          .AsEnumerable()
                          .GroupBy(er => er.GuildId)
                          .ToDictionary(g => g.Key, g => new ConcurrentHashSet<EmojiReaction>(g))
                    );
                }

                foreach ((ulong _, ConcurrentHashSet<EmojiReaction> ers) in this.ereactions) {
                    foreach (EmojiReaction er in ers)
                        er.CacheDbTriggers();
                }

                foreach ((ulong _, ConcurrentHashSet<TextReaction> trs) in this.treactions) {
                    foreach (TextReaction tr in trs)
                        tr.CacheDbTriggers();
                }
            } catch (Exception e) {
                Log.Error(e, "Loading reactions failed");
            }
        }


        #region Emoji Reactions
        public IReadOnlyCollection<EmojiReaction> GetGuildEmojiReactions(ulong gid)
        {
            return this.ereactions.TryGetValue(gid, out ConcurrentHashSet<EmojiReaction>? ers) && ers is { }
                ? ers.ToList()
                : (IReadOnlyCollection<EmojiReaction>)Array.Empty<EmojiReaction>();
        }

        public IReadOnlyCollection<EmojiReaction> FindMatchingEmojiReactions(ulong gid, string trigger)
        {
            return this.GetGuildEmojiReactions(gid)
                .Where(er => er.IsMatch(trigger))
                .ToList();
        }

        public async Task<int> AddEmojiReactionAsync(ulong gid, string emoji, IEnumerable<string> triggers, bool regex)
        {
            ConcurrentHashSet<EmojiReaction> ers = this.ereactions.GetOrAdd(gid, new ConcurrentHashSet<EmojiReaction>());

            int added = 0;
            using (TheGodfatherDbContext db = this.dbb.CreateContext()) {
                await db.Database.BeginTransactionAsync();
                try {
                    EmojiReaction? er = ers.FirstOrDefault(er => er.Response == emoji);
                    if (er is null) {
                        er = new EmojiReaction {
                            GuildId = gid,
                            Response = emoji
                        };
                        if (!ers.Add(er))
                            throw new ConcurrentOperationException("Failed to add emoji reaction to cache");
                        db.EmojiReactions.Add(er);
                        await db.SaveChangesAsync();
                    } else {
                        db.EmojiReactions.Attach(er);
                    }

                    foreach (string trigger in triggers.Select(t => t.ToLowerInvariant())) {
                        string ename = emoji;
                        if (ers.Where(er => er.ContainsTriggerPattern(trigger)).Any(er => er.Response == ename))
                            continue;

                        if (!er.AddTrigger(trigger, isRegex: regex))
                            throw new ConcurrentOperationException("Failed to add emoji reaction trigger");
                        er.DbTriggers.Add(new EmojiReactionTrigger { ReactionId = er.Id, Trigger = regex ? trigger : Regex.Escape(trigger) });
                        db.EmojiReactions.Update(er);

                        added++;
                    }
                } catch (Exception e) {
                    Log.Error(e, "Failed to add emoji reaction(s)");
                } finally {
                    if (added > 0) {
                        db.Database.CommitTransaction();
                        await db.SaveChangesAsync();
                    } else {
                        db.Database.RollbackTransaction();
                    }
                }
            }

            return added;
        }

        public async Task<int> RemoveEmojiReactionsAsync(ulong gid, string emoji)
        {
            if (!this.ereactions.TryGetValue(gid, out ConcurrentHashSet<EmojiReaction>? ers) || !ers.Any())
                return 0;

            int removed = ers.RemoveWhere(er => er.Response == emoji);

            using (TheGodfatherDbContext db = this.dbb.CreateContext()) {
                db.EmojiReactions.RemoveRange(db.EmojiReactions.Where(er => er.GuildIdDb == (long)gid && er.Response == emoji));
                await db.SaveChangesAsync();
            }

            return removed;
        }

        public async Task<int> RemoveEmojiReactionsAsync(ulong gid)
        {
            int removed = 0;
            if (this.ereactions.ContainsKey(gid)) {
                removed = this.ereactions.TryRemove(gid, out ConcurrentHashSet<EmojiReaction>? ers)
                    ? ers.Count
                    : throw new ConcurrentOperationException("Failed to remove emoji reaction collection!");
            }

            using (TheGodfatherDbContext db = this.dbb.CreateContext()) {
                db.EmojiReactions.RemoveRange(db.EmojiReactions.Where(er => er.GuildIdDb == (long)gid));
                await db.SaveChangesAsync();
            }

            return removed;
        }

        public async Task<int> RemoveEmojiReactionsAsync(ulong gid, IEnumerable<int> ids)
        {
            if (!ids.Any())
                return 0;

            if (!this.ereactions.TryGetValue(gid, out ConcurrentHashSet<EmojiReaction>? ers) || !ers.Any())
                return 0;

            int removed = ers.RemoveWhere(er => ids.Contains(er.Id));

            using (TheGodfatherDbContext db = this.dbb.CreateContext()) {
                db.EmojiReactions.RemoveRange(
                    db.EmojiReactions
                      .Where(er => er.GuildIdDb == (long)gid)
                      .AsEnumerable()
                      .Where(er => ids.Contains(er.Id))
                );
                await db.SaveChangesAsync();
            }

            return removed;
        }

        public async Task<int> RemoveEmojiReactionTriggersAsync(ulong gid, IEnumerable<EmojiReaction> reactions, IEnumerable<string> triggers)
        {
            if (!reactions.Any() || !triggers.Any())
                return 0;

            if (!this.ereactions.TryGetValue(gid, out ConcurrentHashSet<EmojiReaction>? ers))
                return 0;

            int removed = 0;
            foreach (string trigger in triggers) {
                foreach (EmojiReaction er in reactions) {
                    er.RemoveTrigger(trigger);
                    if (er.RegexCount == 0)
                        removed += ers.TryRemove(er) ? 1 : 0;
                }
            }

            using (TheGodfatherDbContext db = this.dbb.CreateContext()) {
                var toUpdate = db.EmojiReactions
                   .Where(er => er.GuildIdDb == (long)gid)
                   .Include(er => er.DbTriggers)
                   .AsEnumerable()
                   .Where(er => reactions.Any(r => r.Id == er.Id))
                   .ToList();
                foreach (EmojiReaction er in toUpdate) {
                    foreach (string trigger in triggers) {
                        EmojiReactionTrigger? t = er.DbTriggers.FirstOrDefault(r => er.Id == r.ReactionId && r.Trigger == trigger);
                        if (t is { })
                            er.DbTriggers.Remove(t);
                    }
                    if (er.DbTriggers.Any())
                        db.EmojiReactions.Update(er);
                    else
                        db.EmojiReactions.Remove(er);
                    await db.SaveChangesAsync();
                }
            }

            return removed;
        }

        public async Task RemoveEmojiReactionsWhereAsync(ulong gid, Func<EmojiReaction, bool> condition)
        {
            using TheGodfatherDbContext db = this.dbb.CreateContext();
            db.EmojiReactions.RemoveRange(
                db.EmojiReactions
                    .Where(r => r.GuildIdDb == (long)gid)
                    .AsEnumerable()
                    .Where(condition)
            );
            await db.SaveChangesAsync();
        }
        #endregion

        #region Text Reactions
        public IReadOnlyCollection<TextReaction> GetGuildTextReactions(ulong gid)
        {
            return this.treactions.TryGetValue(gid, out ConcurrentHashSet<TextReaction>? trs) && trs is { }
                ? trs.ToList()
                : (IReadOnlyCollection<TextReaction>)Array.Empty<TextReaction>();
        }

        public TextReaction? FindMatchingTextReaction(ulong gid, string trigger)
            => this.GetGuildTextReactions(gid).FirstOrDefault(tr => tr.IsMatch(trigger));

        public bool GuildHasTextReaction(ulong gid, string trigger)
            => this.GetGuildTextReactions(gid).Any(tr => tr.ContainsTriggerPattern(trigger));

        public async Task<bool> AddTextReactionAsync(ulong gid, string trigger, string response, bool regex)
        {
            ConcurrentHashSet<TextReaction> trs = this.treactions.GetOrAdd(gid, new ConcurrentHashSet<TextReaction>());

            if (trs.Any(tr => tr.ContainsTriggerPattern(trigger)))
                return false;

            bool success = false;
            using (TheGodfatherDbContext db = this.dbb.CreateContext()) {
                await db.Database.BeginTransactionAsync();
                try {
                    TextReaction? tr = trs.FirstOrDefault(tr => tr.GuildId == gid && tr.Response == response);
                    if (tr is null) {
                        tr = new TextReaction {
                            GuildId = gid,
                            Response = response,
                        };
                        if (!trs.Add(tr))
                            throw new ConcurrentOperationException("Failed to add text reaction to cache");
                        db.TextReactions.Add(tr);
                        await db.SaveChangesAsync();
                    } else {
                        db.TextReactions.Attach(tr);
                    }

                    if (!tr.AddTrigger(trigger, isRegex: regex))
                        throw new ConcurrentOperationException("Failed to add text reaction trigger");
                    tr.DbTriggers.Add(new TextReactionTrigger { ReactionId = tr.Id, Trigger = regex ? trigger : Regex.Escape(trigger) });
                    db.TextReactions.Update(tr);

                    success = true;
                } catch (Exception e) {
                    Log.Error(e, "Failed to add text reaction");
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
                removed = this.treactions.TryRemove(gid, out ConcurrentHashSet<TextReaction>? trs)
                    ? trs.Count
                    : throw new ConcurrentOperationException("Failed to remove emoji reaction collection!");
            }

            using (TheGodfatherDbContext db = this.dbb.CreateContext()) {
                db.TextReactions.RemoveRange(db.TextReactions.Where(tr => tr.GuildIdDb == (long)gid));
                await db.SaveChangesAsync();
            }

            return removed;
        }

        public async Task<int> RemoveTextReactionsAsync(ulong gid, IEnumerable<int> ids)
        {
            if (!ids.Any())
                return 0;

            if (!this.treactions.TryGetValue(gid, out ConcurrentHashSet<TextReaction>? trs) || !trs.Any())
                return 0;

            int removed = trs.RemoveWhere(tr => ids.Contains(tr.Id));

            using (TheGodfatherDbContext db = this.dbb.CreateContext()) {
                db.TextReactions.RemoveRange(
                    db.TextReactions
                      .Where(tr => tr.GuildIdDb == (long)gid)
                      .AsEnumerable()
                      .Where(tr => ids.Contains(tr.Id))
                );
                await db.SaveChangesAsync();
            }

            return removed;
        }

        public async Task<int> RemoveTextReactionTriggersAsync(ulong gid, IEnumerable<TextReaction> reactions, IEnumerable<string> triggers)
        {
            if (!reactions.Any() || !triggers.Any())
                return 0;

            if (!this.treactions.TryGetValue(gid, out ConcurrentHashSet<TextReaction>? trs))
                return 0;

            int removed = 0;
            foreach (string trigger in triggers) {
                foreach (TextReaction tr in reactions) {
                    tr.RemoveTrigger(trigger);
                    if (tr.RegexCount == 0)
                        removed += trs.TryRemove(tr) ? 1 : 0;
                }
            }

            using (TheGodfatherDbContext db = this.dbb.CreateContext()) {
                var toUpdate = db.TextReactions
                    .Where(tr => tr.GuildIdDb == (long)gid)
                    .Include(tr => tr.DbTriggers)
                    .AsEnumerable()
                    .Where(tr => reactions.Any(r => r.Id == tr.Id))
                    .ToList();
                foreach (TextReaction tr in toUpdate) {
                    foreach (string trigger in triggers) {
                        TextReactionTrigger? t = tr.DbTriggers.FirstOrDefault(r => tr.Id == r.ReactionId && r.Trigger == trigger);
                        if (t is { })
                            tr.DbTriggers.Remove(t);
                    }
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
