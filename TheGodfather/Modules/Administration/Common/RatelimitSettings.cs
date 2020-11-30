using Humanizer;
using TheGodfather.Services;

namespace TheGodfather.Modules.Administration.Common
{
    public sealed class RatelimitSettings
    {
        public const int MinSensitivity = 4;
        public const int MaxSensitivity = 10;

        public PunishmentAction Action { get; set; } = PunishmentAction.TemporaryMute;
        public bool Enabled { get; set; } = false;
        public short Sensitivity { get; set; } = 5;


        public string ToEmbedFieldString(ulong gid, LocalizationService lcs)
            => this.Enabled ? lcs.GetString(gid, "fmt-settings-rl", this.Sensitivity, this.Action.Humanize()) : lcs.GetString(gid, "str-off");
    }
}
