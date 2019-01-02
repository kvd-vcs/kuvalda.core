using System;

namespace Kuvalda.Core.Exceptions
{
    public class ObjectNotFoundException : Exception
    {
        public readonly string StorageKing;
        public readonly string ObjectKey;

        public ObjectNotFoundException(string storageKing, string objectKey)
        {
            StorageKing = storageKing;
            ObjectKey = objectKey;
        }
    }
}