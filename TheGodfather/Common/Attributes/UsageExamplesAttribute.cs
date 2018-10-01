#region USING_DIRECTIVES
using System;
using System.Linq;
#endregion

namespace TheGodfather.Common.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
    public sealed class UsageExamplesAttribute : Attribute
    {
        public string[] Examples { get; private set; }


        public UsageExamplesAttribute(params string[] examples)
        {
            if (examples is null)
                throw new ArgumentException($"No examples provided to {this.GetType().Name}!");

            this.Examples = examples
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .Select(s => s.Trim())
                .ToArray();

            if (!this.Examples.Any())
                throw new ArgumentException($"Please provide non-empty examples to {this.GetType().Name}!");
        }


        public string JoinExamples(string separator = "\n") 
            => string.Join(separator, this.Examples);
    }
}
