using Etherna.MongODM.Attributes;
using System;
using System.Text.RegularExpressions;

namespace Etherna.EthernaIndex.Domain.Models
{
    public class Video : EntityModelBase<string>
    {
        // Constructors and dispose.
        public Video(
            string description,
            string? encryptionKey,
            EncryptionType encryptionType,
            TimeSpan length,
            Channel ownerChannel,
            string thumbnailHash,
            bool thumbnailHashIsRaw,
            string title,
            string videoHash,
            bool videoHashIsRaw)
        {
            SetDescription(description);
            SetEncryptionKey(encryptionKey, encryptionType);
            Length = length;
            OwnerChannel = ownerChannel ?? throw new ArgumentNullException(nameof(ownerChannel));
            ThumbnailHash = thumbnailHash;
            ThumbnailHashIsRaw = thumbnailHashIsRaw;
            SetTitle(title);
            VideoHash = videoHash ?? throw new ArgumentNullException(nameof(videoHash));
            VideoHashIsRaw = videoHashIsRaw;
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
        public virtual string? EncryptionKey { get; protected set; }
        public virtual EncryptionType EncryptionType { get; protected set; }
        public virtual TimeSpan Length { get; protected set; }
        public virtual Channel OwnerChannel { get; protected set; } = default!;
        public virtual string? ThumbnailHash { get; set; }
        public virtual bool ThumbnailHashIsRaw { get; protected set; }
        public virtual string Title { get; protected set; } = default!;
        public virtual string VideoHash { get; protected set; } = default!;
        public virtual bool VideoHashIsRaw { get; protected set; }

        // Methods.
        [PropertyAlterer(nameof(Description))]
        public virtual void SetDescription(string? description)
        {
            description ??= "";
            description = description.Trim();

            Description = description;
        }

        [PropertyAlterer(nameof(EncryptionKey))]
        [PropertyAlterer(nameof(EncryptionType))]
        public virtual void SetEncryptionKey(string? encryptionKey, EncryptionType encryptionType)
        {
            switch (encryptionType)
            {
                case EncryptionType.AES256:
                    if (!Regex.IsMatch(encryptionKey, "^[A-Fa-f0-9]{64}$"))
                        throw new ArgumentException($"Encryption key is not a valid {encryptionType} key");
                    break;
                case EncryptionType.Plain:
                    if (!string.IsNullOrEmpty(encryptionKey))
                        throw new ArgumentException($"Encryption key must be empty with unencrypted content");
                    break;
                default:
                    throw new InvalidOperationException();
            }

            EncryptionKey = encryptionKey;
            EncryptionType = encryptionType;
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
