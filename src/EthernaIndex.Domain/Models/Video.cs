using Etherna.EthernaIndex.Domain.Models.Swarm;
using Etherna.MongODM.Attributes;
using System;
using System.Text.RegularExpressions;

namespace Etherna.EthernaIndex.Domain.Models
{
    public class Video : EntityModelBase<string>
    {
        // Constructors and dispose.
        public Video(
            string? encryptionKey,
            EncryptionType encryptionType,
            SwarmContentHash manifestHash,
            Channel ownerChannel)
        {
            SetEncryptionKey(encryptionKey, encryptionType);
            ManifestHash = manifestHash ?? throw new ArgumentNullException(nameof(manifestHash));
            OwnerChannel = ownerChannel ?? throw new ArgumentNullException(nameof(ownerChannel));
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
        public virtual string? EncryptionKey { get; protected set; }
        public virtual EncryptionType EncryptionType { get; protected set; }
        public virtual SwarmContentHash ManifestHash { get; protected set; } = default!;
        public virtual Channel OwnerChannel { get; protected set; } = default!;

        // Methods.
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
    }
}
