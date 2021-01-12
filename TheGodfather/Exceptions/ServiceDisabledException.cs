using DSharpPlus.CommandsNext;

namespace TheGodfather.Exceptions
{
    public class ServiceDisabledException : LocalizedException
    {
        public ServiceDisabledException(CommandContext ctx)
            : base(ctx, "err-service-disabled")
        {

        }
    }
}
