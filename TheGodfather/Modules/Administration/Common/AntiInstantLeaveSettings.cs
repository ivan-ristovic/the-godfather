using TheGodfather.Services;

namespace TheGodfather.Modules.Administration.Common
{
    public sealed class AntiInstantLeaveSettings
    {
        public const int MinCooldown = 2;
        public const int MaxCooldown = 20;

        public bool Enabled { get; set; } = false;
        public short Cooldown { get; set; } = 3;


        public string ToEmbedFieldString(ulong gid, LocalizationService lcs)
            => this.Enabled ? lcs.GetString(gid, "fmt-settings-il", this.Cooldown) : lcs.GetString(gid, "str-off");
    }
}
