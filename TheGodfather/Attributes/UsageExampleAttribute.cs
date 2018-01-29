#region USING_DIRECTIVES
using System;
using System.Linq;
using System.Threading.Tasks;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
#endregion

namespace TheGodfather.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
    public class UsageExampleAttribute : CheckBaseAttribute
    {
        public string Example { get; private set; }


        public UsageExampleAttribute(string example)
        {
            Example = example;
        }


        public override Task<bool> ExecuteCheckAsync(CommandContext ctx, bool help)
            => Task.FromResult(true);
    }
}
