using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TheGodfather.Database;
using TheGodfather.Database.Models;
using TheGodfather.Extensions;
using TheGodfather.Services;

namespace TheGodfather.Modules.Administration.Services
{
    public sealed class CommandRulesService : ITheGodfatherService
    {
        public bool IsDisabled => false;

        private readonly DbContextBuilder dbb;


        public CommandRulesService(DbContextBuilder dbb)
        {
            this.dbb = dbb;
        }


        public bool IsBlocked(string qualifiedCommandName, ulong gid, ulong cid, ulong? parentId)
        {
            using (TheGodfatherDbContext db = this.dbb.CreateContext()) {
                CommandRule? specialRule = DbFetch(db, qualifiedCommandName, gid, cid);
                if (specialRule is { })
                    return !specialRule.Allowed;

                if (parentId is { }) {
                    CommandRule? specialParentRule = DbFetch(db, qualifiedCommandName, gid, parentId.Value);
                    if (specialParentRule is { })
                        return !specialParentRule.Allowed;
                }

                return !DbFetch(db, qualifiedCommandName, gid, 0)?.Allowed ?? false;
            }

            
            static CommandRule? DbFetch(TheGodfatherDbContext db, string qcmd, ulong gid, ulong cid)
            {
                IEnumerable<CommandRule> crs = db.CommandRules
                    .Where(cr => cr.GuildIdDb == (long)gid && cr.ChannelIdDb == (long)cid)
                    .AsEnumerable()
                    .Where(cr => cr.AppliesTo(qcmd))
                    ;

                return crs.Any()
                    ? crs.MaxBy(cr => cr.Command)
                    : null;
            }
        }

        public IReadOnlyList<CommandRule> GetRules(ulong gid, string? cmd = null)
        {
            using (TheGodfatherDbContext db = this.dbb.CreateContext()) {
                IEnumerable<CommandRule> rules = db.CommandRules.Where(cr => cr.GuildIdDb == (long)gid);
                if (!string.IsNullOrWhiteSpace(cmd))
                    rules = rules.Where(cr => cr.AppliesTo(cmd));
                return rules.ToList().AsReadOnly();
            }
        }

        public Task AddRuleAsync(ulong gid, string cmd, bool allow, params ulong[] cids)
            => this.AddRuleAsync(gid, cmd, allow, cids.AsEnumerable());

        public async Task AddRuleAsync(ulong gid, string cmd, bool allow, IEnumerable<ulong> cids)
        {
            using (TheGodfatherDbContext db = this.dbb.CreateContext()) {
                IEnumerable<CommandRule> crs = this.GetRules(gid, cmd);

                if (!cids.Any()) {
                    db.CommandRules.RemoveRange(crs);
                } else {
                    db.CommandRules.RemoveRange(crs.Where(cr => cids.Any(cid => cr.ChannelIdDb == (long)cid)));
                    db.CommandRules.AddRange(
                        cids.Distinct()
                            .Where(cid => !crs.Any(cr => this.IsBlocked(cmd, gid, cid, null) != allow))
                            .Select(cid => new CommandRule {
                                Allowed = allow,
                                ChannelId = cid,
                                Command = cmd,
                                GuildId = gid,
                            })
                    );
                }

                if (allow || (!allow && !cids.Any())) {
                    var cr = new CommandRule {
                        Allowed = false,
                        ChannelId = 0,
                        Command = cmd,
                        GuildId = gid,
                    };
                    if (await db.CommandRules.FindAsync(cr.GuildIdDb, cr.ChannelIdDb, cr.Command) is null)
                        db.CommandRules.Add(cr);
                }

                await db.SaveChangesAsync();
            }
        }

        public async Task ClearAsync(ulong gid)
        {
            using (TheGodfatherDbContext db = this.dbb.CreateContext()) {
                db.CommandRules.RemoveRange(db.CommandRules.Where(cr => cr.GuildIdDb == (long)gid));
                await db.SaveChangesAsync();
            }
        }
    }
}
