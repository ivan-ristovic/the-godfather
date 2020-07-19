using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TheGodfather.Common.Collections;
using TheGodfather.Database;
using TheGodfather.Database.Models;
using TheGodfather.Services;

namespace TheGodfather.Modules.Administration.Services
{
    public sealed class CommandRulesService : ITheGodfatherService
    {
        public bool IsDisabled => false;

        private readonly ConcurrentDictionary<ulong, ConcurrentHashSet<CommandRule>> cache; 
        private readonly DbContextBuilder dbb;


        public CommandRulesService(DbContextBuilder dbb)
        {
            this.dbb = dbb;
            this.cache = new ConcurrentDictionary<ulong, ConcurrentHashSet<CommandRule>>();
        }


        public bool IsBlocked(ulong gid, ulong cid, string qualifiedCommandName)
        {
            using (TheGodfatherDbContext db = this.dbb.CreateContext()) {
                IEnumerable<CommandRule> dbrules = db.CommandRules
                    .Where(cr => cr.GuildIdDb == (long)gid && (cr.ChannelIdDb == 0 || cr.ChannelIdDb == (long)cid))
                    .AsEnumerable()
                    .Where(cr => qualifiedCommandName.StartsWith(cr.Command));
                if (!dbrules.Any() || dbrules.Any(cr => cr.ChannelIdDb == (long)cid && cr.Allowed))
                    return false;
            }
            return true;
        }

        public async Task<IReadOnlyList<CommandRule>> GetRulesAsync(ulong gid, string? cmd = null)
        {
            IReadOnlyList<CommandRule> rules;
            using (TheGodfatherDbContext db = this.dbb.CreateContext()) {
                rules = await db.CommandRules
                    .Where(cr => cr.GuildIdDb == (long)gid)
                    .ToListAsync();
            }
            return cmd is { } 
                ? rules.Where(cr => cr.Command.StartsWith(cmd)).ToList() 
                : rules;
        }

        public async Task AddRuleAsync(ulong gid, string cmd, bool allow, IEnumerable<ulong> cids)
        {
            using (TheGodfatherDbContext db = this.dbb.CreateContext()) {
                IEnumerable<CommandRule> crs = await this.GetRulesAsync(gid, cmd);

                db.CommandRules.RemoveRange(crs.Where(cr => cids.Any(id => (long)id == cr.ChannelIdDb)));

                if (cids is null || !cids.Any()) {
                    db.CommandRules.RemoveRange(crs);
                } else {
                    db.CommandRules.AddRange(
                        cids.Distinct()
                            .Select(id => new CommandRule {
                                Allowed = allow,
                                ChannelId = id,
                                Command = cmd,
                                GuildId = gid,
                            })
                    );
                }

                if (!allow || cids.Any()) {
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
    }
}
