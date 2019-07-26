using System;
using System.Linq;
using DSharpPlus.CommandsNext;
using Microsoft.Extensions.DependencyInjection;
using TheGodfather.Modules.Administration.Services;

namespace TheGodfather.Common.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
    public sealed class UsageExampleArgsAttribute : Attribute
    {
        public string[] Examples { get; private set; }


        public UsageExampleArgsAttribute(params string[] examples)
        {
            if (examples is null)
                throw new ArgumentException($"No usage examples provided to {this.GetType().Name}!");

            this.Examples = examples
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .Select(s => s.Trim())
                .ToArray();

            if (!this.Examples.Any())
                throw new ArgumentException($"Empty usage examples attribute provided to {this.GetType().Name}!");
        }


        public string JoinExamples(Command cmd, CommandContext ctx = null, string separator = "\n")
        {
            if (ctx is null)
                return string.Join(separator, this.Examples);

            string cname = cmd.QualifiedName;
            string prefix = ctx.Services.GetService<GuildConfigService>().GetGuildPrefix(ctx.Guild.Id);

            if (cmd.Overloads.Any(o => o.Arguments.All(a => a.IsOptional)))
                return string.Join(separator, new[] { "" }.Concat(this.Examples).Select(GenerateExampleString));
            else
                return string.Join(separator, this.Examples.Select(GenerateExampleString));


            string GenerateExampleString(string example)
                => $"{prefix}{cname} {example}";
        }
    }
}
