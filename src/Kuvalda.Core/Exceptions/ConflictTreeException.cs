using System;

namespace Kuvalda.Core.Exceptions
{
    public class ConflictTreeException : Exception
    {
        public ConflictTreeException(string message) : base(message)
        {
        }
    }
}