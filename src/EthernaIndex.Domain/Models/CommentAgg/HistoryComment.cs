using System;

namespace Etherna.EthernaIndex.Domain.Models.CommentAgg
{
    public class HistoryComment : EntityModelBase<string>
    {
        // Constructors.
        public HistoryComment(
            User? userModerator,
            string text)
        {
            UserModerator = userModerator;
            Text = text ?? throw new ArgumentNullException(nameof(text));
        }
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        protected HistoryComment() { }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        public virtual User? UserModerator { get; protected set; }
        public virtual string Text { get; protected set; }
    }
}
