using Digicando.DomainHelper.Attributes;
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
            SetThumbnailHash(thumbnailHash);
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
        public virtual string Description { get; protected set; }
        public virtual Channel OwnerChannel { get; protected set; }
        public virtual TimeSpan Length { get; protected set; }
        public virtual string Title { get; protected set; }
        public virtual string ThumbnailHash { get; protected set; }
        public virtual string VideoHash { get; protected set; }

        // Methods.
        [PropertyAlterer(nameof(Description))]
        public virtual void SetDescription(string description)
        {
            description ??= "";
            description = description.Trim();

            Description = description;
        }

        [PropertyAlterer(nameof(ThumbnailHash))]
        public virtual void SetThumbnailHash(string hash) =>
            ThumbnailHash = hash;

        [PropertyAlterer(nameof(Title))]
        public virtual void SetTitle(string title)
        {
            title = title.Trim();
            if (string.IsNullOrEmpty(title))
                throw new ArgumentException("Value can't be empty string", nameof(title));
            
            Title = title;
        }
    }
}
