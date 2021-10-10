using Humanizer;
using TheGodfather.Database.Models;
using TheGodfather.Services;

namespace TheGodfather.Modules.Administration.Common
{
    public sealed class AntifloodSettings
    {
        public const int MinCooldown = 5;
        public const int MaxCooldown = 60;
        public const int MinSensitivity = 2;
        public const int MaxSensitivity = 20;

        public Punishment.Action Action { get; set; } = Punishment.Action.PermanentBan;
        public short Cooldown { get; set; } = 10;
        public bool Enabled { get; set; } = false;
        public short Sensitivity { get; set; } = 5;


        public string ToEmbedFieldString(ulong gid, LocalizationService lcs)
            => this.Enabled ? lcs.GetString(gid, "fmt-settings-af", this.Sensitivity, this.Cooldown, this.Action.Humanize()) : lcs.GetString(gid, "str-off");
    }
}
