using System;
using System.Runtime.Serialization;

namespace Kuvalda.Storage.Http
{
    [Serializable]
    public class HttpSendErrorException : Exception
    {
        public HttpSendErrorException()
        {
        }

        public HttpSendErrorException(string message) : base(message)
        {
        }

        public HttpSendErrorException(string message, Exception inner) : base(message, inner)
        {
        }

        protected HttpSendErrorException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }
    }
}