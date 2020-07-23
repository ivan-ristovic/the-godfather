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
                ModuleType.Administration => DiscordColor.SapGreen,
                ModuleType.Chickens => DiscordColor.Orange,
                ModuleType.Currency => DiscordColor.DarkGreen,
                ModuleType.Games => DiscordColor.CornflowerBlue,
                ModuleType.Miscellaneous => DiscordColor.White,
                ModuleType.Music => DiscordColor.Aquamarine,
                ModuleType.Owner => DiscordColor.DarkButNotBlack,
                ModuleType.Polls => DiscordColor.Orange,
                ModuleType.Reactions => DiscordColor.Yellow,
                ModuleType.Reminders => DiscordColor.DarkRed,
                ModuleType.SWAT => DiscordColor.Black,
                ModuleType.Uncategorized => DiscordColor.Gray,
                ModuleType.Searches => DiscordColor.Turquoise,
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
