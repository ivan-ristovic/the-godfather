using System;
using System.Linq;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;

namespace TheGodfather.Attributes
{
    public enum ModuleType
    {
        Administration,
        Chickens,
        Currency,
        Games,
        Miscellaneous,
        Music,
        Owner,
        Polls,
        Reactions,
        Reminders,
        Searches,
        SWAT,
        Uncategorized
    }


    public static class ModuleTypeExtensions
    {
        public static DiscordColor ToDiscordColor(this ModuleType type)
        {
            return type switch
            {
                ModuleType.Administration => DiscordColor.Azure,
                ModuleType.Chickens => DiscordColor.Azure,
                ModuleType.Currency => DiscordColor.Azure,
                ModuleType.Games => DiscordColor.Azure,
                ModuleType.Miscellaneous => DiscordColor.Azure,
                ModuleType.Music => DiscordColor.Azure,
                ModuleType.Owner => DiscordColor.Azure,
                ModuleType.Polls => DiscordColor.Azure,
                ModuleType.Reactions => DiscordColor.Azure,
                ModuleType.Reminders => DiscordColor.Azure,
                ModuleType.SWAT => DiscordColor.Azure,
                ModuleType.Uncategorized => DiscordColor.Azure,
                ModuleType.Searches => DiscordColor.Green,
                _ => DiscordColor.Green,
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


        public ModuleType Module { get; private set; }


        public ModuleAttribute(ModuleType module)
        {
            this.Module = module;
        }
    }
}
