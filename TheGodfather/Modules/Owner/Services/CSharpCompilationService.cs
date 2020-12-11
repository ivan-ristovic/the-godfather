using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;

namespace TheGodfather.Modules.Owner.Services
{
    public static class CSharpCompilationService
    {
        private static readonly Regex _cbRegex = new Regex(@"```(cs\n)?(?<code>[^`]+)```", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public static Type? CompileCommand(string code)
        {
            Match m = _cbRegex.Match(code);
            if (!m.Success)
                return null;

            code = $@"
                [ModuleLifespan(ModuleLifespan.Transient)]
                public sealed class DynamicCommands : TheGodfatherModule
                {{
                    {m.Groups["code"]}
                }}";

            string type = $"DynamicCommands{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}";
            Type? moduleType = null;

            IEnumerable<PortableExecutableReference> refs = AppDomain.CurrentDomain.GetAssemblies()
                    .Where(xa => !xa.IsDynamic && !string.IsNullOrWhiteSpace(xa.Location))
                    .Select(x => MetadataReference.CreateFromFile(x.Location));

            SyntaxTree ast = SyntaxFactory.ParseSyntaxTree(code, new CSharpParseOptions()
                                          .WithKind(SourceCodeKind.Script)
                                          .WithLanguageVersion(LanguageVersion.Latest));
            var opts = new CSharpCompilationOptions(
                OutputKind.DynamicallyLinkedLibrary,
                scriptClassName: type,
                usings: new[] { "System", "System.Collections.Generic", "System.Linq", "System.Text",
                                "System.Threading.Tasks", "DSharpPlus", "DSharpPlus.Entities",
                                "DSharpPlus.CommandsNext", "DSharpPlus.CommandsNext.Attributes", "DSharpPlus.Interactivity",
                                "TheGodfather.Modules", "TheGodfather.Exceptions", "TheGodfather.Attributes",
                },
                optimizationLevel: OptimizationLevel.Release,
                allowUnsafe: true,
                platform: Platform.AnyCpu
            );

            var compilation = CSharpCompilation.CreateScriptCompilation(type, ast, refs, opts, returnType: typeof(object));

            Assembly? assembly = null;
            using (var ms = new MemoryStream()) {
                EmitResult er = compilation.Emit(ms);
                ms.Position = 0;
                assembly = Assembly.Load(ms.ToArray());
            }

            Type? outerType = assembly.ExportedTypes.FirstOrDefault(x => x.Name == type);
            moduleType = outerType?.GetNestedTypes().FirstOrDefault(x => x.BaseType == typeof(TheGodfatherModule));

            return moduleType;
        }
    }
}
