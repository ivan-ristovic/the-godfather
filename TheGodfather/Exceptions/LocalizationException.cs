using System;

namespace TheGodfather.Exceptions
{
    public class LocalizationException : Exception
    {
        public LocalizationException()
            : base()
        {

        }

        public LocalizationException(string message)
            : base(message)
        {

        }

        public LocalizationException(string message, Exception inner)
            : base(message, inner)
        {

        }
    }
}
