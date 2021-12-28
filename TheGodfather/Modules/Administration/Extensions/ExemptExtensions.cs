using System.Text;
using DSharpPlus;
using DSharpPlus.Entities;
using Microsoft.EntityFrameworkCore;

namespace TheGodfather.Modules.Administration.Extensions;

public static class ExemptExtensions
{
    public static IEnumerable<ulong> SelectIds<T>(this IEnumerable<T> objects)
        where T : SnowflakeObject
    {
        return objects.Select(o => o.Id);
    }

    public static void AddExemptions<TEntity>(this DbSet<TEntity> set, ulong gid, ExemptedEntityType type, IEnumerable<ulong> ids)
        where TEntity : ExemptedEntity, new()
    {
        set.AddRange(ids
            .Where(id => !set.AsQueryable().Where(dbe => dbe.GuildIdDb == (long)gid).Any(dbe => dbe.Type == type && dbe.IdDb == (long)id))
            .Select(id => new TEntity {
                GuildId = gid,
                Id = id,
                Type = type
            })
        );
    }

    public static async Task<string?> FormatExemptionsAsync(this IEnumerable<ExemptedEntity> exempts, DiscordClient client)
    {
        if (!exempts.Any())
            return null;

        var sb = new StringBuilder();
        foreach (ExemptedEntity ee in exempts.OrderBy(ee => ee.Type))
            try {
                string exemptName;
                switch (ee.Type) {
                    case ExemptedEntityType.Channel:
                        DiscordChannel chn = await client.GetChannelAsync(ee.Id);
                        exemptName = chn.Mention;
                        break;
                    case ExemptedEntityType.Member:
                        DiscordUser usr = await client.GetUserAsync(ee.Id);
                        exemptName = usr.Mention;
                        break;
                    case ExemptedEntityType.Role:
                        DiscordGuild guild = await client.GetGuildAsync(ee.GuildId);
                        DiscordRole role = guild.GetRole(ee.Id);
                        exemptName = role.Mention;
                        break;
                    default:
                        Log.Warning("Unknown exempt entity type found: {ExemptEntityType}.", ee.Type);
                        continue;
                }
                sb.AppendLine(exemptName);
            } catch {
                sb.AppendLine($"{ee.Type.ToUserFriendlyString()}: {ee.Id}");
            }

        return sb.ToString();
    }

    public static async Task<string?> FormatExemptionsAsync(this IEnumerable<ExemptedBackupEntity> exempts, DiscordClient client)
    {
        if (!exempts.Any())
            return null;

        var sb = new StringBuilder();
        foreach (ExemptedBackupEntity e in exempts)
            try {
                DiscordChannel chn = await client.GetChannelAsync(e.ChannelId);
                sb.Append(chn.Mention).Append(' ');
            } catch {
                sb.Append(e.ChannelId);
            }

        return sb.ToString();
    }
}