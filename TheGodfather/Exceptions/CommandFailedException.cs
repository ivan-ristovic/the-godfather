#region USING_DIRECTIVES
using System;
#endregion

namespace TheGodfather.Exceptions
{
    internal class CommandFailedException : Exception
    {
        public CommandFailedException()
            : base()
        {

        }

        public CommandFailedException(string message)
            : base(message)
        {

        }

        public CommandFailedException(string message, Exception inner) 
            : base(message, inner)
        {

        }
    }
}
