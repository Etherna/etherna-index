using Etherna.MongODM.Attributes;
using System;

namespace Etherna.EthernaIndex.Domain.Models
{
    public class Video : EntityModelBase<string>
    {
        // Constructors and dispose.
        public Video(string description, TimeSpan length, Channel ownerChannel, string thumbnailHash, string title, string videoHash)
        {
            SetDescription(description);
            Length = length;
            OwnerChannel = ownerChannel ?? throw new ArgumentNullException(nameof(ownerChannel));
            ThumbnailHash = thumbnailHash;
            SetTitle(title);
            VideoHash = videoHash ?? throw new ArgumentNullException(nameof(videoHash));

            OwnerChannel.AddVideo(this);
        }
        protected Video() { }

        public override void DisposeForDelete()
        {
            //owner channel
            OwnerChannel.RemoveVideo(this);

            base.DisposeForDelete();
        }

        // Properties.
        public virtual string Description { get; protected set; } = default!;
        public virtual Channel OwnerChannel { get; protected set; } = default!;
        public virtual TimeSpan Length { get; protected set; }
        public virtual string Title { get; protected set; } = default!;
        public virtual string? ThumbnailHash { get; set; }
        public virtual string VideoHash { get; protected set; } = default!;

        // Methods.
        [PropertyAlterer(nameof(Description))]
        public virtual void SetDescription(string? description)
        {
            description ??= "";
            description = description.Trim();

            Description = description;
        }

        [PropertyAlterer(nameof(Title))]
        public virtual void SetTitle(string title)
        {
            if (string.IsNullOrWhiteSpace(title))
                throw new ArgumentException("Value can't be empty string", nameof(title));
            
            Title = title.Trim();
        }
    }
}
