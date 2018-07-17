#region USING_DIRECTIVES
using System;
#endregion

namespace TheGodfather.Common.Attributes
{
    internal enum ModuleType
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
        Searches,
        SWAT,
        Uncategorized
    }


    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
    internal sealed class ModuleAttribute : Attribute
    {
        public ModuleType Module { get; private set; }


        public ModuleAttribute(ModuleType module)
        {
            this.Module = module;
        }
    }
}
