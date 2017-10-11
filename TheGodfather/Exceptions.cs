using System;

namespace TheGodfather.Exceptions
{
    public class InvalidCommandUsageException : ArgumentException
    {
        public InvalidCommandUsageException() : base()
        {

        }

        public InvalidCommandUsageException(string message) : base(m)
        {
            
        }

        public InvalidCommandUsageException(string message, Exception inner) : base(m, inner)
        {

        }
    }

    public class CommandFailedException : Exception
    {
        public CommandFailedException() : base()
        {

        }

        public CommandFailedException(string message) : base(m)
        {

        }

        public CommandFailedException(string message, Exception inner) : base(m, inner)
        {

        }
    }
}
