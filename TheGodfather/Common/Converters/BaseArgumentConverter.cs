using System;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Converters;
using DSharpPlus.Entities;

namespace TheGodfather.Common.Converters
{
    public abstract class BaseArgumentConverter<T> : IArgumentConverter<T>
    {
        public abstract bool TryConvert(string value, out T result);


        public Task<Optional<T>> ConvertAsync(string value, CommandContext ctx)
            => this.TryConvert(value, out T result) ? Task.FromResult(new Optional<T>(result)) : Task.FromResult(new Optional<T>(result));
    }
}
