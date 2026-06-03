using System.Text.RegularExpressions;
using Discord.Commands;
using Serilog.Events;
using TheGodfather.Attributes;
using TheGodfather.Modules.Search.Common;

namespace TheGodfather.Common.TypeReaders;

public sealed class BoolTypeReader : TypeReader
{
    public override Task<TypeReaderResult> ReadAsync(ICommandContext context, string input, IServiceProvider services)
        => LegacyArgumentParsers.TryParseBool(input, out bool result)
            ? Task.FromResult(TypeReaderResult.FromSuccess(result))
            : Task.FromResult(TypeReaderResult.FromError(CommandError.ParseFailed, "Invalid boolean value."));
}

public sealed class ActivityTypeReader : TypeReader
{
    public override Task<TypeReaderResult> ReadAsync(ICommandContext context, string input, IServiceProvider services)
        => LegacyArgumentParsers.TryParseActivityType(input, out ActivityType result)
            ? Task.FromResult(TypeReaderResult.FromSuccess(result))
            : Task.FromResult(TypeReaderResult.FromError(CommandError.ParseFailed, "Invalid activity type."));
}

public sealed class PunishmentActionTypeReader : TypeReader
{
    public override Task<TypeReaderResult> ReadAsync(ICommandContext context, string input, IServiceProvider services)
        => LegacyArgumentParsers.TryParsePunishmentAction(input, out Punishment.Action result)
            ? Task.FromResult(TypeReaderResult.FromSuccess(result))
            : Task.FromResult(TypeReaderResult.FromError(CommandError.ParseFailed, "Invalid punishment action."));
}

public sealed class FilterActionTypeReader : TypeReader
{
    public override Task<TypeReaderResult> ReadAsync(ICommandContext context, string input, IServiceProvider services)
        => LegacyArgumentParsers.TryParseFilterAction(input, out Filter.Action result)
            ? Task.FromResult(TypeReaderResult.FromSuccess(result))
            : Task.FromResult(TypeReaderResult.FromError(CommandError.ParseFailed, "Invalid filter action."));
}

public sealed class TimeWindowTypeReader : TypeReader
{
    public override Task<TypeReaderResult> ReadAsync(ICommandContext context, string input, IServiceProvider services)
        => LegacyArgumentParsers.TryParseTimeWindow(input, out TimeWindow result)
            ? Task.FromResult(TypeReaderResult.FromSuccess(result))
            : Task.FromResult(TypeReaderResult.FromError(CommandError.ParseFailed, "Invalid time window."));
}

public sealed class LogEventLevelTypeReader : TypeReader
{
    public override Task<TypeReaderResult> ReadAsync(ICommandContext context, string input, IServiceProvider services)
        => LegacyArgumentParsers.TryParseLogEventLevel(input, out LogEventLevel result)
            ? Task.FromResult(TypeReaderResult.FromSuccess(result))
            : Task.FromResult(TypeReaderResult.FromError(CommandError.ParseFailed, "Invalid log level."));
}

public sealed class ModuleTypeTypeReader : TypeReader
{
    public override Task<TypeReaderResult> ReadAsync(ICommandContext context, string input, IServiceProvider services)
        => LegacyArgumentParsers.TryParseModuleType(input, out ModuleType result)
            ? Task.FromResult(TypeReaderResult.FromSuccess(result))
            : Task.FromResult(TypeReaderResult.FromError(CommandError.ParseFailed, "Invalid module type."));
}

public static class LegacyArgumentParsers
{
    private static readonly Regex _trueRegex = new(@"^(y+e*(s*|(a*|u*)(h*|p*))|d+a+(v+a+j+)?|1+|e+n+a+b+l+e|o+n+|t+(r+u+e+)?)$", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static readonly Regex _falseRegex = new(@"^(n+(o*(p+e+)?|a*h*|e*i*|y*e*t*)|0+|d+i+s+a+b+l+e|o+f+|f+(a+l+s+e+)?)$", RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly Regex _listeningRegex = new(@"^l+(i+s+t+e+n+([sz]+|i+n+g+)?)?(to)?$", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static readonly Regex _playingRegex = new(@"^p+(l+a+y+([sz]+|i+n+g+)?)?$", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static readonly Regex _streamingRegex = new(@"^s+(t+r+e+a+m+(e*[sz]+|i+n+g+)?)?$", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static readonly Regex _watchingRegex = new(@"^w+(a+t+c+h+(e*[sz]+|i+n+g+)?)?$", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static readonly Regex _competingRegex = new(@"^c+o+m+p+e+t+i+n+g(\s+i+n+)?$", RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly Regex _permanentMuteRegex = new(@"^(p(erm(a(nent(al+y?)?)?)?)?)?(m+(u+t+e*d*)?)$", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static readonly Regex _temporaryMuteRegex = new(@"^t(e?mp(ora(l|ry))?)?(m+(u+t+e*d*)?)$", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static readonly Regex _permanentBanRegex = new(@"^(p(erm(a(nent(al+y?)?)?)?)?)?(b+([ae]+n+e*d*)?)$", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static readonly Regex _temporaryBanRegex = new(@"^t(e?mp(ora(l|ry))?)?(b+([ae]+n+e*d*)?)$", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static readonly Regex _kickRegex = new(@"^k+(i+c*k+e*d*)?$", RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly Regex _deleteRegex = new(@"^d(el(ete)?)?$", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static readonly Regex _sanitizeRegex = new(@"^(s(anitize|poiler)?|e(dit)?)$", RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly Regex _allRegex = new(@"^a(ll)?$", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static readonly Regex _dayRegex = new(@"^d(ay)?$", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static readonly Regex _weekRegex = new(@"^w(eek)?$", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static readonly Regex _monthRegex = new(@"^m(onth)?$", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static readonly Regex _yearRegex = new(@"^y(ear)?$", RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly Regex _debugRegex = new(@"^d(e?bu?g)?$", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static readonly Regex _errorRegex = new(@"^e(rr(or)?)?", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static readonly Regex _fatalRegex = new(@"^f(a?tal?)?", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static readonly Regex _informationRegex = new(@"^i(nf(ormation)?)?", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static readonly Regex _verboseRegex = new(@"^v(e?rb(ose)?)?$", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static readonly Regex _warningRegex = new(@"^w(a?rn(ing)?)?$", RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly Regex _adminRegex = new(@"^admin(istration)?$", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static readonly Regex _chickensRegex = new(@"^chickens?$", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static readonly Regex _currencyRegex = new(@"^currency|money|bank$", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static readonly Regex _gamesRegex = new(@"^gam(es?|ing)$", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static readonly Regex _miscRegex = new(@"^misc(el+aneous)?$", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static readonly Regex _ownerRegex = new(@"^owner$", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static readonly Regex _pollsRegex = new(@"^poll(s|ing)??$", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static readonly Regex _reactionsRegex = new(@"^react(ing|ions)?$", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static readonly Regex _remindersRegex = new(@"^remind(ing|ers?)?|todo$", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static readonly Regex _searchRegex = new(@"^search(es|ing)?$", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static readonly Regex _uncategorizedRegex = new(@"^un(known|categori[sz]ed)|none$", RegexOptions.IgnoreCase | RegexOptions.Compiled);


    public static bool TryParseBool(string value, out bool result)
    {
        value = value.Trim();
        if (_trueRegex.IsMatch(value)) {
            result = true;
            return true;
        }
        if (_falseRegex.IsMatch(value)) {
            result = false;
            return true;
        }
        return bool.TryParse(value, out result);
    }

    public static bool TryParseActivityType(string value, out ActivityType result)
    {
        result = ActivityType.Playing;
        if (_playingRegex.IsMatch(value)) return true;
        if (_watchingRegex.IsMatch(value)) { result = ActivityType.Watching; return true; }
        if (_listeningRegex.IsMatch(value)) { result = ActivityType.Listening; return true; }
        if (_streamingRegex.IsMatch(value)) { result = ActivityType.Streaming; return true; }
        if (_competingRegex.IsMatch(value)) { result = ActivityType.Competing; return true; }
        return false;
    }

    public static bool TryParsePunishmentAction(string value, out Punishment.Action result)
    {
        result = Punishment.Action.Kick;
        if (_kickRegex.IsMatch(value)) return true;
        if (_temporaryBanRegex.IsMatch(value)) { result = Punishment.Action.TemporaryBan; return true; }
        if (_temporaryMuteRegex.IsMatch(value)) { result = Punishment.Action.TemporaryMute; return true; }
        if (_permanentBanRegex.IsMatch(value)) { result = Punishment.Action.PermanentBan; return true; }
        if (_permanentMuteRegex.IsMatch(value)) { result = Punishment.Action.PermanentMute; return true; }
        return false;
    }

    public static bool TryParseFilterAction(string value, out Filter.Action result)
    {
        result = Filter.Action.Delete;
        if (_deleteRegex.IsMatch(value)) return true;
        if (_sanitizeRegex.IsMatch(value)) { result = Filter.Action.Sanitize; return true; }
        return TryParsePunishmentAction(value, out Punishment.Action punishment)
               && Enum.TryParse(punishment.ToString(), out result);
    }

    public static bool TryParseTimeWindow(string value, out TimeWindow result)
    {
        result = TimeWindow.All;
        if (_dayRegex.IsMatch(value)) { result = TimeWindow.Day; return true; }
        if (_weekRegex.IsMatch(value)) { result = TimeWindow.Week; return true; }
        if (_monthRegex.IsMatch(value)) { result = TimeWindow.Month; return true; }
        if (_yearRegex.IsMatch(value)) { result = TimeWindow.Year; return true; }
        return _allRegex.IsMatch(value);
    }

    public static bool TryParseLogEventLevel(string value, out LogEventLevel result)
    {
        result = LogEventLevel.Information;
        if (_debugRegex.IsMatch(value)) { result = LogEventLevel.Debug; return true; }
        if (_errorRegex.IsMatch(value)) { result = LogEventLevel.Error; return true; }
        if (_fatalRegex.IsMatch(value)) { result = LogEventLevel.Fatal; return true; }
        if (_informationRegex.IsMatch(value)) return true;
        if (_verboseRegex.IsMatch(value)) { result = LogEventLevel.Verbose; return true; }
        if (_warningRegex.IsMatch(value)) { result = LogEventLevel.Warning; return true; }
        return false;
    }

    public static bool TryParseModuleType(string value, out ModuleType result)
    {
        result = ModuleType.Uncategorized;
        if (_adminRegex.IsMatch(value)) { result = ModuleType.Administration; return true; }
        if (_chickensRegex.IsMatch(value)) { result = ModuleType.Chickens; return true; }
        if (_currencyRegex.IsMatch(value)) { result = ModuleType.Currency; return true; }
        if (_gamesRegex.IsMatch(value)) { result = ModuleType.Games; return true; }
        if (_miscRegex.IsMatch(value)) { result = ModuleType.Misc; return true; }
        if (_ownerRegex.IsMatch(value)) { result = ModuleType.Owner; return true; }
        if (_pollsRegex.IsMatch(value)) { result = ModuleType.Polls; return true; }
        if (_reactionsRegex.IsMatch(value)) { result = ModuleType.Reactions; return true; }
        if (_remindersRegex.IsMatch(value)) { result = ModuleType.Reminders; return true; }
        if (_searchRegex.IsMatch(value)) { result = ModuleType.Searches; return true; }
        return _uncategorizedRegex.IsMatch(value);
    }
}
