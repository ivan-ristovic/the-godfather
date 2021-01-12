using Humanizer;
using TheGodfather.Services;

namespace TheGodfather.Modules.Administration.Common
{
    public sealed class AntifloodSettings
    {
        public const int MinCooldown = 5;
        public const int MaxCooldown = 60;
        public const int MinSensitivity = 2;
        public const int MaxSensitivity = 20;

        public PunishmentAction Action { get; set; } = PunishmentAction.PermanentBan;
        public short Cooldown { get; set; } = 10;
        public bool Enabled { get; set; } = false;
        public short Sensitivity { get; set; } = 5;


        public string ToEmbedFieldString(ulong gid, LocalizationService lcs)
            => this.Enabled ? lcs.GetString(gid, "fmt-settings-af", this.Sensitivity, this.Cooldown, this.Action.Humanize()) : lcs.GetString(gid, "str-off");
    }
}
