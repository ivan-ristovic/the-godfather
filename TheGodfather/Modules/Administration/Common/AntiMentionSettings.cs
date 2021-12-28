namespace TheGodfather.Modules.Administration.Common;

public sealed class AntiMentionSettings : ISettings
{
    public const int MinSensitivity = 2;
    public const int MaxSensitivity = 20;

    public Punishment.Action Action { get; set; } = Punishment.Action.TemporaryMute;
    public bool Enabled { get; set; } = false;
    public short Sensitivity { get; set; } = 5;


    public TranslationKey ToEmbedFieldString()
        => this.Enabled ? TranslationKey.fmt_settings_am(this.Sensitivity, this.Action.Humanize()) : TranslationKey.str_off;
}