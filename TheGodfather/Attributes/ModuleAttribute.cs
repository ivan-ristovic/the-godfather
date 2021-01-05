using System;
using System.Linq;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using TheGodfather.Modules;

namespace TheGodfather.Attributes
{
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
                _ => DiscordColor.Green,
            };
        }

        public static string ToLocalizedDescriptionKey(this ModuleType type)
        {
            return type switch {
                ModuleType.Administration => "m-admin",
                ModuleType.Chickens => "m-chicken",
                ModuleType.Currency => "m-currency",
                ModuleType.Games => "m-games",
                ModuleType.Misc => "m-misc",
                ModuleType.Music => "m-music",
                ModuleType.Owner => "m-owner",
                ModuleType.Polls => "m-polls",
                ModuleType.Reactions => "m-reactions",
                ModuleType.Reminders => "m-reminders",
                ModuleType.Uncategorized => "m-uncat",
                ModuleType.Searches => "m-search",
                _ => "h-desc-none",
            };
        }
    }


    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
    public sealed class ModuleAttribute : Attribute
    {
        public static ModuleAttribute AttachedTo(Command cmd)
        {
            var mattr = cmd.CustomAttributes.FirstOrDefault(attr => attr is ModuleAttribute) as ModuleAttribute;
            return mattr ?? (cmd.Parent is null ? new ModuleAttribute(ModuleType.Uncategorized) : AttachedTo(cmd.Parent));
        }

        public static ModuleAttribute AttachedTo(Type t)
        {
            return GetCustomAttribute(t, typeof(ModuleAttribute)) is not ModuleAttribute moduleAttr
                ? t.DeclaringType is { } ? AttachedTo(t.DeclaringType) : new ModuleAttribute(ModuleType.Uncategorized)
                : moduleAttr;
        }


        public ModuleType Module { get; private set; }


        public ModuleAttribute(ModuleType module)
        {
            this.Module = module;
        }
    }
}
