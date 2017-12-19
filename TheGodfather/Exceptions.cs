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

    public class DatabaseServiceException : Exception
    {
        public DatabaseServiceException() : base()
        {

        }

        public DatabaseServiceException(string message) : base(message)
        {

        }
        
        public DatabaseServiceException(Exception inner) : base("Database operation failed!", inner)
        {

        }

        public DatabaseServiceException(string message, Exception inner) : base(message, inner)
        {

        }
    }
}
