using System;

namespace TheGodfather.Exceptions
{
    public sealed class CommandCancelledException : Exception
    {
        public CommandCancelledException()
            : base() { }

        public CommandCancelledException(string message)
            : base(message) { }

        public CommandCancelledException(string message, Exception inner)
            : base(message, inner) { }
    }
}
