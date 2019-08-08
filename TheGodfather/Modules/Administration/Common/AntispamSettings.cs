namespace TheGodfather.Modules.Administration.Common
{
    public sealed class AntispamSettings
    {
        public PunishmentAction Action { get; set; } = PunishmentAction.TemporaryMute;
        public bool Enabled { get; set; } = false;
        public short Sensitivity { get; set; } = 5;
    }
}
