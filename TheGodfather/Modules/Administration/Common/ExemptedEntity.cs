namespace TheGodfather.Modules.Administration.Common
{
    public enum ExemptedEntityType : byte
    {
        Channel = 0,
        Member = 1,
        Role = 2
    }

    public static class EntityTypeExtensions
    {
        public static char ToFlag(this ExemptedEntityType entity)
        {
            switch (entity) {
                case ExemptedEntityType.Channel: return 'c';
                case ExemptedEntityType.Member: return 'm';
                case ExemptedEntityType.Role: return 'r';
                default: return '?';
            }
        }

        public static string ToUserFriendlyString(this ExemptedEntityType entity)
        {
            switch (entity) {
                case ExemptedEntityType.Channel: return "Channel";
                case ExemptedEntityType.Member: return "User";
                case ExemptedEntityType.Role: return "Role";
                default: return "Unknown";
            }
        }
    }

    public sealed class ExemptedEntity
    {
        public ulong GuildId { get; set; }
        public ulong Id { get; set; }
        public ExemptedEntityType Type { get; set; }
    }
}
