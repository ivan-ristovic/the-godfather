using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TheGodfather.Database;
using TheGodfather.Database.Models;
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


        public bool IsBlocked(ulong gid, ulong cid, string qualifiedCommandName)
        {
            using (TheGodfatherDbContext db = this.dbb.CreateContext()) {
                IReadOnlyList<CommandRule> crs = db.CommandRules
                    .Where(cr => cr.GuildIdDb == (long)gid)
                    .AsEnumerable()
                    .Where(cr => cr.AppliesTo(qualifiedCommandName))
                    .ToList().AsReadOnly();
                
                if (!crs.Any())
                    return false;

                if (crs.Any(cr => cr.ChannelId == 0))
                    return true;

                CommandRule? special = crs.SingleOrDefault(cr => cr.ChannelId == cid);
                if (special is { })
                    return !special.Allowed;

                CommandRule? other = crs.FirstOrDefault(cr => cr.ChannelId != 0 && cr.ChannelId != cid && cr.Allowed);
                return other is { };
            }
        }

        public IReadOnlyList<CommandRule> GetRulesAsync(ulong gid, string? cmd = null)
        {
            using (TheGodfatherDbContext db = this.dbb.CreateContext()) {
                IEnumerable<CommandRule> rules = db.CommandRules
                    .Where(cr => cr.GuildIdDb == (long)gid)
                    .AsEnumerable();
                return cmd is { }
                    ? rules.Where(cr => cr.AppliesTo(cmd)).ToList().AsReadOnly()
                    : rules.ToList().AsReadOnly();
            }
        }

        public Task AddRuleAsync(ulong gid, string cmd, bool allow, params ulong[] cids)
            => this.AddRuleAsync(gid, cmd, allow, cids.AsEnumerable());

        public async Task AddRuleAsync(ulong gid, string cmd, bool allow, IEnumerable<ulong> cids)
        {
            using (TheGodfatherDbContext db = this.dbb.CreateContext()) {
                IEnumerable<CommandRule> crs = this.GetRulesAsync(gid, cmd);

                if (!cids.Any()) {
                    db.CommandRules.RemoveRange(crs);
                } else {
                    db.CommandRules.RemoveRange(crs.Where(cr => cids.Any(cid => cr.ChannelIdDb == (long)cid)));
                    db.CommandRules.AddRange(
                        cids.Distinct()
                            .Select(cid => new CommandRule {
                                Allowed = allow,
                                ChannelId = cid,
                                Command = cmd,
                                GuildId = gid,
                            })
                    );
                }

                if (!allow && !cids.Any()) {
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
