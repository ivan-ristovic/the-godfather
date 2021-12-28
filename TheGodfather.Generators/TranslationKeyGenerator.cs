using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Newtonsoft.Json;

namespace TheGodfather.Generators;

[Generator]
public class TranslationKeyGenerator : ISourceGenerator
{
    private static readonly Regex _placeholderRegex = new(@"{(?<num>\d)[}:]", RegexOptions.Compiled);

    private static string SanitizeName(string name)
        => name.Replace("-", "_");


    public void Initialize(GeneratorInitializationContext context) { }

    public void Execute(GeneratorExecutionContext context)
    {
        AdditionalText defLocaleFile = context.AdditionalFiles.First(f => f.Path.EndsWith("en-GB.json"));
        string defLocaleContents = defLocaleFile.GetText()?.ToString()
                                   ?? throw new FileNotFoundException("Default locale en-GB not found.");
        Dictionary<string, string> properties =
            JsonConvert.DeserializeObject<Dictionary<string, string>>(defLocaleContents)
            ?? throw new JsonSerializationException();

        using var stringWriter = new StringWriter();
        using var sw = new IndentedTextWriter(stringWriter);

        sw.WriteLine(@"#nullable enable

using System;
        
namespace TheGodfather.Translations;

public readonly partial struct TranslationKey
{
    public static readonly TranslationKey NotFound = new(""str-404"");
        
    public readonly string Key;
    public readonly object?[] Params;
        
    public TranslationKey(string key, params object?[] @params)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentNullException(nameof(key));
        this.Key = key;
        Params = @params;
    }

");
        sw.Indent++;

        foreach (KeyValuePair<string, string> property in properties) {
            MatchCollection matches = _placeholderRegex.Matches(property.Value);

            int argc = 0;
            foreach (Match m in matches) argc = Math.Max(argc, int.Parse(m.Groups["num"].Value) + 1);

            var typedParamStrings = new List<string>();
            string paramStrings = string.Empty;
            for (int i = 0; i < argc; i++) {
                typedParamStrings.Add($"object? p{i}");
                paramStrings += $", p{i}";
            }

            string args = string.Empty;
            if (argc > 0)
                args = $"({string.Join(", ", typedParamStrings)})";

            if (property.Key.StartsWith("desc-"))
                sw.WriteLine($"public const string {SanitizeName(property.Key)} = \"{property.Key}\";");
            else
                sw.WriteLine(
                    $"public static TranslationKey {SanitizeName(property.Key)}{args} => new(\"{property.Key}\"{paramStrings});");
        }

        sw.Indent--;
        sw.WriteLine("}");
        sw.Flush();

        string src = stringWriter.ToString();
        Console.WriteLine(src);
        context.AddSource("TranslationKey.g.cs", src);
    }
}