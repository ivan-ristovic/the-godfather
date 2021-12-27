using TheGodfather.Translations;

namespace TheGodfather.Modules.Administration.Common
{
    public sealed class AntiInstantLeaveSettings : ISettings
    {
        public const int MinCooldown = 2;
        public const int MaxCooldown = 20;

        public bool Enabled { get; set; } = false;
        public short Cooldown { get; set; } = 3;


        public TranslationKey ToEmbedFieldString()
            => this.Enabled ? TranslationKey.fmt_settings_il(this.Cooldown) : TranslationKey.str_off;
    }
}
