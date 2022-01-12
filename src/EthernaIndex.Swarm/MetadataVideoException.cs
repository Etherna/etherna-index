using System;

namespace EthernaIndex.Swarm
{
    public class MetadataVideoException : Exception
    {
        public MetadataVideoException(Exception exception)
            : base("Unable to cast json", exception)
        {

        }
    }
}
