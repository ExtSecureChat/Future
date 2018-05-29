using System;

namespace ExtSecureChat.Future.Exceptions
{
    public class PromiseRejectException : Exception
    {
        public PromiseRejectException(string message) : base(message)
        {
        }
    }
}
