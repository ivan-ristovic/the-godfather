namespace TheGodfather.Modules.Administration.Common
{
    public sealed class RatelimitSettings
    {
        public PunishmentAction Action { get; set; } = PunishmentAction.TemporaryMute;
        public bool Enabled { get; set; } = false;
        public short Sensitivity { get; set; } = 5;
    }
}
