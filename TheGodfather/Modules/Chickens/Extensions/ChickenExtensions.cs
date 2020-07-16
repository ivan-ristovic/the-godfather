using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;
using Microsoft.EntityFrameworkCore;
using Serilog;
using TheGodfather.Common;
using TheGodfather.Database;
using TheGodfather.Database.Models;

namespace TheGodfather.Modules.Chickens.Extensions
{
    public static class ChickenOperations
    {
        public static Task<Chicken?> FindAsync(DiscordClient client, DbContextBuilder dbb, ulong gid, ulong uid, bool findOwner = true)
        {
            Chicken? chicken = null;
            using (TheGodfatherDbContext db = dbb.CreateContext()) {
                chicken = db.Chickens
                    .Include(c => c.Upgrades)
                        .ThenInclude(u => u.Upgrade)
                    .SingleOrDefault(c => c.GuildIdDb == (long)gid && c.UserIdDb == (long)uid);
            }

            return chicken is { } && findOwner ? chicken.SetOwnerAsync(client, dbb) : Task.FromResult(chicken);
        }

        public static Task<Chicken?> FindAsync(DiscordClient client, DbContextBuilder dbb, ulong gid, string name, bool findOwner = true)
        {
            Chicken? chicken = null;
            using (TheGodfatherDbContext db = dbb.CreateContext()) {
                Chicken dbc = db.Chickens
                    .Include(c => c.Upgrades)
                        .ThenInclude(u => u.Upgrade)
                    .Where(c => c.GuildIdDb == (long)gid)
                    .AsEnumerable()
                    .FirstOrDefault(c => string.Compare(c.Name, name, true) == 0);
            }

            return chicken is { } && findOwner ? chicken.SetOwnerAsync(client, dbb) : Task.FromResult(chicken);
        }

        public static async Task<Chicken?> SetOwnerAsync(this Chicken chicken, DiscordClient client, DbContextBuilder dbb)
        {
            try {
                chicken.Owner = await client.GetUserAsync(chicken.UserId);
                return chicken;
            } catch (NotFoundException) {
                Log.Debug("Deleting chicken for user {UserId} in guild {GuildId} due to owner 404", chicken.UserId, chicken.GuildId);
                using (TheGodfatherDbContext db = dbb.CreateContext()) {
                    db.Chickens.Remove(chicken);
                    await db.SaveChangesAsync();
                }
                return null;
            }
        }


        public static DiscordEmbed ToDiscordEmbed(this Chicken chicken)
        {
            var emb = new DiscordEmbedBuilder {
                Title = $"{Emojis.Chicken} {chicken.Name}",
                Color = DiscordColor.Yellow
            };

            emb.AddField("Owner", chicken.Owner?.Mention ?? chicken.UserId.ToString(), inline: true);
            emb.AddField("Credit value", $"{chicken.SellPrice:n0}", inline: true);
            emb.AddField("Stats", chicken.Stats.ToString(), inline: true);
            if (chicken.Stats.Upgrades.Any())
                emb.AddField("Upgrades", string.Join(", ", chicken.Stats.Upgrades.Select(u => u.Upgrade.Name)), inline: true);

            if (chicken.Owner is { })
                emb.WithFooter("Chickens will rule the world someday", chicken.Owner.AvatarUrl);

            return emb.Build();
        }
    }
}
