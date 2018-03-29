using System;

namespace TheGodfather.Exceptions
{
    internal class InvalidCommandUsageException : ArgumentException
    {
        public InvalidCommandUsageException() : base() { }

        public InvalidCommandUsageException(string message) : base(message) { }

        public InvalidCommandUsageException(string message, Exception inner) : base(message, inner) { }
    }
}
