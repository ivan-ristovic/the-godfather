using System;

namespace TheGodfather.Exceptions
{
    internal class DatabaseServiceException : Exception
    {
        public DatabaseServiceException() : base() { }

        public DatabaseServiceException(string message) : base(message) { }

        public DatabaseServiceException(Exception inner) : base("Database operation failed!", inner) { }

        public DatabaseServiceException(string message, Exception inner) : base(message, inner) { }
    }
}
