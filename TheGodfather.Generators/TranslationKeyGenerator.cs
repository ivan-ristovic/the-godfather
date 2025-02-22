using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Newtonsoft.Json;

namespace TheGodfather.Generators;

[Generator]
public class TranslationKeyGenerator : IIncrementalGenerator
{
    private static readonly Regex _placeholderRegex = new(@"{(?<num>\d)[}:]", RegexOptions.Compiled);

    private static string SanitizeName(string name)
        => name.Replace("-", "_");

    public record LocaleFile(string Path, string? Content);

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var defLocaleFile = context.AdditionalTextsProvider
               .Where(at => at.Path.EndsWith("en-GB.json"))
               .Select((at, c) => new LocaleFile(Path.GetFileNameWithoutExtension(at.Path), at.GetText(c)?.ToString()))
               .Where(f => !string.IsNullOrWhiteSpace(f.Content))
               .Collect()
               ;
        
        var cf = context.CompilationProvider.Combine(defLocaleFile);
            
        context.RegisterSourceOutput(cf, this.Generate);
    }

    public void Generate(SourceProductionContext ctx, (Compilation Compilation, ImmutableArray<LocaleFile> Files) cf)
    {
        if (cf.Files.Length != 1) 
            throw new FileNotFoundException("Default locale en-GB not found or multiple en-GB files found.");
        
        LocaleFile defLocale = cf.Files.First();
        Dictionary<string, string> properties =
            JsonConvert.DeserializeObject<Dictionary<string, string>>(defLocale.Content!)
            ?? throw new JsonSerializationException();

        using var stringWriter = new StringWriter();
        using var sw = new IndentedTextWriter(stringWriter);

        sw.WriteLine("""
                     #nullable enable

                     using System;
                             
                     namespace TheGodfather.Translations;

                     public readonly partial struct TranslationKey
                     {
                         public static readonly TranslationKey NotFound = new("str-404");
                                                  
                         public readonly string Key;
                         public readonly object?[] Params;
                                                  
                         public TranslationKey(string key, params object?[] @params)
                         {
                             if (string.IsNullOrWhiteSpace(key))
                                 throw new ArgumentNullException(nameof(key));
                             this.Key = key;
                             Params = @params;
                         }
                         
                     """);
        sw.Indent++;

        foreach (KeyValuePair<string, string> property in properties) {
            MatchCollection matches = _placeholderRegex.Matches(property.Value);

            int argc = 0;
            foreach (Match m in matches) 
                argc = Math.Max(argc, int.Parse(m.Groups["num"].Value) + 1);

            var typedParamStrings = new List<string>();
            string paramStrings = string.Empty;
            for (int i = 0; i < argc; i++) {
                typedParamStrings.Add($"object? p{i}");
                paramStrings += $", p{i}";
            }

            string args = string.Empty;
            if (argc > 0)
                args = $"({string.Join(", ", typedParamStrings)})";

            sw.WriteLine(property.Key.StartsWith("desc-")
                ? $"public const string {SanitizeName(property.Key)} = \"{property.Key}\";"
                : $"public static TranslationKey {SanitizeName(property.Key)}{args} => new(\"{property.Key}\"{paramStrings});");
        }

        sw.Indent--;
        sw.WriteLine("}");
        sw.Flush();

        ctx.AddSource("TranslationKey.g.cs", stringWriter.ToString());
    }

}