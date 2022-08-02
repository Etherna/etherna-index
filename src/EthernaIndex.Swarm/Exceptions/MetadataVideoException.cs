using System;

namespace Etherna.EthernaIndex.Swarm.Exceptions
{
    public class MetadataVideoException : Exception
    {
        public MetadataVideoException()
        {
        }

        public MetadataVideoException(string message) : base(message)
        {
        }

        public MetadataVideoException(string message, Exception innerException) : base(message, innerException)
        {
        }

    }
}
