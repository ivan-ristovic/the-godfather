using System.Collections.Generic;
using System.Linq;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using TheGodfather.Database.Models;

namespace TheGodfather.Modules.Administration.Extensions
{
    public static class ExemptsExtensions
    {
        public static bool AnyAppliesTo(this IEnumerable<ExemptedEntity> exempts, MessageCreateEventArgs e)
        {
            if (exempts.Any(ee => ee.Type == ExemptedEntityType.Channel && (ee.Id == e.Channel.Id || ee.Id == e.Channel.ParentId)))
                return true;
            if (exempts.Any(ee => ee.Type == ExemptedEntityType.Member && ee.Id == e.Author.Id))
                return true;
            if (exempts.Any(ee => ee.Type == ExemptedEntityType.Role && e.Author is DiscordMember member && member.Roles.Any(r => r.Id == ee.Id)))
                return true;
            return false;
        }
    }
}
