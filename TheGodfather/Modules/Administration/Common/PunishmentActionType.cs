namespace TheGodfather.Modules.Administration.Common
{
    public enum PunishmentActionType : byte
    {
        Mute = 0,
        TemporaryMute = 1,
        Kick = 2,
        TemporaryBan = 3,
        PermanentBan = 4
    }

    public static class PunishmentActionTypeExtensions
    {
        public static string ToTypeString(this PunishmentActionType type)
        {
            switch (type) {
                case PunishmentActionType.Kick: return "Kick";
                case PunishmentActionType.Mute: return "Permanent mute";
                case PunishmentActionType.PermanentBan: return "Permanent ban";
                case PunishmentActionType.TemporaryBan: return "Temporary ban";
                case PunishmentActionType.TemporaryMute: return "Temporary mute";
                default: return "Unknown";
            }
        }
    }
}
