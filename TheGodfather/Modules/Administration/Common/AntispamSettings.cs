namespace TheGodfather.Modules.Administration.Common
{
    public sealed class AntispamSettings
    {
        public PunishmentActionType Action { get; set; } = PunishmentActionType.TemporaryMute;
        public bool Enabled { get; set; } = false;
        public short Sensitivity { get; set; } = 5;
    }
}
