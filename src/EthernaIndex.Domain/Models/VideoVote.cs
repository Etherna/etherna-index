using System;

namespace Etherna.EthernaIndex.Domain.Models
{
    public enum VoteValue { Up, Down, Neutral }

    public class VideoVote : EntityModelBase<string>
    {
        // Contructors.
        public VideoVote(
            User owner,
            Video video,
            VoteValue value)
        {
            Owner = owner ?? throw new ArgumentNullException(nameof(owner));
            Value = value;
            Video = video ?? throw new ArgumentNullException(nameof(video));
        }
        protected VideoVote() { }

        // Properties.
        public User Owner { get; protected set; } = default!;
        public VoteValue Value { get; protected set; }
        public Video Video { get; protected set; } = default!;
    }
}
