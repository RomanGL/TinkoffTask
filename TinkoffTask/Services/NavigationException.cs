using System;

namespace TinkoffTask.Services
{
    public sealed class NavigationException : Exception
    {
        public NavigationException()
        {
        }

        public NavigationException(string message) 
            : base(message)
        {
        }
    }
}
