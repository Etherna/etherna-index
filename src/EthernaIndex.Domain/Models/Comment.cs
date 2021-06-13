using System;

namespace Etherna.EthernaIndex.Domain.Models
{
    public class Comment : EntityModelBase<string>
    {
        // Constructors.
        public Comment(
            User owner,
            string text,
            Video video)
        {
            Owner = owner ?? throw new ArgumentNullException(nameof(owner));
            Text = text ?? throw new ArgumentNullException(nameof(text));
            Video = video ?? throw new ArgumentNullException(nameof(video));
        }
        protected Comment() { }

        // Properties.
        public virtual User Owner { get; protected set; } = default!;
        public virtual string Text { get; protected set; } = default!;
        public virtual Video Video { get; protected set; } = default!;
    }
}
