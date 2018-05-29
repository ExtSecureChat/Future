using System;

namespace ExtSecureChat.Future.Exceptions
{
    public class PromiseTimoutException : Exception
    {
        public PromiseTimoutException(string message) : base(message)
        {
        }
    }
}
