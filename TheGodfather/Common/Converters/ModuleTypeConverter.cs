using System.Text.RegularExpressions;

namespace TheGodfather.Common.Converters;

public sealed class ModuleTypeConverter : BaseArgumentConverter<ModuleType>
{
    private static readonly Regex _adminRegex;
    private static readonly Regex _chickensRegex;
    private static readonly Regex _currencyRegex;
    private static readonly Regex _gamesRegex;
    private static readonly Regex _miscRegex;
    private static readonly Regex _musicRegex;
    private static readonly Regex _ownerRegex;
    private static readonly Regex _pollsRegex;
    private static readonly Regex _reactionsRegex;
    private static readonly Regex _remindersRegex;
    private static readonly Regex _searchRegex;
    private static readonly Regex _uncategorizedRegex;


    static ModuleTypeConverter()
    {
        _adminRegex = new Regex(@"^admin(istration)?$", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        _chickensRegex = new Regex(@"^chickens?$", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        _currencyRegex = new Regex(@"^currency|money|bank$", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        _gamesRegex = new Regex(@"^gam(es?|ing)$", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        _miscRegex = new Regex(@"^misc(el+aneous)?$", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        _musicRegex = new Regex(@"^music|tunes$", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        _ownerRegex = new Regex(@"^owner$", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        _pollsRegex = new Regex(@"^poll(s|ing)??$", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        _reactionsRegex = new Regex(@"^react(ing|ions)?$", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        _remindersRegex = new Regex(@"^remind(ing|ers?)?|todo$", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        _searchRegex = new Regex(@"^search(es|ing)?$", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        _uncategorizedRegex = new Regex(@"^un(known|categori[sz]ed)|none$", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    }


    public override bool TryConvert(string value, out ModuleType result)
    {
        result = ModuleType.Uncategorized;
        bool parses = true;

        if (_adminRegex.IsMatch(value))
            result = ModuleType.Administration;
        else if (_chickensRegex.IsMatch(value))
            result = ModuleType.Chickens;
        else if (_currencyRegex.IsMatch(value))
            result = ModuleType.Currency;
        else if (_gamesRegex.IsMatch(value))
            result = ModuleType.Games;
        else if (_miscRegex.IsMatch(value))
            result = ModuleType.Misc;
        else if (_musicRegex.IsMatch(value))
            result = ModuleType.Music;
        else if (_ownerRegex.IsMatch(value))
            result = ModuleType.Owner;
        else if (_pollsRegex.IsMatch(value))
            result = ModuleType.Polls;
        else if (_reactionsRegex.IsMatch(value))
            result = ModuleType.Reactions;
        else if (_remindersRegex.IsMatch(value))
            result = ModuleType.Reminders;
        else if (_searchRegex.IsMatch(value))
            result = ModuleType.Searches;
        else if (_uncategorizedRegex.IsMatch(value))
            result = ModuleType.Uncategorized;
        else
            parses = false;

        return parses;
    }
}