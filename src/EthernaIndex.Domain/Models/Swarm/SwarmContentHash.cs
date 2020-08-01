namespace Etherna.EthernaIndex.Domain.Models.Swarm
{
    public class SwarmContentHash : SwarmHashBase
    {
        // Constructors.
        public SwarmContentHash(
            string hash,
            bool isRaw)
            : base(hash)
        {
            IsRaw = isRaw;
        }
        protected SwarmContentHash() { }

        // Properties.
        public virtual bool IsRaw { get; protected set; }
    }
}
