using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.CodeAnalysis.Scripting;
using TheGodfather.Modules.Owner.Common;

namespace TheGodfather.Modules.Owner.Services
{
    public static class CSharpCompilationService
    {
        private static readonly Regex _cbRegex = new Regex(@"```(cs\n)?(?<code>[^`]+)```", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static ImmutableArray<string> _usings = new[] {
            "System", "System.Collections.Generic", "System.Linq", "System.Text",
            "System.Threading.Tasks", "DSharpPlus", "DSharpPlus.Entities",
            "DSharpPlus.CommandsNext", "DSharpPlus.CommandsNext.Attributes", "DSharpPlus.Interactivity",
            "TheGodfather.Modules", "TheGodfather.Exceptions", "TheGodfather.Attributes"
        }.ToImmutableArray();

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
                usings: _usings,
                optimizationLevel: OptimizationLevel.Release,
                allowUnsafe: true,
                platform: Platform.AnyCpu
            );

            var compilation = CSharpCompilation.CreateScriptCompilation(type, ast, refs, opts, returnType: typeof(object));

            Assembly? assembly = null;
            using (var ms = new MemoryStream()) {
                compilation.Emit(ms);
                ms.Position = 0;
                assembly = Assembly.Load(ms.ToArray());
            }

            Type? outerType = assembly.ExportedTypes.FirstOrDefault(x => x.Name == type);
            moduleType = outerType?.GetNestedTypes().FirstOrDefault(x => x.BaseType == typeof(TheGodfatherModule));

            return moduleType;
        }

        public static Script<object>? Compile(string code, out ImmutableArray<Diagnostic> diagnostics, out Stopwatch compileTime)
        {
            diagnostics = Array.Empty<Diagnostic>().ToImmutableArray();
            compileTime = new Stopwatch();

            Match m = _cbRegex.Match(code);
            if (!m.Success)
                return null;

            code = m.Groups["code"].ToString();

            ScriptOptions sopts = ScriptOptions.Default
                .WithImports(_usings)
                .WithReferences(AppDomain.CurrentDomain.GetAssemblies().Where(xa => !xa.IsDynamic && !string.IsNullOrWhiteSpace(xa.Location)));

            compileTime = Stopwatch.StartNew();
            Script<object> snippet = CSharpScript.Create(code, sopts, typeof(EvaluationEnvironment));
            diagnostics = snippet.Compile();
            compileTime.Stop();

            return snippet;
        }
    }
}
