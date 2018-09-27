namespace TheGodfather.Modules.Administration.Common
{
    public class AntiInstantLeaveSettings
    {
        public bool Enabled { get; set; } = false;
        public short Cooldown { get; set; } = 3;
    }
}
