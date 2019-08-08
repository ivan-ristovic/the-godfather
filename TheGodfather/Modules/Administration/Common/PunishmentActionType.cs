namespace TheGodfather.Modules.Administration.Common
{
    public enum PunishmentAction : byte
    {
        PermanentMute = 0,
        TemporaryMute = 1,
        Kick = 2,
        TemporaryBan = 3,
        PermanentBan = 4
    }

    public static class PunishmentActionTypeExtensions
    {
        public static string ToTypeString(this PunishmentAction type)
        {
            switch (type) {
                case PunishmentAction.Kick: return "Kick";
                case PunishmentAction.PermanentMute: return "Permanent mute";
                case PunishmentAction.PermanentBan: return "Permanent ban";
                case PunishmentAction.TemporaryBan: return "Temporary ban";
                case PunishmentAction.TemporaryMute: return "Temporary mute";
                default: return "Unknown";
            }
        }
    }
}
