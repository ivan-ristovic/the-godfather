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
    }

    public class CommandFailedException : Exception
    {
        public CommandFailedException() : base()
        {

        }

        public CommandFailedException(string m) : base(m)
        {

        }
    }
}
