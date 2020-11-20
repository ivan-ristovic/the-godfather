using TheGodfather.Services;

namespace TheGodfather.Modules.Administration.Common
{
    public sealed class AntispamSettings
    {
        public const int MinSensitivity = 3;
        public const int MaxSensitivity = 10;

        public PunishmentAction Action { get; set; } = PunishmentAction.TemporaryMute;
        public bool Enabled { get; set; } = false;
        public short Sensitivity { get; set; } = 5;


        public string ToEmbedFieldString(ulong gid, LocalizationService lcs)
            => this.Enabled ? lcs.GetString(gid, "fmt-settings-as", this.Sensitivity, this.Action.ToTypeString()) : lcs.GetString(gid, "str-off");
    }
}
