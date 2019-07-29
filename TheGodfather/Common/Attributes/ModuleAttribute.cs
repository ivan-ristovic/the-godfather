using System;
using System.Linq;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;

namespace TheGodfather.Common.Attributes
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
            switch (type) {
                case ModuleType.Administration:
                    return DiscordColor.Azure;
                case ModuleType.Chickens:
                    return DiscordColor.Azure;
                case ModuleType.Currency:
                    return DiscordColor.Azure;
                case ModuleType.Games:
                    return DiscordColor.Azure;
                case ModuleType.Miscellaneous:
                    return DiscordColor.Azure;
                case ModuleType.Music:
                    return DiscordColor.Azure;
                case ModuleType.Owner:
                    return DiscordColor.Azure;
                case ModuleType.Polls:
                    return DiscordColor.Azure;
                case ModuleType.Reactions:
                    return DiscordColor.Azure;
                case ModuleType.Reminders:
                    return DiscordColor.Azure;
                case ModuleType.SWAT:
                    return DiscordColor.Azure;
                case ModuleType.Uncategorized:
                    return DiscordColor.Azure;
                default:
                    return DiscordColor.Green;
            }
        }
    }


    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
    public sealed class ModuleAttribute : Attribute
    {
        public static ModuleAttribute ForCommand(Command cmd)
        {
            var mattr = cmd.CustomAttributes.FirstOrDefault(attr => attr is ModuleAttribute) as ModuleAttribute;
            return mattr ?? (cmd.Parent is null ? new ModuleAttribute(ModuleType.Uncategorized) : ForCommand(cmd.Parent));
        }


        public ModuleType Module { get; private set; }


        public ModuleAttribute(ModuleType module)
        {
            this.Module = module;
        }
    }
}
