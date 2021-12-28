using DSharpPlus;
using DSharpPlus.Exceptions;
using TheGodfather.Modules.Chickens.Services;

namespace TheGodfather.Modules.Chickens.Extensions;

public static class ChickenServiceExtensions
{
    public static async Task<Chicken?> GetAndSetOwnerAsync(this ChickenService service, DiscordClient client, ulong gid, ulong uid)
    {
        Chicken? chicken = await service.GetCompleteAsync(gid, uid);
        if (chicken is null)
            return null;

        bool success = await service.SetOwnerAsync(chicken, client);
        return success ? chicken : null;
    }

    public static async Task<bool> SetOwnerAsync(this ChickenService service, Chicken chicken, DiscordClient client)
    {
        try {
            chicken.Owner = await client.GetUserAsync(chicken.UserId);
            return true;
        } catch (NotFoundException) {
            Log.Debug("Deleting chicken for user {UserId} in guild {GuildId} due to owner 404", chicken.UserId, chicken.GuildId);
            await service.RemoveAsync(chicken);
            return false;
        }
    }
}