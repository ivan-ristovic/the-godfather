using System;

namespace TheGodfather.Exceptions
{
    public class InvalidCommandUsageException : ArgumentException
    {
        public InvalidCommandUsageException() : base()
        {

        }

        public InvalidCommandUsageException(string message) : base(message)
        {
            
        }

        public InvalidCommandUsageException(string message, Exception inner) : base(message, inner)
        {

        }
    }

    public class CommandFailedException : Exception
    {
        public CommandFailedException() : base()
        {

        }

        public CommandFailedException(string message) : base(message)
        {

        }

        public CommandFailedException(string message, Exception inner) : base(message, inner)
        {

        }
    }
}
