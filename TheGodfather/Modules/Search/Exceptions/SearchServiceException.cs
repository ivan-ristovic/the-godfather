using System;

namespace TheGodfather.Modules.Search.Exceptions
{
    public sealed class SearchServiceException<T> : Exception
    {
        public T Details { get; set; }


        public SearchServiceException(string message, T details)
            : base(message) 
        {
            this.Details = details;
        }

        public SearchServiceException(string message, Exception inner, T details)
            : base(message, inner)
        {
            this.Details = details;
        }
    }
}
