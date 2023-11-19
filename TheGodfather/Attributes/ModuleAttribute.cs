using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;

namespace TheGodfather.Attributes;

public enum ModuleType
{
    Administration,
    Chickens,
    Currency,
    Games,
    Misc,
    Music,
    Owner,
    Polls,
    Reactions,
    Reminders,
    Searches,
    Uncategorized
}


public static class ModuleTypeExtensions
{
    public static DiscordColor ToDiscordColor(this ModuleType type)
    {
        return type switch {
            ModuleType.Administration => DiscordColor.SapGreen,
            ModuleType.Chickens => DiscordColor.Orange,
            ModuleType.Currency => DiscordColor.DarkGreen,
            ModuleType.Games => DiscordColor.Teal,
            ModuleType.Misc => DiscordColor.Azure,
            ModuleType.Music => DiscordColor.Aquamarine,
            ModuleType.Owner => DiscordColor.DarkButNotBlack,
            ModuleType.Polls => DiscordColor.Orange,
            ModuleType.Reactions => DiscordColor.Yellow,
            ModuleType.Reminders => DiscordColor.DarkRed,
            ModuleType.Uncategorized => DiscordColor.Gray,
            ModuleType.Searches => DiscordColor.Turquoise,
            _ => DiscordColor.Green
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
            ModuleType.Music => TranslationKey.m_music(cmdList),
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
    public static ModuleAttribute AttachedTo(Command cmd)
    {
        var mattr = cmd.CustomAttributes.FirstOrDefault(attr => attr is ModuleAttribute) as ModuleAttribute;
        if (cmd.Module is null)
            return new ModuleAttribute(ModuleType.Uncategorized);
        mattr ??= cmd.Module.ModuleType.GetCustomAttributes(typeof(ModuleAttribute), true).FirstOrDefault() as ModuleAttribute;
        return mattr ?? (cmd.Parent is null ? new ModuleAttribute(ModuleType.Uncategorized) : AttachedTo(cmd.Parent));
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