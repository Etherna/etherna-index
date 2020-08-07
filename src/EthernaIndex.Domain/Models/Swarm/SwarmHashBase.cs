using System;
using System.Text.RegularExpressions;

namespace Etherna.EthernaIndex.Domain.Models.Swarm
{
    public abstract class SwarmHashBase : ModelBase
    {
        // Constructors.
        public SwarmHashBase(
            string hash)
        {
            if (hash is null)
                throw new ArgumentNullException(nameof(hash));
            if (!Regex.IsMatch(hash, "^[A-Fa-f0-9]{64}$"))
                throw new ArgumentException($"{hash} is not a valid swarm hash", nameof(hash));

            Hash = hash;
        }
        protected SwarmHashBase() { }

        // Properties.
        public virtual string Hash { get; protected set; } = default!;
    }
}
