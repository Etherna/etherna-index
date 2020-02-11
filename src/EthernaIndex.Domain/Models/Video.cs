using System;

namespace Etherna.EthernaIndex.Domain.Models
{
    public class Video : EntityModelBase<string>
    {
        // Constructors.
        public Video(string hash)
        {
            Hash = hash ?? throw new ArgumentNullException(nameof(hash));
        }

        // Properties.
        public string Hash { get; protected set; }
    }
}
