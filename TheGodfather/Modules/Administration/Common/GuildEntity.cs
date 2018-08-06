namespace TheGodfather.Modules.Administration.Common
{
    public enum EntityType
    {
        Channel,
        Member,
        Role
    }

    public static class EntityTypeExtensions
    {
        public static char ToFlag(this EntityType entity)
        {
            switch (entity) {
                case EntityType.Channel: return 'c';
                case EntityType.Member: return 'm';
                case EntityType.Role: return 'r';
                default: return '?';
            }
        }

        public static string ToUserFriendlyString(this EntityType entity)
        {
            switch (entity) {
                case EntityType.Channel: return "Channel";
                case EntityType.Member: return "User";
                case EntityType.Role: return "Role";
                default: return "Unknown";
            }
        }
    }

    public sealed class GuildEntity
    {
        public ulong GuildId { get; set; }
        public ulong Id { get; set; }
        public EntityType Type { get; set; }
    }
}
