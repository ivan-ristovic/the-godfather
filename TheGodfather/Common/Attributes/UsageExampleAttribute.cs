#region USING_DIRECTIVES
using System;
using System.Linq;
#endregion

namespace TheGodfather.Common.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)] // TODO don't allow multiple
    internal sealed class UsageExampleAttribute : Attribute
    {
        public string[] Examples { get; private set; }


        public UsageExampleAttribute(params string[] examples)
        {
            if (!this.Examples.Any())
                throw new ArgumentException($"No examples provided to {this.GetType().Name}!");
            this.Examples = examples;
        }


        public string JoinExamples(string separator = "\n") 
            => string.Join(separator, this.Examples);
    }
}
