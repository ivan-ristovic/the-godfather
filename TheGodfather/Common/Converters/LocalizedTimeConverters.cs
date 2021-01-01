using System;
using System.Globalization;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Converters;
using DSharpPlus.Entities;
using Microsoft.Extensions.DependencyInjection;
using TheGodfather.Services;

namespace TheGodfather.Common.Converters
{
    public class DateTimeConverter : IArgumentConverter<DateTime>
    {
        public Task<Optional<DateTime>> ConvertAsync(string value, CommandContext ctx)
        {
            CultureInfo culture = ctx.Services.GetRequiredService<LocalizationService>().GetGuildCulture(ctx.Guild?.Id);
            return DateTime.TryParse(value, culture, DateTimeStyles.AllowWhiteSpaces, out DateTime result)
                ? Task.FromResult(new Optional<DateTime>(result))
                : Task.FromResult(Optional.FromNoValue<DateTime>());
        }
    }

    public class DateTimeOffsetConverter : IArgumentConverter<DateTimeOffset>
    {
        public Task<Optional<DateTimeOffset>> ConvertAsync(string value, CommandContext ctx)
        {
            CultureInfo culture = ctx.Services.GetRequiredService<LocalizationService>().GetGuildCulture(ctx.Guild?.Id);
            return DateTimeOffset.TryParse(value, culture, DateTimeStyles.AllowWhiteSpaces, out DateTimeOffset result)
                ? Task.FromResult(Optional.FromValue(result))
                : Task.FromResult(Optional.FromNoValue<DateTimeOffset>());
        }
    }
}