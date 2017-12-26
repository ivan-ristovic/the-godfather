using System;

namespace TheGodfather.Exceptions
{
    public class InvalidCommandUsageException : ArgumentException
    {
        public InvalidCommandUsageException() : base() { }

        public InvalidCommandUsageException(string message) : base(message) { }

        public InvalidCommandUsageException(string message, Exception inner) : base(message, inner) { }
    }
}
