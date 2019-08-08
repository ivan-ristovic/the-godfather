namespace TheGodfather.Modules.Administration.Common
{
    public sealed class AntifloodSettings
    {
        public PunishmentAction Action { get; set; } = PunishmentAction.PermanentBan;
        public short Cooldown { get; set; } = 10;
        public bool Enabled { get; set; } = false;
        public short Sensitivity { get; set; } = 5;
    }
}
