using System;

namespace TheGodfather.Exceptions
{
    public sealed class ConcurrentOperationException : Exception
    {
        public ConcurrentOperationException(string message)
            : base(message)
        {

        }

        public ConcurrentOperationException(string message, Exception inner)
            : base(message, inner)
        {
            throw new InvalidOperationException();
        }
    }
}
