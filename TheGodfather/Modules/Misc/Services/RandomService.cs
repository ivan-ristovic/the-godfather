﻿using System.Collections.Immutable;
using System.IO;
using System.Text;
using Newtonsoft.Json;

namespace TheGodfather.Modules.Misc.Services;

public sealed class RandomService : ITheGodfatherService
{
    private static readonly ImmutableArray<TranslationKey> _regularAnswers = new[] {
        TranslationKey.eightball_00,
        TranslationKey.eightball_01,
        TranslationKey.eightball_02, 
        TranslationKey.eightball_03, 
        TranslationKey.eightball_04, 
        TranslationKey.eightball_05,
        TranslationKey.eightball_06, 
        TranslationKey.eightball_07, 
        TranslationKey.eightball_08, 
        TranslationKey.eightball_09, 
        TranslationKey.eightball_10, 
        TranslationKey.eightball_11,
        TranslationKey.eightball_12, 
        TranslationKey.eightball_13,
    }.ToImmutableArray();

    private static readonly ImmutableArray<TranslationKey> _timeAnswers = new[] {
        TranslationKey.eightball_t_00,
        TranslationKey.eightball_t_01,
        TranslationKey.eightball_t_02,
        TranslationKey.eightball_t_03,
        TranslationKey.eightball_t_04,
        TranslationKey.eightball_t_05,
        TranslationKey.eightball_t_06,
        TranslationKey.eightball_t_07,
        TranslationKey.eightball_t_08,
        TranslationKey.eightball_t_09,
        TranslationKey.eightball_t_10,
    }.ToImmutableArray();

    private static readonly ImmutableArray<TranslationKey> _quantityAnswers = new[] {
        TranslationKey.eightball_q_00,
        TranslationKey.eightball_q_01,
        TranslationKey.eightball_q_02,
        TranslationKey.eightball_q_03,
        TranslationKey.eightball_q_04,
        TranslationKey.eightball_q_05,
        TranslationKey.eightball_q_06,
        TranslationKey.eightball_q_07,
        TranslationKey.eightball_q_08,
    }.ToImmutableArray();

    public bool IsDisabled => false;

    private readonly SecureRandom rng;
    private readonly ImmutableArray<string> colors; 
    private ImmutableDictionary<char, string> leetAlphabet;


    public RandomService(bool loadData = true)
    {
        this.rng = new SecureRandom();
        this.colors = new[] {
            "4b3932", "3c2e28", "f0b8a0", "593b2b", "e9b78a", "c5845c", "ffc3aa",
            "8d5524", "c68642", "ffc3aa", "e0ac69", "ffe5c8", "ffdabe", "ffceb4",
        }.ToImmutableArray();
        this.leetAlphabet = new Dictionary<char, string> {
            { 'i' , "i1" },
            { 'l' , "l1" },
            { 'e' , "e3" },
            { 'a' , "@4" },
            { 't' , "t7" },
            { 'o' , "o0" },
            { 's' , "s5" }
        }.ToImmutableDictionary();
        if (loadData)
            this.TryLoadData("Resources");
    }


    public void TryLoadData(string path)
    {
        string alphabetPath = Path.Combine(path, "leet_alphabet.json");
        try {
            Log.Debug("Loading leet alphabet from {Path}", alphabetPath);
            string json = File.ReadAllText(alphabetPath, Encoding.UTF8);
            Dictionary<char, string>? loadedLeetAlphabet = JsonConvert.DeserializeObject<Dictionary<char, string>>(json);
            if (loadedLeetAlphabet is null)
                throw new JsonSerializationException();
            this.leetAlphabet = loadedLeetAlphabet.ToImmutableDictionary();
        } catch (Exception e) {
            Log.Error(e, "Failed to load leet alphabet, path: {Path}", alphabetPath);
            throw;
        }
    }

    public bool Coinflip(int trueRatio = 1)
        => this.rng.NextBool(trueRatio);

    public int Dice(int sides = 6)
        => this.rng.Next(sides) + 1;

    public string ToLeet(string text)
    {
        var sb = new StringBuilder();
        foreach (char c in text) {
            char code = this.rng.NextBool() ? char.ToUpperInvariant(c) : char.ToLowerInvariant(c);
            if (this.rng.NextBool() && this.leetAlphabet.TryGetValue(code, out string? codes))
                code = this.rng.ChooseRandomChar(codes);
            sb.Append(code);
        }
        return sb.ToString();
    }

    public string FromLeet(string leet)
    {
        var sb = new StringBuilder();
        foreach (char c in leet.ToLowerInvariant()) {
            KeyValuePair<char, string> match = this.leetAlphabet.FirstOrDefault(kvp => kvp.Value.Contains(c));
            sb.Append(match.Key != '\0' ? match.Key : c);
        }
        return sb.ToString().Humanize(LetterCasing.Sentence);
    }

    public Uri Color(ulong uid)
        => new Uri($"https://www.color-hex.com/color/{this.colors[(int)(uid % (ulong)this.colors.Length)]}");

    public string Size(ulong uid)
        => $"8{new string('=', (int)(uid % 40))}D";

    public string Choice(string optionStr)
    {
        IEnumerable<string> options = optionStr
            .Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
            .Distinct();
        return this.rng.ChooseRandomElement(options);
    }

    public TranslationKey GetRandomYesNoAnswer()
        => this.rng.ChooseRandomElement(_regularAnswers);

    public TranslationKey GetRandomTimeAnswer()
        => this.rng.ChooseRandomElement(_timeAnswers);

    public TranslationKey GetRandomQuantityAnswer()
        => this.rng.ChooseRandomElement(_quantityAnswers);
}