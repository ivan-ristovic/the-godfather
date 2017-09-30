using System;

namespace TheGodfatherBot.Exceptions
{
    public class InvalidCommandUsageException : ArgumentException
    {
        public InvalidCommandUsageException() : base()
        {

        }

        public InvalidCommandUsageException(string m) : base(m)
        {
            
        }

        public InvalidCommandUsageException(string m, Exception inner) : base(m, inner)
        {

        }
    }

    public class CommandFailedException : Exception
    {
        public CommandFailedException() : base()
        {

        }

        public CommandFailedException(string m) : base(m)
        {

        }

        public CommandFailedException(string m, Exception inner) : base(m, inner)
        {

        }
    }
}
