using System;

namespace TheGodfatherBot.Exceptions
{
    public class CommandUsageException : ArgumentException
    {
        public CommandUsageException(string m) : base(m)
        {
            
        }
    }

    public class CommandFailedException : Exception
    {
        public CommandFailedException(string m) : base(m)
        {

        }
    }
}
