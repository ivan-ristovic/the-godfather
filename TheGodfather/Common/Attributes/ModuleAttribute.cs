#region USING_DIRECTIVES
using System;
#endregion

namespace TheGodfather.Common.Attributes
{
    public enum ModuleType : byte
    {
        Administration,
        Gambling,
        Games,
        Miscellaneous,
        Music,
        Owner,
        Polls,
        Reactions,
        Searches,
        SWAT,
        Uncategorized
    }


    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
    internal class ModuleAttribute : Attribute
    {
        public ModuleType Module { get; private set; }


        public ModuleAttribute(ModuleType module)
        {
            Module = module;
        }
    }
}
