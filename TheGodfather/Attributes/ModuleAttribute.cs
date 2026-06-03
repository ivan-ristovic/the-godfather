using Discord.Commands;

namespace TheGodfather.Attributes;

public enum ModuleType
{
    Administration,
    Chickens,
    Currency,
    Games,
    Misc,
    Owner,
    Polls,
    Reactions,
    Reminders,
    Searches,
    Uncategorized
}


public static class ModuleTypeExtensions
{
    public static Color ToColor(this ModuleType type)
    {
        return type switch {
            ModuleType.Administration => new Color(0x2ECC71),
            ModuleType.Chickens => new Color(0xE67E22),
            ModuleType.Currency => new Color(0x006400),
            ModuleType.Games => Color.Teal,
            ModuleType.Misc => new Color(0x007FFF),
            ModuleType.Owner => new Color(0x2F3136),
            ModuleType.Polls => new Color(0xF1C40F),
            ModuleType.Reactions => Color.Gold,
            ModuleType.Reminders => new Color(0x8B0000),
            ModuleType.Uncategorized => Color.DarkGrey,
            ModuleType.Searches => new Color(0x1ABC9C),
            _ => Color.Green
        };
    }

    public static string ToLocalizedDescriptionKey(this ModuleType type)
        => type.ToLocalizedDescription(null).Key;

    public static TranslationKey ToLocalizedDescription(this ModuleType type, string? cmdList)
    {
        return type switch {
            ModuleType.Administration => TranslationKey.m_admin(cmdList),
            ModuleType.Chickens => TranslationKey.m_chicken(cmdList),
            ModuleType.Currency => TranslationKey.m_currency(cmdList),
            ModuleType.Games => TranslationKey.m_games(cmdList),
            ModuleType.Misc => TranslationKey.m_misc(cmdList),
            ModuleType.Owner => TranslationKey.m_owner(cmdList),
            ModuleType.Polls => TranslationKey.m_polls(cmdList),
            ModuleType.Reactions => TranslationKey.m_reactions(cmdList),
            ModuleType.Reminders => TranslationKey.m_reminders(cmdList),
            ModuleType.Uncategorized => TranslationKey.m_uncat(cmdList),
            ModuleType.Searches => TranslationKey.m_search(cmdList),
            _ => TranslationKey.h_desc_none
        };
    }
}


[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public sealed class ModuleAttribute : Attribute
{
    public static ModuleAttribute AttachedTo(CommandInfo cmd)
    {
        ModuleAttribute? mattr = cmd.Attributes.OfType<ModuleAttribute>().FirstOrDefault();
        if (mattr is not null)
            return mattr;

        for (ModuleInfo? module = cmd.Module; module is not null; module = module.Parent) {
            mattr = module.Attributes.OfType<ModuleAttribute>().FirstOrDefault();
            if (mattr is not null)
                return mattr;
        }

        return new ModuleAttribute(ModuleType.Uncategorized);
    }

    public static ModuleAttribute AttachedTo(Type t)
    {
        return GetCustomAttribute(t, typeof(ModuleAttribute)) is not ModuleAttribute moduleAttr
            ? t.DeclaringType is not null ? AttachedTo(t.DeclaringType) : new ModuleAttribute(ModuleType.Uncategorized)
            : moduleAttr;
    }


    public ModuleType Module { get; }


    public ModuleAttribute(ModuleType module)
    {
        this.Module = module;
    }
}
