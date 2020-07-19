using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
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
    }
}
