using Humanizer;
using TheGodfather.Database.Models;
using TheGodfather.Translations;

namespace TheGodfather.Modules.Administration.Common
{
    public sealed class RatelimitSettings : ISettings
    {
        public const int MinSensitivity = 4;
        public const int MaxSensitivity = 10;

        public Punishment.Action Action { get; set; } = Punishment.Action.TemporaryMute;
        public bool Enabled { get; set; } = false;
        public short Sensitivity { get; set; } = 5;


        public TranslationKey ToEmbedFieldString()
            => this.Enabled ? TranslationKey.fmt_settings_am(this.Sensitivity, this.Action.Humanize()) : TranslationKey.str_off;
    }
}
